using System;
using System.ComponentModel.Composition;
using Microsoft.ServiceBus.Messaging;

namespace AzureUtilities.Service_Bus
{
    [Export(typeof(IServiceBusContext))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class ServiceBusContext : IServiceBusContext
    {
        private string _connectionString;

        public void AddToQueue(string queueName, object item)
        {
            QueueClient client = QueueClient.CreateFromConnectionString(ConnectionString, queueName);
            BrokeredMessage message = new BrokeredMessage(item);
            client.Send(message);
        }

        public string ConnectionString
        {
            get { return _connectionString; }
            set
            {
                if (string.IsNullOrEmpty(_connectionString))
                {
                    _connectionString = value;
                }
                else
                    throw new InvalidOperationException("Connections string can only be set once.");
            }
        }
    }
}