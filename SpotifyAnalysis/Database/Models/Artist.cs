using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpotifyAnalysis.Database.Models
{
    public class Artist
    {
        [MaxLength(100)]
        public string SpotifyId { get; set; }

        [MaxLength(255)]
        public string ImageUrl { get; set; }

        [MaxLength(255)]
        public string Name { get; set; }

        public int Popularity { get; set; }

        public DateTime LastUpdated { get; set; }

        public List<Album> Albums { get; set; }
        public List<Track> Tracks { get; set; }
    }
}