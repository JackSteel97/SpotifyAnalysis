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

namespace SpotifyAnalysis
{
    public class AnalysisProgram : IHostedService
    {
        private readonly AppConfiguration _appConfig;
        private readonly ILogger<AnalysisProgram> _logger;
        private readonly Transformer _transformer;

        public AnalysisProgram(AppConfiguration appConfig, ILogger<AnalysisProgram> logger, Transformer transformer)
        {
            _appConfig = appConfig;
            _logger = logger;
            _transformer = transformer;
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

            await _transformer.Process(endSongs, _appConfig.SpotifyClientId, _appConfig.SpotifyClientSecret);
        }

        private string GetSpotifyApiAccessToken()
        {
            Console.WriteLine();
            Console.Write("Please enter your Spotify API Access Token: ");
            var token = Console.ReadLine().Trim();
            return token;
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