using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpotifyAnalysis.Database.Models
{
    public class Track
    {
        [MaxLength(100)]
        public string SpotifyId { get; set; }

        [MaxLength(100)]
        public string AlbumId { get; set; }

        public int TrackLengthMs { get; set; }

        public bool Explicit { get; set; }

        [MaxLength(255)]
        public string PreviewUrl { get; set; }

        public float Acousticness { get; set; }

        public float Danceability { get; set; }

        public float Energy { get; set; }

        public float Instrumentalness { get; set; }

        public Key Key { get; set; }

        public float Liveness { get; set; }

        public float Loudness { get; set; }

        public Mode Mode { get; set; }

        public float Speechiness { get; set; }

        public float EstimatedTempo { get; set; }

        public float TimeSignature { get; set; }

        public float Valence { get; set; }

        public Album Album { get; set; }
        public List<Artist> Artists { get; set; }

        public List<Stream> Streams { get; set; }
    }
}