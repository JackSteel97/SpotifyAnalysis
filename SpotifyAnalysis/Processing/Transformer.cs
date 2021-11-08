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
        private readonly SpotifyAnalysisContext _context;
        private readonly ILogger<Transformer> _logger;

        private readonly TrackPublisher _trackPublisher;

        public Transformer(SpotifyAnalysisContext dbContext, TrackPublisher trackPublisher, ILogger<Transformer> logger)
        {
            _context = dbContext;
            _trackPublisher = trackPublisher;

            _logger = logger;
        }

        public async Task Process(List<EndSongEntry> input)
        {
            double saveCounter = 1;
            foreach (var song in input)
            {
                if (!string.IsNullOrWhiteSpace(song.TrackUri))
                {
                    _logger.LogInformation($"Getting detailed data for a stream of [{song.TrackName} - {song.ArtistName}]");
                    await Process(song);
                    _logger.LogInformation($"[{saveCounter}/{input.Count} - {saveCounter / input.Count:P}] Saved stream to database");
                }
                ++saveCounter;
            }
        }

        private async Task Process(EndSongEntry song)
        {
            try
            {
                Stream stream = await ConvertToStream(song);
                _context.Stream.Add(stream);
                await _context.SaveChangesAsync();
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
            result.Track = await _trackPublisher.Get(trackId);

            return result;
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