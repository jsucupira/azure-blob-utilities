namespace AzureUtilities.Queues
{
    public interface IQueueProcessor
    {
        QueueResult ProcessQueueMessage(string message);
    }
}