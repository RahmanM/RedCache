using System;
using CachingFramework.Redis.Serializers;
using Nerdle.AutoConfig;
using Polly;
using System.Diagnostics;
using CachingFramework.Redis.Contracts;

namespace RedCache.Core
{

    public class ContextFactory
    {
        private static Lazy<CachingFramework.Redis.Context> lazyContext = new Lazy<CachingFramework.Redis.Context>(() => GetContext(Connection));
        private static string Connection { get; set; } = null;

        public static CachingFramework.Redis.Context InitialiseContext(bool forceNew = false)
        {
            if (forceNew)
            {
                return GetContext();
            }

            return lazyContext.Value;
        }

        public static IContext InitialiseContext(string connectionString, bool forceNewRedisConnection)
        {
            Connection = connectionString;

            if (forceNewRedisConnection)
            {
                return Connect(connectionString);
            }

            return lazyContext.Value;
        }

        private static CachingFramework.Redis.Context GetContext(string connection = null)
        {
            var config = AutoConfig.Map<RedCacheAppSettings>();
            return Connect(config.RedisConnection);
        }

        private static CachingFramework.Redis.Context Connect(string connection)
        {
            CachingFramework.Redis.Context context = null;

            Policy
                .Handle<DivideByZeroException>()
                .WaitAndRetry(new[]
                  {
                    TimeSpan.FromMilliseconds(100),
                    TimeSpan.FromSeconds(150),
                    TimeSpan.FromSeconds(200)
                  }, (exception, timeSpan, retryCount, conxt) =>
                  {
                      // TODO: Log the exeption
                      Debug.WriteLine(exception.Message);
                  })
                  .Execute(() =>
                  {
                      context = new CachingFramework.Redis.Context(connection, new JsonSerializer());
                  });

            return context;
        }

        private ContextFactory()
        {
        }
    }


}


public class RedCacheAppSettings
{
    public string RedisConnection { get; set; }
}
