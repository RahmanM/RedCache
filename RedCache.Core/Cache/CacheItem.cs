using System;

namespace RedCache.Core
{

    internal class CacheItem<TValue>
    {
        public CacheItem()
        {
            // ONLY to be used by run time serialisation.
        }

        public CacheItem(TValue item)
        {
            Item = item;
        }

        public CacheItem(TValue item, TimeSpan slidingExpiration)
        {
            SlidingExpiration = slidingExpiration;
            Item = item;
        }

        public CacheItem(TValue item, DateTime absoluteExpiration)
        {
            Item = item;
            AbsoluteExpiration = absoluteExpiration;
        }

        public TValue Item { get; set; }
        public TimeSpan SlidingExpiration { get; set; }
        public DateTime AbsoluteExpiration { get; set; }
    }

}
