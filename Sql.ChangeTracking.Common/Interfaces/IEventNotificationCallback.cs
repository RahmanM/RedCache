using System.ServiceModel;

namespace Sql.ChangeTracking.Common
{

    public interface IEventNotificationCallback
    {
        [OperationContract(IsOneWay = true)]
        void PublishTableChange(string tableName, string key);
    }
}
