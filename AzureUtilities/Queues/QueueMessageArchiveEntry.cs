using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureUtilities.Queues
{
    public class QueueMessageArchiveEntry : TableEntity
    {
        public QueueMessageArchiveEntry()
        {
            DateTime now = DateTime.UtcNow;
            PartitionKey = $"{now:yyyy-MM-DD}";
            RowKey = Guid.NewGuid().ToString();
        }

        public string Message { get; set; }
        public string Status { get; set; }

    }
}
