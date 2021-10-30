using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpotifyAnalysis.Database
{
    public class SpotifyAnalysisContext : DbContext
    {
        public SpotifyAnalysisContext(DbContextOptions<SpotifyAnalysisContext> options) : base(options)
        {
        }
    }
}