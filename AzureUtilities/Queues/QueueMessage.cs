namespace AzureUtilities.Queues
{
    public class QueueMessage
    {
        public string MessageType { get; set; }
        public int MaxRetries { get; set; }
        public string Message { get; set; }
    }
}
