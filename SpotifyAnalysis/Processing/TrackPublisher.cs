using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SpotifyAnalysis.Database;
using SpotifyAnalysis.Database.Models;
using SpotifyAnalysis.Models.Configuration;
using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpotifyAnalysis.Processing
{
    public class TrackPublisher : BasePublisher
    {
        private readonly Dictionary<string, Track> _tracksById = new Dictionary<string, Track>();
        private readonly ArtistPublisher _artistPublisher;
        private readonly AlbumPublisher _albumPublisher;

        public TrackPublisher(SpotifyAnalysisContext context, AppConfiguration appConfig, ILogger<TrackPublisher> logger, ArtistPublisher artistPublisher, AlbumPublisher albumPublisher) : base(context, appConfig, logger)
        {
            _artistPublisher = artistPublisher;
            _albumPublisher = albumPublisher;
            Initialise();
        }

        public async Task<Track> Get(string id)
        {
            // Try getting from cache.
            if (_tracksById.TryGetValue(id, out var cachedTrack) && !IsStale(cachedTrack.LastUpdated))
            {
                return cachedTrack;
            }

            var spotifyTrack = await GetFromSpotify(id);
            var spotifyTrackFeatures = await GetFeaturesFromSpotify(id);
            return await AddOrUpdate(spotifyTrack, spotifyTrackFeatures);
        }

        private async Task<FullTrack> GetFromSpotify(string id, int attemptNumber = 1)
        {
            try
            {
                _logger.LogInformation($"Getting Track data from spotify for [{id}]");
                return await _spotifyClient.Tracks.Get(id);
            }
            catch (APITooManyRequestsException e)
            {
                _logger.LogWarning($"Rate-Limit reached, backing off getting Track [{id}] for {e.RetryAfter.TotalSeconds} seconds");

                await Task.Delay(e.RetryAfter);
                return await GetFromSpotify(id, ++attemptNumber);
            }
        }

        private async Task<TrackAudioFeatures> GetFeaturesFromSpotify(string id)
        {
            try
            {
                _logger.LogInformation($"Getting Track Audio Feature data from spotify for [{id}]");
                return await _spotifyClient.Tracks.GetAudioFeatures(id);
            }
            catch (APITooManyRequestsException e)
            {
                _logger.LogWarning($"Rate-Limit reached, backing off getting Track Audio Features [{id}] for {e.RetryAfter.TotalSeconds} seconds");

                await Task.Delay(e.RetryAfter);
                return await GetFeaturesFromSpotify(id);
            }
        }

        private async Task<Track> AddOrUpdate(FullTrack spotifyTrack, TrackAudioFeatures audioFeatures)
        {
            if (_tracksById.TryGetValue(spotifyTrack.Id, out var existingTrack))
            {
                return await Update(spotifyTrack, audioFeatures, existingTrack);
            }
            return await Add(spotifyTrack, audioFeatures);
        }

        private async Task<Track> Add(FullTrack spotifyTrack, TrackAudioFeatures audioFeatures)
        {
            var dbTrack = new Track()
            {
                SpotifyId = spotifyTrack.Id,
                Name = spotifyTrack.Name,
                AlbumId = spotifyTrack.Album.Id,
                TrackLengthMs = spotifyTrack.DurationMs,
                Explicit = spotifyTrack.Explicit,
                PreviewUrl = spotifyTrack.PreviewUrl,

                Acousticness = audioFeatures.Acousticness,
                Danceability = audioFeatures.Danceability,
                Energy = audioFeatures.Energy,
                Instrumentalness = audioFeatures.Instrumentalness,
                Key = (Key)audioFeatures.Key,
                Liveness = audioFeatures.Liveness,
                Loudness = audioFeatures.Loudness,
                Mode = (Mode)audioFeatures.Mode,
                Speechiness = audioFeatures.Speechiness,
                EstimatedTempo = audioFeatures.Tempo,
                TimeSignature = audioFeatures.TimeSignature,
                Valence = audioFeatures.Valence,

                Album = await _albumPublisher.Get(spotifyTrack.Album.Id),
                Artists = await _artistPublisher.Get(spotifyTrack.Artists.Select(x => x.Id)),
                LastUpdated = DateTime.UtcNow
            };

            _context.Track.Add(dbTrack);
            await _context.SaveChangesAsync();
            _tracksById.Add(dbTrack.SpotifyId, dbTrack);

            return dbTrack;
        }

        private async Task<Track> Update(FullTrack spotifyTrack, TrackAudioFeatures audioFeatures, Track existingTrack)
        {
            existingTrack.LastUpdated = DateTime.UtcNow;
            existingTrack.Name = spotifyTrack.Name;
            existingTrack.TrackLengthMs = spotifyTrack.DurationMs;

            existingTrack.Acousticness = audioFeatures.Acousticness;
            existingTrack.Danceability = audioFeatures.Danceability;
            existingTrack.Energy = audioFeatures.Energy;
            existingTrack.Instrumentalness = audioFeatures.Instrumentalness;
            existingTrack.Key = (Key)audioFeatures.Key;
            existingTrack.Liveness = audioFeatures.Liveness;
            existingTrack.Loudness = audioFeatures.Loudness;
            existingTrack.Mode = (Mode)audioFeatures.Mode;
            existingTrack.Speechiness = audioFeatures.Speechiness;
            existingTrack.EstimatedTempo = audioFeatures.Tempo;
            existingTrack.TimeSignature = audioFeatures.TimeSignature;
            existingTrack.Valence = audioFeatures.Valence;

            await _context.SaveChangesAsync();
            return existingTrack;
        }

        private void Initialise()
        {
            var existingEntries = _context.Track
                .Include(x => x.Album)
                .Include(x => x.Artists)
                .ToList();

            foreach (var entry in existingEntries)
            {
                _tracksById.Add(entry.SpotifyId, entry);
            }

            _logger.LogInformation($"Loaded {existingEntries.Count} Tracks from the existing database cache.");
        }
    }
}