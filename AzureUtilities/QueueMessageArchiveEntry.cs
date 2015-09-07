using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureUtilities
{
    public class QueueMessageArchiveEntry : TableEntity
    {
        public QueueMessageArchiveEntry()
        {
            DateTime now = DateTime.UtcNow;
            PartitionKey = $"{now:yyyy-MM}";
            RowKey = $"{now:dd HH:mm:ss.fff}-{Guid.NewGuid()}";
        }

        public string Message { get; set; }
        public string Status { get; set; }

    }
}
