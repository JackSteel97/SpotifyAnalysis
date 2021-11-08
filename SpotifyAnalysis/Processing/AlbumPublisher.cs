using Microsoft.EntityFrameworkCore;
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
    public class AlbumPublisher : BasePublisher
    {
        private readonly Dictionary<string, Album> _albumsById = new Dictionary<string, Album>();
        private readonly ArtistPublisher _artistPublisher;

        public AlbumPublisher(SpotifyAnalysisContext context, AppConfiguration appConfig, ILogger<AlbumPublisher> logger, ArtistPublisher artistPublisher) : base(context, appConfig, logger)
        {
            _artistPublisher = artistPublisher;
            Initialise();
        }

        public async Task<Album> Get(string id)
        {
            // Try getting from cache.
            if (_albumsById.TryGetValue(id, out var cachedAlbum) && !IsStale(cachedAlbum.LastUpdated))
            {
                return cachedAlbum;
            }

            var spotifyAlbum = await GetFromSpotify(id);
            return await AddOrUpdate(spotifyAlbum);
        }

        private async Task<FullAlbum> GetFromSpotify(string id)
        {
            try
            {
                _logger.LogInformation($"Getting Album data from spotify for [{id}]");
                return await _spotifyClient.Albums.Get(id);
            }
            catch (APITooManyRequestsException e)
            {
                _logger.LogWarning($"Rate-Limit reached, backing off getting Album [{id}] for {e.RetryAfter.TotalSeconds} seconds");

                await Task.Delay(e.RetryAfter);
                return await GetFromSpotify(id);
            }
        }

        private async Task<Album> AddOrUpdate(FullAlbum spotifyAlbum)
        {
            if (_albumsById.TryGetValue(spotifyAlbum.Id, out var existingAlbum))
            {
                return await Update(spotifyAlbum, existingAlbum);
            }
            return await Add(spotifyAlbum);
        }

        private async Task<Album> Add(FullAlbum spotifyAlbum)
        {
            var dbAlbum = new Album()
            {
                SpotifyId = spotifyAlbum.Id,
                ImageUrl = spotifyAlbum.Images.FirstOrDefault()?.Url,
                Name = spotifyAlbum.Name,
                ReleaseDate = spotifyAlbum.ReleaseDate,
                Artists = await _artistPublisher.Get(spotifyAlbum.Artists.Select(x => x.Id)),
                LastUpdated = DateTime.UtcNow
            };

            _context.Album.Add(dbAlbum);
            await _context.SaveChangesAsync();
            _albumsById.Add(dbAlbum.SpotifyId, dbAlbum);

            return dbAlbum;
        }

        private async Task<Album> Update(FullAlbum spotifyAlbum, Album existingAlbum)
        {
            existingAlbum.LastUpdated = DateTime.UtcNow;
            existingAlbum.ReleaseDate = spotifyAlbum.ReleaseDate;
            existingAlbum.ImageUrl = spotifyAlbum.Images.FirstOrDefault()?.Url;
            existingAlbum.Artists = await _artistPublisher.Get(spotifyAlbum.Artists.Select(x => x.Id));

            await _context.SaveChangesAsync();
            return existingAlbum;
        }

        private void Initialise()
        {
            var existingEntries = _context.Album
                .Include(x => x.Artists)
                .Include(x => x.Tracks)
                .ToList();
            foreach (var entry in existingEntries)
            {
                _albumsById.Add(entry.SpotifyId, entry);
            }

            _logger.LogInformation($"Loaded {existingEntries.Count} Albums from the existing database cache.");
        }
    }
}