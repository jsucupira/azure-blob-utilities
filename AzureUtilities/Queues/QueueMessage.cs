using Newtonsoft.Json;

namespace AzureUtilities.Queues
{
    /// <summary>
    /// Class QueueMessage.
    /// </summary>
    public class QueueMessage
    {
        /// <summary>
        /// The serialized json object
        /// Use DeserializeMessage() to get the object that was stored
        /// </summary>
        /// <value>The json message.</value>
        public string JsonMessage { get; private set; }
        /// <summary>
        /// Gets or sets the maximum retries.
        /// </summary>
        /// <value>The maximum retries.</value>
        public int MaxRetries { get; set; }
        /// <summary>
        /// Gets or sets the type of the message.
        /// </summary>
        /// <value>The type of the message.</value>
        public string MessageType { get; set; }

        /// <summary>
        /// Serializes the Message as a JSON format
        /// </summary>
        /// <param name="object">The object.</param>
        public void AddMessage(object @object)
        {
            JsonMessage = JsonConvert.SerializeObject(@object);
        }

        /// <summary>
        /// Deserializes the message that was stored as a JSON format.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>T.</returns>
        public T DeserializeMessage<T>()
        {
            if (!string.IsNullOrEmpty(JsonMessage))
                return JsonConvert.DeserializeObject<T>(JsonMessage);

            return default(T);
        }
    }
}