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
        private readonly ConcurrentDictionary<(string, string, DateTime), Stream> _streamsByEndTime = new ConcurrentDictionary<(string, string, DateTime), Stream>();
        private readonly TrackPublisher _trackPublisher;

        public StreamPublisher(SpotifyAnalysisContext context, AppConfiguration appConfig, ILogger<TrackPublisher> logger, TrackPublisher trackPublisher) : base(context, appConfig, logger)
        {
            _trackPublisher = trackPublisher;
            Initialise();
        }

        public async Task Process(EndSongEntry song)
        {
            try
            {
                Stream stream = await ConvertToStream(song);
                if(!_streamsByEndTime.TryAdd(GetKey(stream), stream))
                {
                    _logger.LogInformation("Skipping duplicate stream");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while processing song [{song.TrackName} - {song.ArtistName}]");
            }
        }

        public async Task SaveChanges()
        {
            foreach(var stream in _streamsByEndTime.Values)
            {
                if(stream.Id == 0)
                {
                    _context.Stream.Add(stream);
                }
            }
            await _context.SaveChangesAsync();
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
            result.Track = await _trackPublisher.Get(trackId);

            return result;
        }

        private void Initialise()
        {
            var existingEntries = _context.Stream.Include(x => x.Track).ToList();
            foreach (var entry in existingEntries)
            {
                if(!_streamsByEndTime.TryAdd(GetKey(entry), entry))
                {
                    throw new Exception("Duplicates detected");
                }
            }

            _logger.LogInformation($"Loaded {existingEntries.Count} Streams from the existing database cache.");
        }

        private static (string, string, DateTime) GetKey(Stream stream)
        {
            return (stream.TrackId, stream.Username, stream.End);
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
