﻿namespace AzureUtilities
{
    public interface IQueueProcessor
    {
        QueueResult ProcessQueueMessage(string message);
    }
}