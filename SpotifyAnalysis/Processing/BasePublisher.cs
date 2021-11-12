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
    public class BasePublisher
    {
        protected readonly SpotifyAnalysisContext _context;
        protected readonly AppConfiguration _appConfig;
        protected readonly ILogger<BasePublisher> _logger;

        protected readonly SpotifyClient _spotifyClient;

        private static readonly TimeSpan _staleTimeout = TimeSpan.FromDays(7);

        public BasePublisher(SpotifyAnalysisContext context, AppConfiguration appConfig, ILogger<BasePublisher> logger)
        {
            _context = context;
            _appConfig = appConfig;
            _logger = logger;

            var config = SpotifyClientConfig.CreateDefault().WithAuthenticator(new ClientCredentialsAuthenticator(appConfig.SpotifyClientId, appConfig.SpotifyClientSecret));
            _spotifyClient = new SpotifyClient(config);
        }

        protected static bool IsStale(DateTime timestamp)
        {
            var timeSince = DateTime.UtcNow - timestamp;
            return timeSince >= _staleTimeout;
        }
    }
}