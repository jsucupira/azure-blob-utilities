using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureUtilities.Tables
{
    [Export(typeof (IAzureTableUtility))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class AzureTableUtility : IAzureTableUtility
    {
        private string _connectionString;
        private CloudStorageAccount _storageAccount;

        private CloudTable _table;
        private string _tableName;

        private CloudTable Table
        {
            get
            {
                if (_table == null)
                {
                    // Create the table client.
                    CloudTableClient tableClient = _storageAccount.CreateCloudTableClient();
                    // Create the CloudTable object that represents the "people" table.
                    _table = tableClient.GetTableReference(_tableName);
                }
                return _table;
            }
        }

        public bool CreateTable()
        {
            return Table.CreateIfNotExists();
        }

        public IList<TableResult> DeleteBatch(IEnumerable<TableEntity> entities)
        {
            TableBatchOperation batch = new TableBatchOperation();
            foreach (TableEntity tableEntity in entities)
                batch.Add(TableOperation.Delete(tableEntity));

            return Table.ExecuteBatch(batch);
        }

        public void DeleteEntity(string partitionKey, string rowKey)
        {
            DynamicTableEntity entity = new DynamicTableEntity(partitionKey, rowKey);
            TableOperation retrieve = TableOperation.Retrieve(partitionKey, rowKey);
            TableResult tableResult = this.Table.Execute(retrieve);

            if (tableResult != null && tableResult.Result != null)
            {
                entity.ETag = "*";
                TableOperation delete = TableOperation.Delete(entity);
                Table.Execute(delete);
            }
        }

        public void DeleteTable()
        {
            // Delete the table it if exists.
            Table.DeleteIfExists();
        }

        public List<T> ExecuteQuery<T>(TableQuery<T> exQuery) where T : TableEntity, new()
        {
            List<T> results = Table.ExecuteQuery(exQuery).Select(ent => ent).ToList();
            return results;
        }

        public T FindBy<T>(string partitionKey, string rowKey) where T : TableEntity, new()
        {
            TableOperation retrieve = TableOperation.Retrieve<T>(partitionKey, rowKey);
            TableResult result = this.Table.Execute(retrieve);
            if (result != null)
            {
                if (result.HttpStatusCode == 404)
                {
                    return default(T);
                }
                return (T)result.Result;
            }
            return default(T);
        }

        public IEnumerable<T> FindByPartitionKey<T>(string partitionKey) where T : TableEntity, new()
        {
            TableQuery<T> query = new TableQuery<T>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey));

            return Table.ExecuteQuery(query);
        }

        public TableResult Insert(TableEntity item)
        {
            TableOperation insertOperation = TableOperation.Insert(item);
            return Table.Execute(insertOperation);
        }

        public IEnumerable<string> QueryByProperty<T>(string property) where T : TableEntity, new()
        {
            // Define the query, and only select the Email property
            TableQuery<DynamicTableEntity> projectionQuery = new TableQuery<DynamicTableEntity>().Select(new[] {property});

            // Define an entity resolver to work with the entity after retrieval.
            EntityResolver<string> resolver = (pk, rk, ts, props, etag) => props.ContainsKey(property) ? props[property].StringValue : null;

            return Table.ExecuteQuery(projectionQuery, resolver, null, null);
        }

        public void UpdateItem<T>(TableEntity tableEntity) where T : TableEntity, new()
        {
            // Create a retrieve operation that takes a customer entity.
            TableOperation retrieveOperation = TableOperation.Retrieve<T>(tableEntity.PartitionKey, tableEntity.RowKey);

            // Execute the operation.
            TableResult retrievedResult = Table.Execute(retrieveOperation);

            // Assign the result to a CustomerEntity object.
            TableEntity updateEntity = (TableEntity) retrievedResult.Result;

            if (updateEntity != null)
            {
                // Change the phone number.
                updateEntity = tableEntity;

                // Create the InsertOrReplace TableOperation
                TableOperation updateOperation = TableOperation.Replace(updateEntity);

                // Execute the operation.
                Table.Execute(updateOperation);
            }
        }

        public IList<TableResult> UpsertBatch(IEnumerable<TableEntity> entities)
        {
            TableBatchOperation batch = new TableBatchOperation();
            foreach (TableEntity tableEntity in entities)
                batch.Add(TableOperation.InsertOrReplace(tableEntity));

            return Table.ExecuteBatch(batch);
        }

        public TableResult Upset<T>(TableEntity tableEntity) where T : TableEntity, new()
        {
            // Create the InsertOrReplace TableOperation
            TableOperation insertOrReplaceOperation = TableOperation.InsertOrReplace(tableEntity);

            // Execute the operation.
            return Table.Execute(insertOrReplaceOperation);
        }

        public string ConnectionString
        {
            get { return _connectionString; }
            set
            {
                if (string.IsNullOrEmpty(_connectionString))
                {
                    _connectionString = value;
                    _storageAccount = CloudStorageAccount.Parse(_connectionString);
                }
                else
                    throw new InvalidOperationException("Connections string can only be set once.");
            }
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