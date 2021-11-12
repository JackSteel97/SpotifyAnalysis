using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SpotifyAnalysis.Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpotifyAnalysis.Database
{
    public class SpotifyAnalysisContext : DbContext
    {
        public DbSet<Album> Album { get; set; }

        public DbSet<Artist> Artist { get; set; }

        public DbSet<Track> Track { get; set; }

        public DbSet<Stream> Stream { get; set; }

        public SpotifyAnalysisContext(DbContextOptions<SpotifyAnalysisContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Album>(entity =>
            {
                entity.HasKey(x => x.SpotifyId);
            });

            modelBuilder.Entity<Artist>(entity =>
            {
                entity.HasKey(x => x.SpotifyId);
                entity.HasMany(x => x.Albums).WithMany(x => x.Artists);
            });

            modelBuilder.Entity<Track>(entity =>
            {
                entity.HasKey(x => x.SpotifyId);
                entity.HasOne(x => x.Album).WithMany(x => x.Tracks).HasForeignKey(x => x.AlbumId);
                entity.HasMany(x => x.Artists).WithMany(x => x.Tracks);
            });

            modelBuilder.Entity<Stream>(entity =>
            {
                entity.HasKey(x => new { x.Username, x.TrackId, x.End });

                entity.Property(x => x.Id).ValueGeneratedOnAdd().Metadata.SetAfterSaveBehavior(Microsoft.EntityFrameworkCore.Metadata.PropertySaveBehavior.Throw);
                entity.HasOne(x => x.Track).WithMany(x => x.Streams).HasForeignKey(x => x.TrackId);
            });


            //Always UTC dates.
           var dateTimeConverter = new ValueConverter<DateTime, DateTime>(
               v => v.ToUniversalTime(),
               v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

            var nullableDateTimeConverter = new ValueConverter<DateTime?, DateTime?>(
                v => v.HasValue ? v.Value : v.Value.ToUniversalTime(),
                v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v);

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (entityType.IsKeyless)
                {
                    continue;
                }

                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(DateTime))
                    {
                        property.SetValueConverter(dateTimeConverter);
                    }
                    else if (property.ClrType == typeof(DateTime?))
                    {
                        property.SetValueConverter(nullableDateTimeConverter);
                    }
                }
            }
        }
    }
}