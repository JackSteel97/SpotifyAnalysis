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
using System.Threading;
using System.Threading.Tasks;

namespace SpotifyAnalysis.Processing
{
    public class Transformer
    {
        private readonly ILogger<Transformer> _logger;
        private readonly StreamPublisher _streamPublisher;


        public Transformer(StreamPublisher streamPublisher, ILogger<Transformer> logger)
        {
            _streamPublisher = streamPublisher;
            _logger = logger;
        }

        public async Task Process(List<EndSongEntry> input)
        {
            int saveCounter = 1;

            var cts = new CancellationTokenSource();
            var ct = cts.Token; 

            var progressTask = Task.Run(async () =>
            {
                
                while (!ct.IsCancellationRequested)
                {
                    _logger.LogInformation($"[{saveCounter}/{input.Count} - {(float)saveCounter / input.Count:P}] Processed");

                    if (!ct.IsCancellationRequested)
                    {
                        await Task.Delay(10_000);
                    }
                }
            });

            await Parallel.ForEachAsync(input, async (song, token) =>
            {
                if (!string.IsNullOrWhiteSpace(song.TrackUri))
                {
                    _logger.LogDebug($"Getting detailed data for a stream of [{song.TrackName} - {song.ArtistName}]");
                    await _streamPublisher.Process(song);
                    _logger.LogDebug($"[{saveCounter}/{input.Count} - {(float)saveCounter / input.Count:P}] Processed");
                }
                Interlocked.Increment(ref saveCounter);
            });


            cts.Cancel();

            _logger.LogInformation("Saving at end of processing...");
            await _streamPublisher.SaveChanges();
            _logger.LogInformation("Finished saving");
        }
    }
}