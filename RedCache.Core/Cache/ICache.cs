using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace RedCache.Core
{

    // Define other methods and classes here
    public interface ICache<TItem>
    {
        TItem GetOrAdd(string key, Func<TItem> item);
        TItem GetOrAdd(string key, Func<TItem> item, DateTime absoluteExpiration);
        TItem GetOrAdd(string key, Func<TItem> item, TimeSpan slidingExpiration, string[] tags);
        TItem GetOrAdd(string key, Func<TItem> item, SqlCacheDependency sqlDependency);

        // Base cache
        void Add(string key, TItem item);
        void Add(string key, TItem item, string[] tags);
        Task AddAsync(string key, TItem item, string[] tags);
        Task AddAsync(string key, TItem item);

        // with absolute expiration
        void Add(string key, TItem item, DateTime absoluteExpiration);
        void Add(string key, TItem item, DateTime absoluteExpiration, string[] tags);
        void Add(string key, TItem item, DateTime absoluteExpiration, Action<string, KeyEvent> callback);
        void Add(string key, TItem item, DateTime absoluteExpiration, string[] tags, Action<string, KeyEvent> callback);

        Task AddAsync(string key, TItem item, DateTime absoluteExpiration);
        Task AddAsync(string key, TItem item, DateTime absoluteExpiration, string[] tags);
        Task AddAsync(string key, TItem item, DateTime absoluteExpiration, Action<string, KeyEvent> callback);
        Task AddAsync(string key, TItem item, DateTime absoluteExpiration, string[] tags, Action<string, KeyEvent> callback);

        // with sliding expiration
        void Add(string key, TItem item, TimeSpan slidingExpiration);
        void Add(string key, TItem item, TimeSpan slidingExpiration, string[] tags);
        void Add(string key, TItem item, TimeSpan slidingExpiration, string[] tags, Action<string, KeyEvent> callback);
        void Add(string key, TItem item, TimeSpan slidingExpiration, Action<string, KeyEvent> callback);

        Task AddAsync(string key, TItem item, TimeSpan slidingExpiration);
        Task AddAsync(string key, TItem item, TimeSpan slidingExpiration, string[] tags);
        Task AddAsync(string key, TItem item, TimeSpan slidingExpiration, string[] tags, Action<string, KeyEvent> callback);
        Task AddAsync(string key, TItem item, TimeSpan slidingExpiration, Action<string, KeyEvent> callback);

        // named dictionary
        void Add(string collectionKey, string itemKey, TItem item);
        void Add(string collectionKey, string itemKey, TItem item, TimeSpan collectionSlidingExpiration);
        void Add(string collectionKey, string itemKey, TItem item, TimeSpan collectionSlidingExpiration, Action<string, KeyEvent> callback);
        void Add(string collectionKey, string itemKey, TItem item, DateTime collectionAbsoluteExpiration);
        void Add(string collectionKey, string itemKey, TItem item, DateTime collectionAbsoluteExpiration, Action<string, KeyEvent> callback);
        void Add(string collectionKey, string itemKey, TItem item, string[] tags);
        void Remove(string collectionKey, string itemKey);
        Dictionary<string, TItem> GetCollection(string collectionKey);
        TItem Get(string collectionKey, string itemKey);


        // with sql dependency
        void Add(string key, TItem item, SqlCacheDependency sqlDependency);
        void Add(string key, TItem item, SqlCacheDependency sqlDependency, string[] tags);

        // Remove
        void Remove(string key);
        void Remove(string[] tags);

        TItem Get(string key);
        Task<TItem> GetAsync(string key);
    }

}
