using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CachingFramework.Redis.Contracts.Providers;
using CachingFramework.Redis.Contracts;
using Sql.ChangeTracking.Common;
using Polly;

namespace RedCache.Core
{

    public class Cache<TValue> : ICache<TValue>, ISqlChangeCallback
    {
        protected readonly ICacheProvider _Cache;
        protected readonly Dictionary<string, Action<string, KeyEvent>> Callbacks = new Dictionary<string, Action<string, KeyEvent>>();
        protected readonly object locker = new object();
        protected IContext _Context;

        #region Constructors

        public Cache()
        {
            _Context = ContextFactory.InitialiseContext();
            _Cache = _Context.Cache;
        }

        public Cache(bool forceNewRedisConnection)
        {
            _Context = ContextFactory.InitialiseContext(forceNewRedisConnection);
            _Cache = _Context.Cache;
        }

        public Cache(string connectionString, bool forceNewRedisConnection)
        {
            _Context = ContextFactory.InitialiseContext(connectionString, forceNewRedisConnection);
            _Cache = _Context.Cache;
        } 

        #endregion

        #region Absolute Expirations
        public void Add(string key, TValue item, DateTime absoluteExpiration)
        {
            Add(key, new CacheItem<TValue>(item, absoluteExpiration));
        }

        public void Add(string key, TValue item, DateTime absoluteExpiration, string[] tags)
        {
            Add(key, new CacheItem<TValue>(item, absoluteExpiration), tags);
        }

        public TValue GetOrAdd(string key, Func<TValue> item)
        {
            return GetOrAdd(key, item, TimeSpan.Zero, DateTime.MinValue, null);
        }

        public TValue GetOrAdd(string key, Func<TValue> item, DateTime absoluteExpiration)
        {
            return GetOrAdd(key, item, TimeSpan.Zero, absoluteExpiration, null);
        }
        #endregion

        #region Sliding Expiration

        public TValue GetOrAdd(string key, Func<TValue> item, TimeSpan slidingExpiration)
        {
            return GetOrAdd(key, item, slidingExpiration, DateTime.MinValue, null);
        }

        public TValue GetOrAdd(string key, Func<TValue> item, TimeSpan slidingExpiration, string[] tags)
        {
            return GetOrAdd(key, item, slidingExpiration, DateTime.MinValue, tags);
        }
        public void Add(string key, TValue item, TimeSpan slidingExpiration, string[] tags)
        {
            Add(key, new CacheItem<TValue>(item, slidingExpiration), tags);
        }

        private void Add(string key, CacheItem<TValue> cacheItem, string[] tags)
        {
            Add(key, cacheItem, tags);
        }

        public TValue Get(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));

            var cacheItem = _Cache.GetObject<CacheItem<TValue>>(key);

            if (cacheItem?.SlidingExpiration > TimeSpan.Zero)
            {
                _Cache.KeyTimeToLive(key, cacheItem.SlidingExpiration);
            }

