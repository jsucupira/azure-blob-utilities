using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage.Queue;

namespace AzureUtilities.Queues
{
    /// <summary>
    /// Interface IAzureQueueUtility
    /// </summary>
    public interface IAzureQueueUtility
    {
        /// <summary>
        /// Adds the queue message.
        /// </summary>
        /// <param name="queueMessage">The queue message.</param>
        void AddQueueMessage(QueueMessage queueMessage);

        /// <summary>
        /// Archives the queue message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="status">The status.</param>
        void ArchiveQueueMessage(QueueMessage message, string status = "success");

        /// <summary>
        /// Clears the poison message.
        /// </summary>
        /// <param name="message">The message.</param>
        void ClearPoisonMessage(CloudQueueMessage message);

        /// <summary>
        /// Dequeues the message.
        /// </summary>
        /// <param name="message">The message.</param>
        void DequeueMessage(CloudQueueMessage message);

        /// <summary>
        /// Gets the queue count.
        /// </summary>
        /// <returns>System.Int32.</returns>
        int GetQueueCount();

        /// <summary>
        /// Gets the queue message from cloud message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>QueueMessage.</returns>
        QueueMessage GetQueueMessageFromCloudMessage(CloudQueueMessage message);

        /// <summary>
        /// Gets the queue messages.
        /// </summary>
        /// <param name="messageCount">The message count.</param>
        /// <returns>List&lt;CloudQueueMessage&gt;.</returns>
        List<CloudQueueMessage> GetQueueMessages(int messageCount);

        /// <summary>
        /// Processes the first queue message in the Azure queue.
        /// </summary>
        /// <param name="queueProcessor">Queue processor object to be used to process the message.</param>
        /// <returns>QueueResult object detailing the result of the processor.</returns>
        QueueResult ProcessQueueMessage(IQueueProcessor queueProcessor);

        /// <summary>
        /// Processes the passed Queue Message using the passed Processor object.
        /// </summary>
        /// <param name="cloudMessage">Azure CloudQueueMessage that wraps the QueueMessage object.</param>
        /// <param name="queueProcessor">Queue processor object to be used to process the message.</param>
        /// <returns>QueueResult object detailing the result of the processor.</returns>
        /// <remarks>This is the method that does the actual processing the other overloads all call this method.</remarks>
        QueueResult ProcessQueueMessage(CloudQueueMessage cloudMessage, IQueueProcessor queueProcessor);

        /// <summary>
        /// Processes the first 'n' messages in the Queue based on the message Count passed in.
        /// </summary>
        /// <param name="messageCount">Int number of messages that will be processed in the batch</param>
        /// <param name="queueProcessor">Queue processor object to be used to process the message.</param>
        /// <returns>List of QueueResult objects detailing the result of the processor.</returns>
        List<QueueResult> ProcessQueueMessages(int messageCount, IQueueProcessor queueProcessor);

        /// <summary>
        /// Processes the passed list of messages with the passed queue processor
        /// </summary>
        /// <param name="messages">List of Azure CloudQueueMessages that wraps the QueueMessage object.</param>
        /// <param name="queueProcessor">Queue processor object to be used to process the message.</param>
        /// <returns>List of QueueResult objects detailing the result of the processor.</returns>
        List<QueueResult> ProcessQueueMessages(List<CloudQueueMessage> messages, IQueueProcessor queueProcessor);

        /// <summary>
        /// Gets or sets the name of the archive table.
        /// </summary>
        /// <value>The name of the archive table.</value>
        string ArchiveTableName { get; set; }
        /// <summary>
        /// Gets or sets the name of the queue.
        /// </summary>
        /// <value>The name of the queue.</value>
        string QueueName { get; set; }
        /// <summary>
        /// Gets or sets the connection string.
        /// </summary>
        /// <value>The connection string.</value>
        string ConnectionString { get; set; }
    }
}