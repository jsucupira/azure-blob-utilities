using System;
using System.ComponentModel.Composition;
using AzureUtilities.Service_Bus;

namespace AzureUtilities.Mock
{
    [Export(typeof (IServiceBusContext))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class MockeServiceBus : IServiceBusContext
    {
        private string _connectionString;

        public void AddToQueue(string queueName, object item)
        {
        }

        public string ConnectionString
        {
            get { return _connectionString; }
            set
            {
                if (string.IsNullOrEmpty(_connectionString))
                    _connectionString = value;
                else
                    throw new InvalidOperationException("Connections string can only be set once.");
            }
        }
    }
}