            return cacheItem != null ? cacheItem.Item : default(TValue);
        }
        
        public void Add(string key, TValue item, TimeSpan slidingExpiration)
        {
            Add(key, new CacheItem<TValue>(item, slidingExpiration));
        }

        #endregion

        #region SQL Dependency

        public TValue GetOrAdd(string key, Func<TValue> item, SqlCacheDependency sqlDependency)
        {
            var itemFromCache = Get(key);
            if (!EqualityComparer<TValue>.Default.Equals(itemFromCache, default(TValue)))
            {
                return itemFromCache;
            }

            var result = item();

            AddWithSqlDependency(key, result, sqlDependency);

            return result;
        }

        public void Add(string key, TValue item, SqlCacheDependency sqlDependency)
        {
            AddWithSqlDependency(key, item, sqlDependency);
        }

        private IChangeTrackingSubscriptions _channel;

        private void AddWithSqlDependency(string key, TValue item, SqlCacheDependency sqlDependency)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            if (sqlDependency == null)
                throw new ArgumentNullException(nameof(sqlDependency));

            _Cache.SetObject(key, new CacheItem<TValue>()
            {
                AbsoluteExpiration = DateTime.MinValue,
                SlidingExpiration = TimeSpan.Zero,
                Item = item
            });

            Policy.Handle<Exception>()
                   .RetryForever()
                   .Execute(() =>
                   {
                       // subscribe to sql dependency
                       _channel = new ChangeTrackingChannelHelper((ISqlChangeCallback)this).OpenChannel();
                       _channel.Subscribe(key, sqlDependency.TableName);
                   }
                );

            // subscribe client for callback
            SubscribeCallback(key, sqlDependency.Callback);
        }

        public void Add(string key, TValue item, SqlCacheDependency sqlDependency, string[] tags)
        {
            AddWithSqlDependency(key, item, sqlDependency);

            _Cache.AddTagsToKey(key, tags);
        }
      

        public void SqlChangedCallback(string table, string key)
        {
            if (!string.IsNullOrEmpty(key))
            {
                _Cache.Remove(key);

                if (_channel != null)
                {
                    _channel.Unsubscribe(key, table);
                }
            }
        }

        #endregion

        #region Add Remove
        
        public void Add(string key, TValue item)
        {
            Add(key, new CacheItem<TValue>(item));
        }

        public void Add(string key, TValue item, string[] tags)
        {
            Add(key, new CacheItem<TValue>(item), tags);
        }

        public void Remove(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));

            _Cache.Remove(key);
        }

        public void Remove(string[] tags)
        {
            if (tags == null) throw new ArgumentNullException(nameof(tags));

            _Cache.InvalidateKeysByTag(tags);
        } 
        #endregion

        #region Callbacks
        public void Add(string key, TValue item, DateTime absoluteExpiration, Action<string, KeyEvent> callback)
        {
            Add(key, new CacheItem<TValue>(item, absoluteExpiration));
            SubscribeCallback(key, callback);
        }

        public void Add(string key, TValue item, DateTime absoluteExpiration, string[] tags, Action<string, KeyEvent> callback)
        {
            Add(key, new CacheItem<TValue>(item, absoluteExpiration), tags);
            SubscribeCallback(key, callback);
        }

        public void Add(string key, TValue item, TimeSpan slidingExpiration, string[] tags, Action<string, KeyEvent> callback)
        {
            Add(key, new CacheItem<TValue>(item, slidingExpiration), tags);
            SubscribeCallback(key, callback);
        }

        public void Add(string key, TValue item, TimeSpan slidingExpiration, Action<string, KeyEvent> callback)
        {
            Add(key, new CacheItem<TValue>(item, slidingExpiration));
            SubscribeCallback(key, callback);
        }
        #endregion

        #region Async

        public async Task AddAsync(string key, TValue item)
        {
            await AddAsync(key, item);
        }

        public async Task AddAsync(string key, TValue item, string[] tags)
        {
            await AddAsync(key, new CacheItem<TValue>(item));
            await _Cache.AddTagsToKeyAsync(key, tags);
        }

        public async Task<TValue> GetAsync(string key)
        {
            return await _Cache.GetObjectAsync<TValue>(key);
        }

        public async Task AddAsync(string key, TValue item, DateTime absoluteExpiration)
        {
            await AddAsync(key, new CacheItem<TValue>(item, absoluteExpiration));
        }

        public async Task AddAsync(string key, TValue item, DateTime absoluteExpiration, string[] tags)
        {
            await AddAsync(key, new CacheItem<TValue>(item, absoluteExpiration));
            await _Cache.AddTagsToKeyAsync(key, tags);
        }

        public async Task AddAsync(string key, TValue item, DateTime absoluteExpiration, Action<string, KeyEvent> callback)
        {
            await AddAsync(key, new CacheItem<TValue>(item, absoluteExpiration));
            SubscribeCallback(key, callback);
        }

        public async Task AddAsync(string key, TValue item, DateTime absoluteExpiration, string[] tags, Action<string, KeyEvent> callback)
        {
            await AddAsync(key, new CacheItem<TValue>(item, absoluteExpiration));
            await _Cache.AddTagsToKeyAsync(key, tags);
            SubscribeCallback(key, callback);
        }

        public async Task AddAsync(string key, TValue item, TimeSpan slidingExpiration)
        {
            await AddAsync(key, new CacheItem<TValue>(item, slidingExpiration));
        }

        public async Task AddAsync(string key, TValue item, TimeSpan slidingExpiration, string[] tags)
        {
            await AddAsync(key, new CacheItem<TValue>(item, slidingExpiration));
            await _Cache.AddTagsToKeyAsync(key, tags);
        }

        public async Task AddAsync(string key, TValue item, TimeSpan slidingExpiration, string[] tags, Action<string, KeyEvent> callback)
        {
            await AddAsync(key, new CacheItem<TValue>(item, slidingExpiration));
            await _Cache.AddTagsToKeyAsync(key, tags);
            SubscribeCallback(key, callback);
        }

        public async Task AddAsync(string key, TValue item, TimeSpan slidingExpiration, Action<string, KeyEvent> callback)
        {
            await AddAsync(key, new CacheItem<TValue>(item, slidingExpiration));
            SubscribeCallback(key, callback);
        } 
        #endregion

        #region Helpers

        private TValue GetOrAdd(string key, Func<TValue> item, TimeSpan slidingExpiration, DateTime absoluteExpiration, string[] tags)
        {
            var itemFromCache = Get(key);
            if (!EqualityComparer<TValue>.Default.Equals(itemFromCache, default(TValue)))
            {
                return itemFromCache;
            }

            var result = item();

            if (slidingExpiration > TimeSpan.Zero)
            {
                Add(key, new CacheItem<TValue>(result, slidingExpiration));
            }

            if (absoluteExpiration > DateTime.MinValue)
            {
                Add(key, new CacheItem<TValue>(result, absoluteExpiration));
            }

            if (tags != null)
            {
                _Cache.AddTagsToKey(key, tags);
            }

            return result;
        }

        private void SubscribeCallback(string key, Action<string, KeyEvent> callback)
        {
            lock (locker)
            {
                if (!Callbacks.ContainsKey(key))
                    Callbacks.Add(key, callback);
                else
                    throw new InvalidOperationException("Key already exist in cache.");
            }

            _Context.KeyEvents.Subscribe(KeyEventSubscriptionType.KeySpace, (callbackKey, callbackEvent) =>
            {
                lock (locker)
                {
                    if (Callbacks.ContainsKey(callbackKey))
                    {
                        Callbacks[key].Invoke(callbackKey, (KeyEvent)callbackEvent);
                        Callbacks.Remove(callbackKey);
                    }
                }
            });
        }

        private void Add(string key, CacheItem<TValue> cacheItem)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            if (cacheItem == null)
                throw new ArgumentNullException(nameof(cacheItem));

            _Cache.SetObject(key, cacheItem);

            if (cacheItem.SlidingExpiration > TimeSpan.Zero)
            {
                _Cache.KeyTimeToLive(key, cacheItem.SlidingExpiration);
            }

            if (cacheItem.AbsoluteExpiration > DateTime.MinValue)
            {
                _Cache.KeyExpire(key, cacheItem.AbsoluteExpiration);
            }
        }

        private async Task AddAsync(string key, CacheItem<TValue> cacheItem)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            if (cacheItem == null)
                throw new ArgumentNullException(nameof(cacheItem));

            await _Cache.SetObjectAsync(key, cacheItem);

            if (cacheItem.SlidingExpiration > TimeSpan.Zero)
            {
                await _Cache.KeyTimeToLiveAsync(key, cacheItem.SlidingExpiration);
            }

            if (cacheItem.AbsoluteExpiration > DateTime.MinValue)
            {
                await _Cache.KeyExpireAsync(key, cacheItem.AbsoluteExpiration);
            }
        }

        #endregion

        #region Named Collection

        public void Add(string collectionKey, string itemKey, TValue item)
        {
            AddHashedSet(collectionKey, itemKey, new CacheItem<TValue>() { Item = item, AbsoluteExpiration = DateTime.MinValue, SlidingExpiration = default(TimeSpan) }, null);
        }

        private void AddHashedSet(string collectionKey, string itemKey, CacheItem<TValue> cacheItem, string[] tags)
        {
            if (collectionKey == null)
                throw new ArgumentNullException(nameof(collectionKey));

            if (itemKey == null)
                throw new ArgumentNullException(nameof(itemKey));

            _Cache.SetHashed(collectionKey, itemKey, cacheItem.Item);

            if (cacheItem.SlidingExpiration > TimeSpan.Zero)
            {
                _Cache.KeyTimeToLive(collectionKey, cacheItem.SlidingExpiration);
            }

            if (cacheItem.AbsoluteExpiration > DateTime.MinValue)
            {
                _Cache.KeyExpire(collectionKey, cacheItem.AbsoluteExpiration);
            }

            if (tags != null)
            {
                _Cache.AddTagsToKey(collectionKey, tags);
            }
        }

        public void Add(string collectionKey, string itemKey, TValue item, TimeSpan collectionSlidingExpiration)
        {
            AddHashedSet(collectionKey, itemKey, new CacheItem<TValue>() { Item = item, AbsoluteExpiration = DateTime.MinValue, SlidingExpiration = collectionSlidingExpiration }, null);
        }

        public void Add(string collectionKey, string itemKey, TValue item, TimeSpan collectionSlidingExpiration, Action<string, KeyEvent> callback)
        {
            AddHashedSet(collectionKey, itemKey, new CacheItem<TValue>() { Item = item, AbsoluteExpiration = DateTime.MinValue, SlidingExpiration = collectionSlidingExpiration }, null);
            SubscribeCallback(collectionKey, callback);
        }

        public void Add(string collectionKey, string itemKey, TValue item, DateTime collectionAbsoluteExpiration)
        {
            AddHashedSet(collectionKey, itemKey, new CacheItem<TValue>() { Item = item, AbsoluteExpiration = collectionAbsoluteExpiration, SlidingExpiration = default(TimeSpan) }, null);
        }

        public void Add(string collectionKey, string itemKey, TValue item, DateTime collectionAbsoluteExpiration, Action<string, KeyEvent> callback)
        {
            AddHashedSet(collectionKey, itemKey, new CacheItem<TValue>() { Item = item, AbsoluteExpiration = collectionAbsoluteExpiration, SlidingExpiration = default(TimeSpan) }, null);
            SubscribeCallback(collectionKey, callback);
        }

        public void Add(string collectionKey, string itemKey, TValue item, string[] tags)
        {
            AddHashedSet(collectionKey, itemKey, new CacheItem<TValue>() { Item = item, AbsoluteExpiration = DateTime.MinValue, SlidingExpiration = default(TimeSpan) }, tags);
        }

        public void Remove(string collectionKey, string itemKey)
        {
            if (collectionKey == null)
                throw new ArgumentNullException(nameof(collectionKey));

            if (itemKey == null)
                throw new ArgumentNullException(nameof(itemKey));

            _Cache.RemoveHashed(collectionKey, itemKey);
        }

        public Dictionary<string, TValue> GetCollection(string collectionKey)
        {
            if (collectionKey == null)
                throw new ArgumentNullException(nameof(collectionKey));

            var dictionary = new Dictionary<string, TValue>();

            bool expirationSet = false;

            var dict = _Context.Collections.GetRedisDictionary<string, CacheItem<TValue>>(collectionKey);
            foreach (var item in dict)
            {
                dictionary.Add(item.Key, item.Value.Item);

                if (item.Value.SlidingExpiration > TimeSpan.Zero && !expirationSet)
                {
                    _Cache.KeyTimeToLive(collectionKey, item.Value.SlidingExpiration);
                    expirationSet = true;
                }
            }

            return dictionary;
        }

        public TValue Get(string collectionKey, string itemKey)
        {
            if (collectionKey == null)
                throw new ArgumentNullException(nameof(collectionKey));

            if (itemKey == null)
                throw new ArgumentNullException(nameof(itemKey));

            var cacheItem = _Cache.GetHashed<CacheItem<TValue>>(collectionKey, itemKey);

            if (cacheItem.SlidingExpiration > TimeSpan.Zero)
            {
                _Cache.KeyTimeToLive(collectionKey, cacheItem.SlidingExpiration);
            }

            return cacheItem.Item;
        }


        #endregion
    }

}
