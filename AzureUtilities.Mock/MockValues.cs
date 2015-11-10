using System.Collections.Generic;

namespace AzureUtilities.Mock
{
    public static class MockValues
    {
        private static readonly Dictionary<string, object> _values;
        private static readonly object _lockObject = new object();

        static MockValues()
        {
            _values = new Dictionary<string, object>();
        }

        public static List<string> GetAllKeys(string keyStartsWith)
        {
            lock (_lockObject)
            {
                List<string> keys = new List<string>();
                foreach (string key in _values.Keys)
                {
                    if (key.StartsWith(keyStartsWith))
                        keys.Add(key);
                }
                return keys;
            }
        }

        public static object GetValue(string key)
        {
            lock (_lockObject)
            {
                object value;
                if (_values.TryGetValue(key, out value))
                    return value;
                return null;
            }
        }

        public static void SetValue(string key, object value)
        {
            lock (_lockObject)
            {
                _values.Remove(key);
                _values.Add(key, value);
            }
        }
    }
}