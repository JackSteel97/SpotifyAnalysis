using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SpotifyAnalysis.Database;
using SpotifyAnalysis.Database.Models;
using SpotifyAnalysis.Models.Configuration;
using SpotifyAPI.Web;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpotifyAnalysis.Processing
{
    public class ArtistPublisher : BasePublisher
    {
        private readonly ConcurrentDictionary<string, Artist> _artistsById = new ConcurrentDictionary<string, Artist>();

        public ArtistPublisher(SpotifyAnalysisContext context, AppConfiguration appConfig, ILogger<ArtistPublisher> logger) : base(context, appConfig, logger)
        {
            Initialise();
        }

        public async Task<List<Artist>> Get(IEnumerable<string> ids)
        {
            List<Artist> artists = new List<Artist>();
            foreach (var artistId in ids)
            {
                artists.Add(await Get(artistId));
            }
            return artists;
        }

        public async Task<Artist> Get(string id)
        {
            // Try getting from cache.
            if (_artistsById.TryGetValue(id, out var cachedArtist) && !IsStale(cachedArtist.LastUpdated))
            {
                return cachedArtist;
            }

            var spotifyArtist = await GetFromSpotify(id);
            return _artistsById.AddOrUpdate(spotifyArtist.Id, GetArtist(spotifyArtist), (k, v) => Update(spotifyArtist, v));
        }

        private async Task<FullArtist> GetFromSpotify(string id)
        {
            try
            {
                _logger.LogDebug($"Getting Artist data from spotify for [{id}]");
                return await _spotifyClient.Artists.Get(id);
            }
            catch (APITooManyRequestsException e)
            {
                _logger.LogWarning($"Rate-Limit reached, backing off getting Artist [{id}] for {e.RetryAfter.TotalSeconds} seconds");

                await Task.Delay(e.RetryAfter);
                return await GetFromSpotify(id);
            }
        }

        private static Artist GetArtist(FullArtist spotifyArtist)
        {
            var dbArtist = new Artist()
            {
                SpotifyId = spotifyArtist.Id,
                ImageUrl = spotifyArtist.Images.FirstOrDefault()?.Url,
                Name = spotifyArtist.Name,
                Popularity = spotifyArtist.Popularity,
                LastUpdated = DateTime.UtcNow
            };

            return dbArtist;
        }

        private static Artist Update(FullArtist spotifyArtist, Artist existingArtist)
        {
            existingArtist.LastUpdated = DateTime.UtcNow;
            existingArtist.Popularity = spotifyArtist.Popularity;
            existingArtist.ImageUrl = spotifyArtist.Images.FirstOrDefault()?.Url;

            return existingArtist;
        }

        private void Initialise()
        {
            var existingEntries = _context.Artist.ToList();
            foreach (var entry in existingEntries)
            {
                if(!_artistsById.TryAdd(entry.SpotifyId, entry))
                {
                    throw new InvalidOperationException("Duplicates detected while loading Artists");
                }
            }

            _logger.LogInformation($"Loaded {existingEntries.Count} Artists from the existing database cache.");
        }
    }
}