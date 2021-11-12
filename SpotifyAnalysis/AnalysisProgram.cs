using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SpotifyAnalysis.Input;
using SpotifyAnalysis.Models.Configuration;
using SpotifyAnalysis.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Humanizer;

namespace SpotifyAnalysis
{
    public class AnalysisProgram : IHostedService
    {
        private readonly ILogger<AnalysisProgram> _logger;
        private readonly Transformer _transformer;

        public AnalysisProgram(ILogger<AnalysisProgram> logger, Transformer transformer)
        {
            _logger = logger;
            _transformer = transformer;
        }

        public async Task<TimeSpan> Run()
        {
            Console.WriteLine("Please enter the full path to the end_song.json file(s) wrapped in quotes, and use spaces to separate each filepath: ");
            var entry = Console.ReadLine();
            var filePaths = entry.Split(" ");

            List<string> validPaths = new List<string>();
            foreach (var path in filePaths)
            {
                var trimmedPath = path.Trim('"');
                if (File.Exists(trimmedPath))
                {
                    Console.WriteLine($"Validated [{trimmedPath}]");
                    validPaths.Add(trimmedPath);
                }
                else
                {
                    Console.WriteLine($"[{trimmedPath}] Does not exist, please check your entry and try again.");
                    return TimeSpan.Zero; // exit run.
                }
            }

            var endSongs = JsonReader.ReadStreamingHistory(validPaths);

            DateTime start = DateTime.UtcNow;
            await _transformer.Process(endSongs);
            DateTime end = DateTime.UtcNow;

            return end - start;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting Spotify Analysis Service");
            TimeSpan runningTime = TimeSpan.Zero;
            while (!cancellationToken.IsCancellationRequested)
            {
                if (runningTime.TotalMilliseconds > 0)
                {
                    Console.WriteLine($"Finished processing in {runningTime.Humanize(3)}");
                }
                runningTime = await Run();
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping Spotify Analysis Service");

            return Task.CompletedTask;
        }
    }
}