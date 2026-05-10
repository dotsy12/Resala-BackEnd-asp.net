using BackEnd.Application.Interfaces.Repositories;
using BackEnd.Domain.Entities.Basket;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace BackEnd.Infrastructure.Persistence.Repositories
{
    public class RedisBasketRepository : IBasketRepository
    {
        private static readonly DistributedCacheEntryOptions CacheOptions = new()
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(7)
        };

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly IDistributedCache _cache;

        public RedisBasketRepository(IDistributedCache cache)
            => _cache = cache;

        public async Task<DonorBasket?> GetBasketAsync(string donorId)
        {
            var json = await _cache.GetStringAsync(GetBasketKey(donorId));

            return string.IsNullOrWhiteSpace(json)
                ? null
                : JsonSerializer.Deserialize<DonorBasket>(json, JsonOptions);
        }

        public async Task UpdateBasketAsync(DonorBasket basket)
        {
            var json = JsonSerializer.Serialize(basket, JsonOptions);
            await _cache.SetStringAsync(GetBasketKey(basket.Id), json, CacheOptions);
        }

        public async Task DeleteBasketAsync(string donorId)
            => await _cache.RemoveAsync(GetBasketKey(donorId));

        private static string GetBasketKey(string donorId) => $"basket:{donorId}";
    }
}
