using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpotifyAnalysis.Database.Models
{
    public class Stream
    {
        public long Id { get; set; }

        [MaxLength(100)]
        public string Username { get; set; }

        public DateTime Start { get; set; }

        public DateTime End { get; set; }

        public int DurationMs { get; set; }

        [MaxLength(100)]
        public string TrackId { get; set; }

        [MaxLength(30)]
        public string ReasonStart { get; set; }

        [MaxLength(30)]
        public string ReasonEnd { get; set; }

        public bool Shuffle { get; set; }

        public bool IncognitoMode { get; set; }

        public Track Track { get; set; }
    }
}