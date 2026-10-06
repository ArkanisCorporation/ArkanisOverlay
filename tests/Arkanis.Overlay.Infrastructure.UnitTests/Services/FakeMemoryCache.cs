namespace Arkanis.Overlay.Infrastructure.UnitTests.Services;

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;

public class FakeMemoryCache : IMemoryCache
{
    public void Dispose()
        => GC.SuppressFinalize(this);

    public bool TryGetValue(object key, out object? value)
    {
        value = null;
        return false;
    }

    public ICacheEntry CreateEntry(object key)
        => new FakeEntry(key);

    public void Remove(object key)
    {
    }

    private class FakeEntry(object key) : ICacheEntry
    {
        public void Dispose()
        {
        }

        public object Key { get; } = key;
        public object? Value { get; set; }
        public DateTimeOffset? AbsoluteExpiration { get; set; }
        public TimeSpan? AbsoluteExpirationRelativeToNow { get; set; }
        public TimeSpan? SlidingExpiration { get; set; }
        public IList<IChangeToken> ExpirationTokens { get; } = [];
        public IList<PostEvictionCallbackRegistration> PostEvictionCallbacks { get; } = [];
        public CacheItemPriority Priority { get; set; }
        public long? Size { get; set; }
    }
}
