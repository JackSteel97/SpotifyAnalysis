using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SpotifyAnalysis.Input;
using SpotifyAnalysis.Models.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SpotifyAnalysis
{
    public class AnalysisProgram : IHostedService
    {
        private readonly AppConfiguration _appConfig;
        private readonly ILogger<AnalysisProgram> _logger;

        public AnalysisProgram(AppConfiguration appConfig, ILogger<AnalysisProgram> logger)
        {
            _appConfig = appConfig;
            _logger = logger;
        }

        public async Task Run()
        {
            Console.WriteLine("Please enter the full path to the end_song.json file(s) wrapped in quotes, and use spaces to separate each filepath: ");
            var entry = Console.ReadLine();
            var filePaths = entry.Split(" ");

            List<string> validPaths = new List<string>();
            foreach (var path in filePaths)
            {
                if (File.Exists(path))
                {
                    Console.WriteLine($"Validated [{path}]");
                    validPaths.Add(path);
                }
                else
                {
                    Console.WriteLine($"[{path}] Does not exist, please check your entry and try again.");
                    return; // exit run.
                }
            }

            var endSongs = JsonReader.ReadStreamingHistory(validPaths);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting Spotify Analysis Service");

            while (!cancellationToken.IsCancellationRequested)
            {
                Console.Clear();
                await Run();
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping Spotify Analysis Service");

            return Task.CompletedTask;
        }
    }
}