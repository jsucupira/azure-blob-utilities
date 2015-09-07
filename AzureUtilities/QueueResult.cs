using System;

namespace AzureUtilities
{
    [Serializable]
    public class QueueResult
    {
        public bool Result { get; set; }
        public string Error { get; set; }
        public string Response { get; set; }
    }
}
