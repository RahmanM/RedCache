using System;

namespace RedCache.Core
{

    public interface ISqlCacheDependency
    {
        //string ClientUniqueId { get; set; }
        string TableName { get; set; }
        Action<string, KeyEvent> Callback { get; set; }
    }

    public class SqlCacheDependency : ISqlCacheDependency
    {
        public SqlCacheDependency(string tableName, Action<string, KeyEvent> callback)
        {
            if (callback == null)
                throw new ArgumentNullException(nameof(callback));

            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            TableName = tableName;
            Callback = callback;
        }

        public string TableName { get ; set ; }
        public Action<string, KeyEvent> Callback { get ; set ; }
    }

}
