using StackExchange.Redis;
using System.Text.Json;

namespace infrastructure.caching
{
    public class RedisService : ICacheService
    {
        private readonly IDatabase _db;
        private readonly string _prefix;

        public RedisService(IConnectionMultiplexer redis, string servicePrefix)
        {
            _db = redis.GetDatabase();
            _prefix = servicePrefix;
        }

        private string AddPrefix(string key) => $"{_prefix}:{key}";

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            var json = JsonSerializer.Serialize(value);
            await _db.StringSetAsync(AddPrefix(key), json, expiry);
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            var value = await _db.StringGetAsync(AddPrefix(key));
            if (value.IsNullOrEmpty) return default;
            return JsonSerializer.Deserialize<T>(value!);
        }

        public async Task<bool> RemoveAsync(string key)
        {
            return await _db.KeyDeleteAsync(AddPrefix(key));
        }
    }
}
