using System.Collections.Generic;
using System.Linq;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;

namespace AzureUtilities
{
    /// <summary>
    /// Helper class to manage messages in an Azure Queue
    /// </summary>
    public class AzureQueueUtility
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AzureQueueUtility"/> class.
        /// </summary>
        /// <param name="accountConnectionString">The account connection string.</param>
        /// <param name="queueName">Name of the queue.</param>
        /// <param name="archiveTableName">Name of the archive table.</param>
        public AzureQueueUtility(string accountConnectionString, string queueName, string archiveTableName = "QueueArchive")
        {
            _storageAccount = CloudStorageAccount.Parse(accountConnectionString);
            _queueClient = _storageAccount.CreateCloudQueueClient();
            _queueName = queueName;
            _archiveTableName = archiveTableName;
        }

        /// <summary>
        /// The _archive table name
        /// </summary>
        private readonly string _archiveTableName;

        /// <summary>
        /// The _queue client
        /// </summary>
        private readonly CloudQueueClient _queueClient;
        /// <summary>
        /// The _queue name
        /// </summary>
        private readonly string _queueName;
        /// <summary>
        /// The _storage account
        /// </summary>
        private readonly CloudStorageAccount _storageAccount;

        /// <summary>
        /// Adds the queue message.
        /// </summary>
        /// <param name="queueMessage">The queue message.</param>
        public void AddQueueMessage(QueueMessage queueMessage)
        {
            CloudQueue queue = _queueClient.GetQueueReference(_queueName);
            queue.CreateIfNotExists();

            string messJSON = JsonConvert.SerializeObject(queueMessage);

            queue.AddMessage(new CloudQueueMessage(messJSON));
        }

        /// <summary>
        /// Archives the queue message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="status">The status.</param>
        public void ArchiveQueueMessage(QueueMessage message, string status = "success")
        {
            //save message to AZURE Table?
            AzureTableUtility tableUtility = new AzureTableUtility(_storageAccount, _archiveTableName);
            QueueMessageArchiveEntry entry = new QueueMessageArchiveEntry
            {
                Message = JsonConvert.SerializeObject(message), 
                Status = status
            };
            tableUtility.AddItemToTable(entry);
        }

        /// <summary>
        /// Clears the poison message.
        /// </summary>
        /// <param name="message">The message.</param>
        public void ClearPoisonMessage(CloudQueueMessage message)
        {
            //archive message
            ArchiveQueueMessage(GetQueueMessageFromCloudMessage(message), "poison");
            //dequeue message
            DequeueMessage(message);
        }

        /// <summary>
        /// Dequeues the message.
        /// </summary>
        /// <param name="message">The message.</param>
        public void DequeueMessage(CloudQueueMessage message)
        {
            CloudQueue queue = _queueClient.GetQueueReference(_queueName);
            queue.DeleteMessage(message);
        }

        /// <summary>
        /// Gets the queue count.
        /// </summary>
        /// <returns>System.Int32.</returns>
        public int GetQueueCount()
        {
            CloudQueue queue = _queueClient.GetQueueReference(_queueName);
            queue.FetchAttributes();
            return queue.ApproximateMessageCount ?? default(int);
        }

        /// <summary>
        /// Gets the queue message from cloud message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>QueueMessage.</returns>
        public QueueMessage GetQueueMessageFromCloudMessage(CloudQueueMessage message)
        {
            return JsonConvert.DeserializeObject<QueueMessage>(message.AsString);
        }

        /// <summary>
        /// Gets the queue messages.
        /// </summary>
        /// <param name="messageCount">The message count.</param>
        /// <returns>List&lt;CloudQueueMessage&gt;.</returns>
        public List<CloudQueueMessage> GetQueueMessages(int messageCount)
        {
            CloudQueue queue = _queueClient.GetQueueReference(_queueName);

            //max number of messages Azure Queues will return is 32 
            List<CloudQueueMessage> messages = queue.GetMessages(messageCount > 32 ? 32 : messageCount).ToList();

            return messages;
        }

        /// <summary>
        /// Processes the first queue message in the Azure queue.
        /// </summary>
        /// <param name="queueProcessor">Queue processor object to be used to process the message.</param>
        /// <returns>QueueResult object detailing the result of the processor.</returns>
        public QueueResult ProcessQueueMessage(IQueueProcessor queueProcessor)
        {
            return ProcessQueueMessage(GetQueueMessages(1).FirstOrDefault(), queueProcessor);
        }

        /// <summary>
        /// Processes the passed Queue Message using the passed Processor object.
        /// </summary>
        /// <param name="cloudMessage">Azure CloudQueueMessage that wraps the QueueMessage object.</param>
        /// <param name="queueProcessor">Queue processor object to be used to process the message.</param>
        /// <returns>QueueResult object detailing the result of the processor.</returns>
        /// <remarks>This is the method that does the actual processing the other overloads all call this method.</remarks>
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

        /// <summary>
        /// Processes the first 'n' messages in the Queue based on the message Count passed in.
        /// </summary>
        /// <param name="messageCount">Int number of messages that will be processed in the batch</param>
        /// <param name="queueProcessor">Queue processor object to be used to process the message.</param>
        /// <returns>List of QueueResult objects detailing the result of the processor.</returns>
        public List<QueueResult> ProcessQueueMessages(int messageCount, IQueueProcessor queueProcessor)
        {
            return ProcessQueueMessages(GetQueueMessages(messageCount), queueProcessor);
        }

        /// <summary>
        /// Processes the passed list of messages with the passed queue processor
        /// </summary>
        /// <param name="messages">List of Azure CloudQueueMessages that wraps the QueueMessage object.</param>
        /// <param name="queueProcessor">Queue processor object to be used to process the message.</param>
        /// <returns>List of QueueResult objects detailing the result of the processor.</returns>
        public List<QueueResult> ProcessQueueMessages(List<CloudQueueMessage> messages, IQueueProcessor queueProcessor)
        {
            List<QueueResult> results = new List<QueueResult>();
            //loop messages
            foreach (CloudQueueMessage cloudMessage in messages)
                results.Add(ProcessQueueMessage(cloudMessage, queueProcessor));

            return results;
        }
    }
}