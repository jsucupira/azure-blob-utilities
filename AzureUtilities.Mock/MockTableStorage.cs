using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Net;
using AzureUtilities.Tables;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureUtilities.Mock
{
    [Export(typeof (IAzureTableUtility))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class MockTableStorage : IAzureTableUtility
    {
        private const char DELIMITER = '~';
        private static readonly Dictionary<string, Dictionary<string, object>> _tables = new Dictionary<string, Dictionary<string, object>>();
        private string _tableName;
        private string _connectionString;

        private Dictionary<string, object> Table
        {
            get
            {
                lock (_tables)
                {
                    Dictionary<string, object> table = null;

                    if (string.IsNullOrEmpty(TableName))
                        throw new ArgumentNullException("TableName");
                    if (_tables.ContainsKey(TableName))
                        table = _tables[TableName];
                    else
                    {
                        table = new Dictionary<string, object>();
                        _tables.Add(TableName, table);
                    }
                    return table;
                }
            }
        }
        
        public IList<TableResult> DeleteBatch(IEnumerable<TableEntity> entities)
        {
            throw new NotImplementedException();
        }

        public void DeleteEntity(string partitionKey, string rowKey)
        {
            string key = string.Format("{0}{1}{2}", partitionKey, DELIMITER, rowKey);
            Table.Remove(key);
        }

        public void DeleteTable()
        {
            if (string.IsNullOrEmpty(TableName))
                throw new ApplicationException("TableName name missing");
            lock (_tables)
            {
                if (_tables.ContainsKey(TableName))
                    _tables.Remove(TableName);
            }
        }

        public List<T> ExecuteQuery<T>(TableQuery<T> exQuery) where T : TableEntity, new()
        {
            throw new NotImplementedException();
        }

        public T FindBy<T>(string partitionKey, string rowKey) where T : TableEntity, new()
        {
            string key = string.Format("{0}{1}{2}", partitionKey, DELIMITER, rowKey);
            object value;
            if (Table.TryGetValue(key, out value))
                return (T) value;
            return default(T);
        }

        public IEnumerable<T> FindByPartitionKey<T>(string partitionKey) where T : TableEntity, new()
        {
            List<T> results = new List<T>();
            foreach (string key in Table.Keys)
            {
                string[] parts = key.Split(DELIMITER);
                if (parts[0] == partitionKey)
                    results.Add((T) Table[key]);
            }
            return results;
        }

        public TableResult Insert(TableEntity item)
        {
            string key = string.Format("{0}{1}{2}", item.PartitionKey, DELIMITER, item.RowKey);
            item.Timestamp = DateTime.Now;
            Table.Remove(key);
            Table.Add(key, item);
            return new TableResult
            {
                HttpStatusCode = (int) HttpStatusCode.NoContent
            };
        }

        public IEnumerable<string> QueryByProperty<T>(string property) where T : TableEntity, new()
        {
            throw new NotImplementedException();
        }

        public void UpdateItem<T>(TableEntity tableEntity) where T : TableEntity, new()
        {
            string key = string.Format("{0}{1}{2}", tableEntity.PartitionKey, DELIMITER, tableEntity.RowKey);
            Table[key] = tableEntity;
        }

        public IList<TableResult> UpsertBatch(IEnumerable<TableEntity> entities)
        {
            throw new NotImplementedException();
        }

        public TableResult Upset<T>(TableEntity tableEntity) where T : TableEntity, new()
        {
            string key = string.Format("{0}{1}{2}", tableEntity.PartitionKey, DELIMITER, tableEntity.RowKey);

            if (Table.ContainsKey(key))
                UpdateItem<T>(tableEntity);
            else
                Insert(tableEntity);

            return new TableResult
            {
                HttpStatusCode = (int) HttpStatusCode.NoContent
            };
        }

        public string ConnectionString
        {
            get { return _connectionString; }
            set
            {
                if (string.IsNullOrEmpty(_connectionString))
                {
                    _connectionString = value;
                }
                else
                    throw new InvalidOperationException("Connections string can only be set once.");
            }
        }

        public bool CreateTable()
        {
            if (string.IsNullOrEmpty(ConnectionString))
                throw new InvalidOperationException("Connectionstring cannot be null");

            if (string.IsNullOrEmpty(TableName))
                throw new InvalidOperationException("TableName name missing");
            return true;
        }

        public string TableName
        {
            get { return _tableName; }
            set
            {
                if (_tableName != null)
                    throw new InvalidOperationException("TableName can only be set once.");
                _tableName = value;
            }
        }
    }
}