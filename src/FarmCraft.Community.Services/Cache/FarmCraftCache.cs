using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace FarmCraft.Community.Services.Cache
{
    public class FarmCraftCache : ICacheService
    {
        private readonly MemoryCache _cache;
        private readonly MemoryCacheEntryOptions _defaultEntryOptions;

        public FarmCraftCache(IOptions<CacheSettings> settings)
        {
            _cache = new MemoryCache(new MemoryCacheOptions
            {
                SizeLimit = 1024
            });

            _defaultEntryOptions = new MemoryCacheEntryOptions()
                .SetSize(1)
                .SetPriority(CacheItemPriority.Normal)
                .SetAbsoluteExpiration(
                    TimeSpan.FromMinutes(settings.Value.DefaultCacheDurationMinutes));
        }

        public object GetAndSetItem(string key, Func<object> setter)
        {
            if(!_cache.TryGetValue(key, out object value)) {
                value = setter();

                _cache.Set(key, value, _defaultEntryOptions);
            }

            return value;
        }

        public object GetAndSetItem(string key, Func<object> setter, TimeSpan validFor)
        {
            if(!_cache.TryGetValue(key, out object value))
            {
                value = setter();
                MemoryCacheEntryOptions options = _defaultEntryOptions;
                options.SetAbsoluteExpiration(validFor);

                _cache.Set(key, value, options);
            }

            return value;
        }

        public object GetItem(string key)
        {
            _cache.TryGetValue(key, out object value);
            return value;
        }

        public void SetItem(string key, object value)
        {
            _cache.Set(key, value, _defaultEntryOptions);
        }

        public void SetItem(string key, object value, TimeSpan validFor)
        {
            MemoryCacheEntryOptions options = _defaultEntryOptions;
            options.SetAbsoluteExpiration(validFor);

            _cache.Set(key, value, options);
        }
    }
}
