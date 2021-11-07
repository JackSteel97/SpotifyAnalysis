using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SpotifyAnalysis.Database;
using SpotifyAnalysis.Database.Models;
using SpotifyAnalysis.Input.Models;
using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpotifyAnalysis.Processing
{
    public class Transformer
    {
        private readonly SpotifyAnalysisContext _dbContext;
        private readonly ILogger<Transformer> _logger;
        private SpotifyClient _spotifyClient;

        public Transformer(SpotifyAnalysisContext dbContext, ILogger<Transformer> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task Process(List<EndSongEntry> input, string clientId, string clientSecret)
        {
            var config = SpotifyClientConfig.CreateDefault().WithAuthenticator(new ClientCredentialsAuthenticator(clientId, clientSecret));
            _spotifyClient = new SpotifyClient(config);
            int saveCounter = 1;
            foreach (var song in input)
            {
                if (!string.IsNullOrWhiteSpace(song.TrackUri))
                {
                    _logger.LogInformation($"Getting detailed data for a stream of [{song.TrackName} - {song.ArtistName}]");
                    await Process(song);
                    _logger.LogInformation($"[{saveCounter}/{input.Count}] Saved stream to database");
                }
                ++saveCounter;
            }
        }

        private async Task Process(EndSongEntry song, int attemptNumber = 1)
        {
            try
            {
                Stream stream = await ConvertToStream(song);
                _dbContext.Stream.Add(stream);
                await _dbContext.SaveChangesAsync();
            }
            catch (APITooManyRequestsException)
            {
                // Backoff
                var backoffDelay = 10_000 * attemptNumber;
                _logger.LogWarning($"Rate-Limit reached, backing off for {backoffDelay / 1000} seconds");
                await Task.Delay(backoffDelay);
                await Process(song, ++attemptNumber);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"An error occurred while processing song [{song.TrackName} - {song.ArtistName}]");
            }
        }

        private async Task<Stream> ConvertToStream(EndSongEntry song)
        {
            var trackId = GetSpotifyId(song.TrackUri);

            var result = new Stream();
            result.Username = song.Username;
            result.DurationMs = song.MsPlayed;
            result.End = song.TrackStopTimestamp;
            result.Start = song.TrackStopTimestamp.Subtract(TimeSpan.FromMilliseconds(song.MsPlayed));
            result.TrackId = trackId;
            result.ReasonStart = song.ReasonStart;
            result.ReasonEnd = song.ReasonEnd;
            result.Shuffle = song.Shuffle.GetValueOrDefault();
            result.IncognitoMode = song.IncognitoMode.GetValueOrDefault();
            result.Track = await GetTrack(trackId);

            return result;
        }

        private async Task<Track> GetTrack(string trackId)
        {
            Track result;

            // Get from db?
            result = await _dbContext.Track.FirstOrDefaultAsync(x => x.SpotifyId == trackId);
            if (result != default && !IsOutOfDate(result.LastUpdated))
            {
                // Already linked in db.
                return null;
            }

            _logger.LogInformation($"Track {trackId} was not found up to date in the database, getting from spotify");
            var track = await _spotifyClient.Tracks.Get(trackId);
            if (track == default)
            {
                return null;
            }
            var features = await _spotifyClient.Tracks.GetAudioFeatures(trackId);

            var newResult = new Track
            {
                SpotifyId = trackId,
                Name = track.Name,
                AlbumId = track.Album.Id,
                TrackLengthMs = track.DurationMs,
                Explicit = track.Explicit,
                PreviewUrl = track.PreviewUrl,
                Acousticness = features.Acousticness,
                Danceability = features.Danceability,
                Energy = features.Energy,
                Instrumentalness = features.Instrumentalness,
                Key = (Key)features.Key,
                Liveness = features.Liveness,
                Loudness = features.Loudness,
                Mode = (Mode)features.Mode,
                Speechiness = features.Speechiness,
                EstimatedTempo = features.Tempo,
                TimeSignature = features.TimeSignature,
                Valence = features.Valence,
                Album = await GetAlbum(track.Album.Id),
                LastUpdated = DateTime.UtcNow
            };

            if (result != null)
            {
                _dbContext.Entry(result).CurrentValues.SetValues(newResult);
                await _dbContext.SaveChangesAsync();
                return null;
            }

            return newResult;
        }

        private async Task<Album> GetAlbum(string albumId)
        {
            Album result;
            // Get from db?
            result = await _dbContext.Album.FirstOrDefaultAsync(x => x.SpotifyId == albumId);
            if (result != default && !IsOutOfDate(result.LastUpdated))
            {
                // Already linked in db.
                return null;
            }

            _logger.LogInformation($"Album {albumId} was not found up to date in the database, getting from spotify");
            var album = await _spotifyClient.Albums.Get(albumId);
            if (album == default)
            {
                return null;
            }

            var newResult = new Album
            {
                SpotifyId = albumId,
                ImageUrl = album.Images.FirstOrDefault()?.Url,
                Name = album.Name,
                ReleaseDate = album.ReleaseDate,
                Artists = await GetArtists(album.Artists.Select(x => x.Id)),
                LastUpdated = DateTime.UtcNow
            };

            if (result != null)
            {
                _dbContext.Entry(result).CurrentValues.SetValues(newResult);
                await _dbContext.SaveChangesAsync();
                return null;
            }

            return newResult;
        }

        private async Task<List<Artist>> GetArtists(IEnumerable<string> artistIds)
        {
            List<Artist> result = new List<Artist>();
            foreach (var artistId in artistIds)
            {
                var artist = await GetArtist(artistId);
                if (artist != default)
                {
                    result.Add(artist);
                }
            }
            return result;
        }

        private async Task<Artist> GetArtist(string artistId)
        {
            Artist result;
            // Get from db?
            result = await _dbContext.Artist.FirstOrDefaultAsync(x => x.SpotifyId == artistId);
            if (result != default && !IsOutOfDate(result.LastUpdated))
            {
                // Already linked in db.
                return null;
            }

            _logger.LogInformation($"Artist {artistId} was not found up to date in the database, getting from spotify");

            var artist = await _spotifyClient.Artists.Get(artistId);

            var newResult = new Artist
            {
                SpotifyId = artist.Id,
                ImageUrl = artist.Images.FirstOrDefault()?.Url,
                Name = artist.Name,
                Popularity = artist.Popularity,
                LastUpdated = DateTime.UtcNow
            };

            if (result != null)
            {
                _dbContext.Entry(result).CurrentValues.SetValues(newResult);
                await _dbContext.SaveChangesAsync();
                return null;
            }

            return newResult;
        }

        private bool IsOutOfDate(DateTime lastUpdated)
        {
            var timeSinceUpdate = DateTime.UtcNow - lastUpdated;
            return timeSinceUpdate >= TimeSpan.FromDays(3);
        }

        private string GetSpotifyId(string spotifyUri)
        {
            var parts = spotifyUri.Split(":");
            if (parts.Length == 3)
            {
                return parts[2];
            }
            else
            {
                throw new InvalidOperationException($"Spotify Uri not in correct format [{spotifyUri}]");
            }
        }
    }
}