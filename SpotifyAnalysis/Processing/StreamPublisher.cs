using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SpotifyAnalysis.Database;
using SpotifyAnalysis.Database.Models;
using SpotifyAnalysis.Input.Models;
using SpotifyAnalysis.Models.Configuration;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpotifyAnalysis.Processing
{
    public class StreamPublisher : BasePublisher
    {
        private readonly ConcurrentDictionary<(string, string, string), Stream> _streamCache = new ConcurrentDictionary<(string, string, string), Stream>();
        private readonly TrackPublisher _trackPublisher;
        private readonly ConcurrentBag<Stream> _newStreams = new ConcurrentBag<Stream>();

        public StreamPublisher(SpotifyAnalysisContext context, AppConfiguration appConfig, ILogger<TrackPublisher> logger, TrackPublisher trackPublisher) : base(context, appConfig, logger)
        {
            _trackPublisher = trackPublisher;
            Initialise();
        }

        public async Task Process(EndSongEntry song)
        {
            try
            {
                var trackId = GetSpotifyId(song.TrackUri);
                var key = GetKey(song, trackId);
                if (!_streamCache.ContainsKey(key))
                {
                    Stream stream = await ConvertToStream(song);
                    _streamCache.TryAdd(key, stream);
                }
                else
                {
                    _logger.LogInformation($"Skipping duplicate stream [{song.TrackName} - {song.ArtistName}] at [{song.TrackStopTimestamp.ToString("G")}]");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while processing song [{song.TrackName} - {song.ArtistName}]");
            }
        }

        public async Task SaveChanges()
        {
            foreach (var stream in _streamCache.Values)
            {
                // Not already tracked in db.
                if (stream.Id == 0)
                {
                    _context.Stream.Add(stream);
                }
            }
            await _context.SaveChangesAsync();
        }

        private async Task<Stream> ConvertToStream(EndSongEntry song)
        {
            var trackId = GetSpotifyId(song.TrackUri);

            var result = new Stream
            {
                Username = song.Username,
                DurationMs = song.MsPlayed,
                End = song.TrackStopTimestamp,
                Start = song.TrackStopTimestamp.Subtract(TimeSpan.FromMilliseconds(song.MsPlayed)),
                TrackId = trackId,
                ReasonStart = song.ReasonStart,
                ReasonEnd = song.ReasonEnd,
                Shuffle = song.Shuffle.GetValueOrDefault(),
                IncognitoMode = song.IncognitoMode.GetValueOrDefault(),
                Track = await _trackPublisher.Get(trackId)
            };

            return result;
        }

        private void Initialise()
        {
            var existingEntries = _context.Stream.AsNoTracking().ToList();
            foreach (var entry in existingEntries)
            {
                if (!_streamCache.TryAdd(GetKey(entry), entry))
                {
                    throw new Exception("Duplicates detected");
                }
            }

            _logger.LogInformation($"Loaded {existingEntries.Count} Streams from the existing database cache.");
        }

        private static (string, string, string) GetKey(Stream stream)
        {
            return (stream.TrackId, stream.Username, stream.End.ToString("O"));
        }

        private static (string, string, string) GetKey(EndSongEntry song, string trackId)
        {
            return (trackId, song.Username, song.TrackStopTimestamp.ToString("O"));
        }

        private static string GetSpotifyId(string spotifyUri)
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
