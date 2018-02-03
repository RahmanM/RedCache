using Serilog;
using Sql.ChangeTracking.Data;
using System.Collections.Generic;
using System.ServiceModel;

namespace Sql.ChangeTracking.Common
{
    // NB: It requires session to enable callback, and CallbackContract is important!!
    [ServiceContract(SessionMode = SessionMode.Required, CallbackContract = typeof(IEventNotificationCallback))]
    public interface IChangeTrackingSubscriptions
    {
        [OperationContract(IsOneWay = true)]
        void Subscribe(string uniqueKey, string tableName);

        [OperationContract(IsOneWay = true)]
        void Unsubscribe(string uniqueKey, string tableName);

        [OperationContract(IsOneWay = true)] 
        void TableChanged(UspTableVersionChangeTrackingReturnModel table);

        ILogger Logger { get; set; }

        Dictionary<Subscriber, IEventNotificationCallback> Subscribers { get; set; }
    }
}
