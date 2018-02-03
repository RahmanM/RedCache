using System;
using Sql.ChangeTracking.Common;

namespace RedCache.Core
{

    public class CallbackCleint : IEventNotificationCallback, IDisposable, IEquatable<CallbackCleint>
    {
        public CallbackCleint(ISqlChangeCallback callback)
        {
            Callback = callback;
        }

        public ISqlChangeCallback Callback { get; }

        public void Dispose()
        {
        }

        public void PublishTableChange(string tableName, string key)
        {
            Callback.SqlChangedCallback(tableName, key);
        }

        public bool Equals(CallbackCleint other)
        {
            throw new NotImplementedException();
        }
    }

}
