# RedCache
A caching framework using redis

- Enables to cache simple values 
- complex objects
- objects heirrarchy
- enables to create named cache
- SQL Dependency caching
- absolute caching
- slicing expirary cache
- cache expiry callback

## public interface

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
    
    
   ## Examples
   
   - Simple types
   
   ```
   var intCache = new Cache<int>();
   int i = 1;
  for (i = 1; i <= TotalCacheItems; i++)
  {
      intCache.Add(i.ToString(), i, TimeSpan.FromSeconds(10));
  }

  i = 1;
  for (i = 1; i <= TotalCacheItems; i++)
  {
      intCache.Get(i.ToString());
  }
   
 ```
   
   
