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
    public class ArtistPublisher : BasePublisher
    {
        private readonly Dictionary<string, Artist> _artistsById = new Dictionary<string, Artist>();

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
            return await AddOrUpdate(spotifyArtist);
        }

        private async Task<FullArtist> GetFromSpotify(string id)
        {
            try
            {
                _logger.LogInformation($"Getting Artist data from spotify for [{id}]");
                return await _spotifyClient.Artists.Get(id);
            }
            catch (APITooManyRequestsException e)
            {
                _logger.LogWarning($"Rate-Limit reached, backing off getting Artist [{id}] for {e.RetryAfter.TotalSeconds} seconds");

                await Task.Delay(e.RetryAfter);
                return await GetFromSpotify(id);
            }
        }

        private async Task<Artist> AddOrUpdate(FullArtist spotifyArtist)
        {
            if (_artistsById.TryGetValue(spotifyArtist.Id, out var existingArtist))
            {
                return await Update(spotifyArtist, existingArtist);
            }
            return await Add(spotifyArtist);
        }

        private async Task<Artist> Add(FullArtist spotifyArtist)
        {
            var dbArtist = new Artist()
            {
                SpotifyId = spotifyArtist.Id,
                ImageUrl = spotifyArtist.Images.FirstOrDefault()?.Url,
                Name = spotifyArtist.Name,
                Popularity = spotifyArtist.Popularity,
                LastUpdated = DateTime.UtcNow
            };

            _context.Artist.Add(dbArtist);
            await _context.SaveChangesAsync();
            _artistsById.Add(dbArtist.SpotifyId, dbArtist);

            return dbArtist;
        }

        private async Task<Artist> Update(FullArtist spotifyArtist, Artist existingArtist)
        {
            existingArtist.LastUpdated = DateTime.UtcNow;
            existingArtist.Popularity = spotifyArtist.Popularity;
            existingArtist.ImageUrl = spotifyArtist.Images.FirstOrDefault()?.Url;

            await _context.SaveChangesAsync();
            return existingArtist;
        }

        private void Initialise()
        {
            var existingEntries = _context.Artist
                .Include(x => x.Albums)
                .Include(x => x.Tracks)
                .ToList();
            foreach (var entry in existingEntries)
            {
                _artistsById.Add(entry.SpotifyId, entry);
            }

            _logger.LogInformation($"Loaded {existingEntries.Count} Artists from the existing database cache.");
        }
    }
}