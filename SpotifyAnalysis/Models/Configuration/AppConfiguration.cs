using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpotifyAnalysis.Models.Configuration
{
    public class AppConfiguration
    {
        public DatabaseConfiguration Database { get; set; }

        public string SpotifyClientId { get; set; }
        public string SpotifyClientSecret { get; set; }
    }
}