using Microsoft.Extensions.Caching.Distributed;

namespace Funda.Infra
{
    public static class CacheExtensions
    {
        public static bool ContainsKey(this IDistributedCache distributedCache, string key) => 
            !string.IsNullOrEmpty(distributedCache.GetString(key));
    }
}
