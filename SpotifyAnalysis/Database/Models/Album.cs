using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpotifyAnalysis.Database.Models
{
    public class Album
    {
        [MaxLength(100)]
        public string SpotifyId { get; set; }

        [MaxLength(255)]
        public string ImageUrl { get; set; }

        [MaxLength(255)]
        public string Name { get; set; }

        public DateTime ReleaseDate { get; set; }

        public DateTime LastUpdated { get; set; }

        public List<Artist> Artists { get; set; }
        public List<Track> Tracks { get; set; }
    }
}