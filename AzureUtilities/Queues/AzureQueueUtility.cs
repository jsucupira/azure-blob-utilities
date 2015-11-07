using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace AzureUtilities.Queues
{
    [Export(typeof (IAzureQueueUtility))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class AzureQueueUtility : IAzureQueueUtility
    {
        private string _archiveTableName;
        private string _connectionString;

        private CloudQueueClient _queueClient;

        private string _queueName;

        private CloudStorageAccount _storageAccount;

        public void AddQueueMessage(QueueMessage queueMessage)
        {
            CloudQueue queue = _queueClient.GetQueueReference(_queueName);
            queue.CreateIfNotExists();

            string messJSON = JsonConvert.SerializeObject(queueMessage);

            queue.AddMessage(new CloudQueueMessage(messJSON));
        }

        public void ArchiveQueueMessage(QueueMessage message, string status = "success")
        {
            CloudTableClient tableClient = _storageAccount.CreateCloudTableClient();
            // Create the CloudTable object that represents the "people" table.
            CloudTable table = tableClient.GetTableReference(_archiveTableName);
            //save message to AZURE Table?
            QueueMessageArchiveEntry entry = new QueueMessageArchiveEntry
            {
                Message = JsonConvert.SerializeObject(message),
                Status = status
            };
            TableOperation insertOperation = TableOperation.Insert(entry);
            table.Execute(insertOperation);
        }

        public void ClearPoisonMessage(CloudQueueMessage message)
        {
            //archive message
            ArchiveQueueMessage(GetQueueMessageFromCloudMessage(message), "poison");
            //dequeue message
            DequeueMessage(message);
        }

        public void DequeueMessage(CloudQueueMessage message)
        {
            CloudQueue queue = _queueClient.GetQueueReference(_queueName);
            queue.DeleteMessage(message);
        }

        public int GetQueueCount()
        {
            CloudQueue queue = _queueClient.GetQueueReference(_queueName);
            queue.FetchAttributes();
            return queue.ApproximateMessageCount ?? default(int);
        }

        public QueueMessage GetQueueMessageFromCloudMessage(CloudQueueMessage message)
        {
            return JsonConvert.DeserializeObject<QueueMessage>(message.AsString);
        }

        public List<CloudQueueMessage> GetQueueMessages(int messageCount)
        {
            CloudQueue queue = _queueClient.GetQueueReference(_queueName);

            //max number of messages Azure Queues will return is 32 
            List<CloudQueueMessage> messages = queue.GetMessages(messageCount > 32 ? 32 : messageCount).ToList();

            return messages;
        }

        public QueueResult ProcessQueueMessage(IQueueProcessor queueProcessor)
        {
            return ProcessQueueMessage(GetQueueMessages(1).FirstOrDefault(), queueProcessor);
        }

        public QueueResult ProcessQueueMessage(CloudQueueMessage cloudMessage, IQueueProcessor queueProcessor)
        {
            QueueMessage qMessage = GetQueueMessageFromCloudMessage(cloudMessage);
            QueueResult queueResult = new QueueResult();

            //test for poison message 
            if (cloudMessage.DequeueCount > qMessage.MaxRetries)
            {
                // if poison clear poison message
                ClearPoisonMessage(cloudMessage);
                queueResult.Error = "Max retry count has been exceeded.";
                queueResult.Result = false;
                queueResult.Response = string.Empty;
            }
            else
            {
                //if not process queue message
                //make ProcessQueueMessageCall
                queueResult = queueProcessor.ProcessQueueMessage(qMessage.Message);

                if (string.IsNullOrEmpty(queueResult.Error))
                {
                    //archive message
                    ArchiveQueueMessage(qMessage);
                    //dequeue message
                    DequeueMessage(cloudMessage);
                }
            }
            return queueResult;
        }

        public List<QueueResult> ProcessQueueMessages(int messageCount, IQueueProcessor queueProcessor)
        {
            return ProcessQueueMessages(GetQueueMessages(messageCount), queueProcessor);
        }

        public List<QueueResult> ProcessQueueMessages(List<CloudQueueMessage> messages, IQueueProcessor queueProcessor)
        {
            List<QueueResult> results = new List<QueueResult>();
            //loop messages
            foreach (CloudQueueMessage cloudMessage in messages)
                results.Add(ProcessQueueMessage(cloudMessage, queueProcessor));

            return results;
        }

        public string ArchiveTableName
        {
            get { return _archiveTableName; }
            set
            {
                _archiveTableName = value;

                if (_archiveTableName != null)
                    throw new InvalidOperationException("TableName can only be set once.");
            }
        }

        public string QueueName
        {
            get { return _queueName; }
            set
            {
                _queueName = value;

                if (_queueName != null)
                    throw new InvalidOperationException("Queue Name can only be set once.");
            }
        }

        public string ConnectionString
        {
            get { return _connectionString; }
            set
            {
                _connectionString = value;
                _storageAccount = CloudStorageAccount.Parse(_connectionString);
                _queueClient = _storageAccount.CreateCloudQueueClient();
                if (_connectionString != null)
                    throw new InvalidOperationException("Connection String can only be set once.");
            }
        }
    }
}