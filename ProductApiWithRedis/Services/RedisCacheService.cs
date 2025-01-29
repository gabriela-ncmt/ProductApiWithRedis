using Newtonsoft.Json;
using StackExchange.Redis;

namespace ProductApiWithRedis.Services
{
    public class RedisCacheService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase _database;
        public RedisCacheService(IConnectionMultiplexer redis)
        {
            _redis = redis;
            _database = _redis.GetDatabase();
        }

        /*
         * This method stores a value in Redis
         * by associating it with a specific key.
         * The value is serialized to JSON format before being saved in Redis,
         * to ensure that complex data types can be stored 
         * and retrieved correctly.
         */
        public async Task SetCacheAsync(string key, object value)
        {
            var jsonValue = JsonConvert.SerializeObject(value);
            await _database.StringSetAsync(key, jsonValue);
        }

        /*
         * This method retrieves a value stored in Redis for a given key.
         * It tries to get the key, and if it finds a value,
         * it deserializes it back to the original type (T).
        */

        public async Task<T> GetCacheAsync<T>(string key)
        {
            var jsonValue = await _database.StringGetAsync(key);
            if (jsonValue.IsNullOrEmpty)
                return default;

            return JsonConvert.DeserializeObject<T>(jsonValue);
        }

        /*
         * This method removes a value stored in Redis for a specific key (key).
         * If the key exists in Redis, it is deleted.
        */
        public async Task RemoveCacheAsync(string key)
        {
            await _database.KeyDeleteAsync(key);
        }
    }
}
