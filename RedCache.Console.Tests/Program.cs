using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using RedCache.Core;

namespace Sql.ConsoleClient.Tests
{
    class Program
    {
        private const int TotalCacheItems = 1000;

        static void Main(string[] args)
        {
            // Add items in bulk
            AddSimpleTypes();

            // Add list of items
            AddList();

            // Test sql dependency
            SqlDependency();

            Console.ReadLine();
        }

        private static void SqlDependency()
        {
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
        }

        private static void AddList()
        {
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

            Console.WriteLine(cache.Get("Users").FirstOrDefault().UserName);
        }

        private static void AddSimpleTypes()
        {
            var intCache = new Cache<int>();

            var watch = new Stopwatch();
            watch.Start();

            int i = 1;
            for (i = 1; i <= TotalCacheItems; i++)
            {
                intCache.Add(i.ToString(), i, TimeSpan.FromSeconds(10));
            }

            Console.WriteLine($"Added {i} => " + watch.ElapsedMilliseconds / 1000);
            watch.Restart();

            i = 1;
            for (i = 1; i <= TotalCacheItems; i++)
            {
                intCache.Get(i.ToString());
            }

            Console.WriteLine($"Read {i} => " + watch.Elapsed.Seconds);

        }
    }

   

    public class User
    {
        public int Id { get; set; }
        public string UserName { get; set; }
    }
}
