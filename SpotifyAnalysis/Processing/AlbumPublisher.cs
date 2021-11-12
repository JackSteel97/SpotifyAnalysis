using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SpotifyAnalysis.Database;
using SpotifyAnalysis.Database.Models;
using SpotifyAnalysis.Models.Configuration;
using SpotifyAnalysis.Utilities;
using SpotifyAPI.Web;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpotifyAnalysis.Processing
{
    public class AlbumPublisher : BasePublisher
    {
        private readonly ConcurrentDictionary<string, AsyncLazy<Album>> _albumsById = new ConcurrentDictionary<string, AsyncLazy<Album>>();
        private readonly ArtistPublisher _artistPublisher;

        public AlbumPublisher(SpotifyAnalysisContext context, AppConfiguration appConfig, ILogger<AlbumPublisher> logger, ArtistPublisher artistPublisher) : base(context, appConfig, logger)
        {
            _artistPublisher = artistPublisher;
            Initialise();
        }

        public async Task<Album> Get(string id)
        {
            // Try getting from cache.
            if (_albumsById.TryGetValue(id, out var cachedAlbum) && !IsStale((await cachedAlbum.Value).LastUpdated))
            {
                return await cachedAlbum.Value;
            }

            var spotifyAlbum = await GetFromSpotify(id);

            var albumTask = _albumsById.AddOrUpdate(spotifyAlbum.Id,
                new AsyncLazy<Album>(() => GetAlbum(spotifyAlbum)),
                (k, v) => new AsyncLazy<Album>(() => Update(spotifyAlbum, v)));
            return await albumTask.Value;
        }

        private async Task<FullAlbum> GetFromSpotify(string id)
        {
            try
            {
                _logger.LogDebug($"Getting Album data from spotify for [{id}]");
                return await _spotifyClient.Albums.Get(id);
            }
            catch (APITooManyRequestsException e)
            {
                _logger.LogWarning($"Rate-Limit reached, backing off getting Album [{id}] for {e.RetryAfter.TotalSeconds} seconds");

                await Task.Delay(e.RetryAfter);
                return await GetFromSpotify(id);
            }
        }

        private async Task<Album> GetAlbum(FullAlbum spotifyAlbum)
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

            return dbAlbum;
        }

        private async Task<Album> Update(FullAlbum spotifyAlbum, AsyncLazy<Album> existingAlbumTask)
        {
            var existingAlbum = await existingAlbumTask.Value;
            existingAlbum.LastUpdated = DateTime.UtcNow;
            existingAlbum.ReleaseDate = spotifyAlbum.ReleaseDate;
            existingAlbum.ImageUrl = spotifyAlbum.Images.FirstOrDefault()?.Url;
            existingAlbum.Artists = await _artistPublisher.Get(spotifyAlbum.Artists.Select(x => x.Id));

            return existingAlbum;
        }

        private void Initialise()
        {
            var existingEntries = _context.Album.ToList();
            foreach (var entry in existingEntries)
            {
                if (!_albumsById.TryAdd(entry.SpotifyId, new AsyncLazy<Album>(() => Task.FromResult(entry))))
                {
                    throw new InvalidOperationException("Duplicates detected while loading Albums");
                }
            }

            _logger.LogInformation($"Loaded {existingEntries.Count} Albums from the existing database cache.");
        }
    }
}