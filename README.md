# RedCache
A caching framework using redis

- enables to cache simple values 
- complex objects
- objects heirrarchy
- enables to create named cache
- sql Dependency caching
- absolute caching
- sliding expirary cache
- cache expiry callback

## public interface

```
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
    
   ``` 
    
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
 
 - Complex object
 
 ```
  var list = new List<User>();
  list.Add(
  new User()
  {
      Id = 1,
      UserName = "rahman"
  }
  );

  list.Add(
  new User()
  {
      Id = 2,
      UserName = "roya"
  }
  );

  var cache = new Cache<List<User>>();
  cache.Add("Users", list, TimeSpan.FromSeconds(3));
 ```
   
 - Sql Dependency
 
 ```
  var cache = new Cache<User>();
  var user = new User()
  {
      Id = 999,
      UserName = "rahman"
  };

  cache.Add("rahman999", user, new SqlCacheDependency("Customer", (key, evt) =>
  {
      Console.WriteLine($"Key {key} was changed in database and captured by sql tracking!");
  }
  ));
 ```
 
 
