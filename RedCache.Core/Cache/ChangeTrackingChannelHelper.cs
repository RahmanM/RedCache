using System;
using System.ServiceModel;
using Sql.ChangeTracking.Common;
using Sql.ChangeTracking.Client;

namespace RedCache.Core
{
    /// <summary>
    /// Enables to connect to Sql Change tracking WCF service using duplex connection
    /// </summary>
    public class ChangeTrackingChannelHelper
    {
        public ChangeTrackingChannelHelper(ISqlChangeCallback callback)
        {
            if (callback == null)
                throw new ArgumentNullException(nameof(callback));

            Callback = callback;
        }

        public ISqlChangeCallback Callback { get; }

        private IChangeTrackingSubscriptions _channel = null;
        public IChangeTrackingSubscriptions OpenChannel()
        {
            var callbackCleint = new CallbackCleint(Callback);

            var client =
                new DuplexChannelFactoryClient<IChangeTrackingSubscriptions, IEventNotificationCallback>
                (callbackCleint, ServiceUrl.SqlTrackingWcfServiceAddress);

            _channel = client.CreateChannel();
            ((ICommunicationObject)_channel).Closed += Channel_Closed;
            ((ICommunicationObject)_channel).Faulted += Channel_Faulted;

            return _channel;
        }

        private void Channel_Faulted(object sender, EventArgs e)
        {
            // TODO: add logging
            OpenChannel();
        }

        private void Channel_Closed(object sender, EventArgs e)
        {
            OpenChannel();
        }


        private void HandleExceptionAndRetry(Func<ICommunicationObject> connectToService)
        {
            connectToService();
        }
    }

}
