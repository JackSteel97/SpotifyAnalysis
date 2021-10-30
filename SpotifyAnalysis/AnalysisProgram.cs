using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SpotifyAnalysis.Models.Configuration;
using System;
using System.Collections.Generic;
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

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting Spotify Analysis Service");

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping Spotify Analysis Service");

            return Task.CompletedTask;
        }
    }
}