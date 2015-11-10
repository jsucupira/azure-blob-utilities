namespace AzureUtilities.Service_Bus
{
    /// <summary>
    /// Interface IServiceBusContext
    /// </summary>
    public interface IServiceBusContext
    {
        /// <summary>
        /// Adds to queue.
        /// </summary>
        /// <param name="queueName">Name of the queue.</param>
        /// <param name="item">The item.</param>
        void AddToQueue(string queueName, object item);
        /// <summary>
        /// Gets or sets the connection string.
        /// </summary>
        /// <value>The connection string.</value>
        string ConnectionString { get; set; }
    }
}
