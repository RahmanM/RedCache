using Serilog;
using Sql.ChangeTracking.Common;
using Sql.ChangeTracking.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;

namespace Sql.ChangeTracking.Wcf
{
    public class SqlChangeTrackingWcfService : IChangeTrackingSubscriptions
    {
        private object locker = new object();
        public Dictionary<Subscriber, IEventNotificationCallback> Subscribers {get; set; }

        public SqlChangeTrackingWcfService()
        {
            Subscribers = new Dictionary<Subscriber, IEventNotificationCallback>();
        }

        public ILogger Logger { get; set; }

        public void Subscribe(string uniqueKey, string tableName)
        {
            try
            {
                IEventNotificationCallback callback = OperationContext.Current.GetCallbackChannel<IEventNotificationCallback>();
                lock (locker)
                {
                    Subscriber subscriber = new Subscriber();
                    subscriber.Id = uniqueKey;
                    subscriber.TableInterested = tableName;
                    Subscribers.Add(subscriber, callback);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, ex.Message);
                throw;
            }
        }

        public void TableChanged(UspTableVersionChangeTrackingReturnModel table)
        {
            // get all the subscribers
            try
            {
                var subscriberKeys = (from c in Subscribers
                                      select c.Key).ToList();

                for (int i = subscriberKeys.Count-1; i >= 0 ; i--)
                {
                    var keyValue = subscriberKeys.ElementAt(i);

                    IEventNotificationCallback callback = Subscribers[keyValue];
                    if (((ICommunicationObject)callback).State == CommunicationState.Opened)
                    {
                        //call back only those subscribers who are interested in this fileType
                        if (string.Equals(keyValue.TableInterested, table.Name, StringComparison.OrdinalIgnoreCase))
                        {
                            callback.PublishTableChange(table.Name, keyValue.Id);
                        }
                    }
                    else
                    {
                        //These subscribers are no longer active. Delete them from subscriber list
                        subscriberKeys.Remove(keyValue);
                        Subscribers.Remove(keyValue);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, ex.Message);
                throw;
            }

        }

        public void Unsubscribe(string uniqueKey, string tableName)
        {
            try
            {
                lock (locker)
                {
                    var SubToBeDeleted = from c in Subscribers.Keys
                                         where c.Id == uniqueKey && c.TableInterested == tableName
                                         select c;

                    if (SubToBeDeleted.Any())
                    {
                        Subscribers.Remove(SubToBeDeleted.First());
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, ex.Message);
                throw;
            }
        }
    }
}
