using System.Collections.Generic;
using System.Linq;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureUtilities
{
    /// <summary>
    /// Class AzureTableUtility.
    /// </summary>
    public class AzureTableUtility
    {
        /// <summary>
        /// The _table name
        /// </summary>
        private readonly string _tableName;
        /// <summary>
        /// The _storage account
        /// </summary>
        private readonly CloudStorageAccount _storageAccount;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureTableUtility"/> class.
        /// </summary>
        /// <param name="accountConnectionString">The account connection string.</param>
        /// <param name="tableName">Name of the table.</param>
        public AzureTableUtility(string accountConnectionString, string tableName)
        {
            _tableName = tableName;
            _storageAccount = CloudStorageAccount.Parse(accountConnectionString);
            CreateTable();
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="AzureTableUtility"/> class.
        /// </summary>
        /// <param name="storageAccount">The storage account.</param>
        /// <param name="tableName">Name of the table.</param>
        public AzureTableUtility(CloudStorageAccount storageAccount, string tableName)
        {
            _tableName = tableName;
            _storageAccount = storageAccount;
            CreateTable();
        }

        /// <summary>
        /// Creates the table.
        /// </summary>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public bool CreateTable()
        {
            // Create the table client.
            CloudTableClient tableClient = _storageAccount.CreateCloudTableClient();
            // Create the table if it doesn't exist.
            CloudTable table = tableClient.GetTableReference(_tableName);
            return table.CreateIfNotExists();
        }

        /// <summary>
        /// Adds the item to table.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>TableResult.</returns>
        public TableResult AddItemToTable(TableEntity item)
        {
            // Create the table client.
            CloudTableClient tableClient = _storageAccount.CreateCloudTableClient();

            // Create the CloudTable object that represents the "people" table.
            CloudTable table = tableClient.GetTableReference(_tableName);

            // Create the TableOperation that inserts the customer entity.
            TableOperation insertOperation = TableOperation.Insert(item);

            // Execute the insert operation.
            return table.Execute(insertOperation);
        }

        /// <summary>
        /// Adds the item to table.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>IEnumerable&lt;TableResult&gt;.</returns>
        public IEnumerable<TableResult> AddItemToTable(IEnumerable<TableEntity> item)
        {
            // Create the table client.
            CloudTableClient tableClient = _storageAccount.CreateCloudTableClient();

            // Create the CloudTable object that represents the "people" table.
            CloudTable table = tableClient.GetTableReference(_tableName);

            // Create the batch operation.
            TableBatchOperation batchOperation = new TableBatchOperation();

            foreach (var tableEntity in item)
                batchOperation.Insert(tableEntity);

            // Execute the batch operation.
            return table.ExecuteBatch(batchOperation);
        }

        /// <summary>
        /// Finds the by.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="partitionKey">The partition key.</param>
        /// <param name="rowKey">The row key.</param>
        /// <returns>IEnumerable&lt;T&gt;.</returns>
        public IEnumerable<T> FindBy<T>(string partitionKey, string rowKey) where T : TableEntity, new()
        {
            // Create the table client.
            CloudTableClient tableClient = _storageAccount.CreateCloudTableClient();

            // Create the CloudTable object that represents the "people" table.
            CloudTable table = tableClient.GetTableReference(_tableName);

            // Construct the query operation for all customer entities where PartitionKey="Smith".
            var filter = TableQuery.CombineFilters(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey)
                , TableOperators.And, TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, rowKey));

            TableQuery<T> query = new TableQuery<T>().Where(filter);

            // Print the fields for each customer.
            return table.ExecuteQuery(query);
        }

        /// <summary>
        /// Finds the by partition key.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="partitionKey">The partition key.</param>
        /// <returns>IEnumerable&lt;T&gt;.</returns>
        public IEnumerable<T> FindByPartitionKey<T>(string partitionKey) where T : TableEntity, new()
        {
            // Create the table client.
            CloudTableClient tableClient = _storageAccount.CreateCloudTableClient();

            // Create the CloudTable object that represents the "people" table.
            CloudTable table = tableClient.GetTableReference(_tableName);

            // Construct the query operation for all customer entities where PartitionKey="Smith".
            TableQuery<T> query = new TableQuery<T>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey));

            // Print the fields for each customer.
            return table.ExecuteQuery(query);
        }

        /// <summary>
        /// Creates the query.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>TableQuery&lt;T&gt;.</returns>
        public TableQuery<T> CreateQuery<T>() where T : TableEntity, new()
        {
            // Create the table query.
            TableQuery<T> rangeQuery = new TableQuery<T>();

            // Loop through the results, displaying information about the entity.
            return rangeQuery;
        }

        /// <summary>
        /// Executes the query.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="exQuery">The ex query.</param>
        /// <returns>List&lt;T&gt;.</returns>
        public List<T> ExecuteQuery<T>(TableQuery<T> exQuery) where T : TableEntity, new()
        {
            CloudTableClient tableClient = _storageAccount.CreateCloudTableClient();

            //Create the CloudTable object that represents the "people" table.
            CloudTable table = tableClient.GetTableReference(_tableName);
            var results = table.ExecuteQuery(exQuery).Select(ent => (T)ent).ToList();
            return results;
        }

        /// <summary>
        /// Updates the item.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tableEntity">The table entity.</param>
        public void UpdateItem<T>(TableEntity tableEntity) where T : TableEntity, new()
        {
            // Create the table client
            CloudTableClient tableClient = _storageAccount.CreateCloudTableClient();

            // Create the CloudTable object that represents the "people" table.
            CloudTable table = tableClient.GetTableReference(_tableName);

            // Create a retrieve operation that takes a customer entity.
            TableOperation retrieveOperation = TableOperation.Retrieve<T>(tableEntity.PartitionKey, tableEntity.RowKey);

            // Execute the operation.
            TableResult retrievedResult = table.Execute(retrieveOperation);

            // Assign the result to a CustomerEntity object.
            TableEntity updateEntity = (TableEntity)retrievedResult.Result;

            if (updateEntity != null)
            {
                // Change the phone number.
                updateEntity = tableEntity;

                // Create the InsertOrReplace TableOperation
                TableOperation updateOperation = TableOperation.Replace(updateEntity);

                // Execute the operation.
                table.Execute(updateOperation);
            }
        }

        /// <summary>
        /// Upsets the specified table entity.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tableEntity">The table entity.</param>
        public void Upset<T>(TableEntity tableEntity) where T : TableEntity, new()
        {
            // Create the table client.
            CloudTableClient tableClient = _storageAccount.CreateCloudTableClient();

            // Create the CloudTable object that represents the "people" table.
            CloudTable table = tableClient.GetTableReference(_tableName);

            // Create the InsertOrReplace TableOperation
            TableOperation insertOrReplaceOperation = TableOperation.InsertOrReplace(tableEntity);

            // Execute the operation.
            table.Execute(insertOrReplaceOperation);
        }

        /// <summary>
        /// Queries the by property.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="property">The property.</param>
        /// <returns>IEnumerable&lt;System.String&gt;.</returns>
        public IEnumerable<string> QueryByProperty<T>(string property) where T : TableEntity, new()
        {
            // Create the table client
            CloudTableClient tableClient = _storageAccount.CreateCloudTableClient();

            //Create the CloudTable that represents the "people" table.
            CloudTable table = tableClient.GetTableReference(_tableName);

            // Define the query, and only select the Email property
            TableQuery<DynamicTableEntity> projectionQuery = new TableQuery<DynamicTableEntity>().Select(new[] { property });

            // Define an entity resolver to work with the entity after retrieval.
            EntityResolver<string> resolver = (pk, rk, ts, props, etag) => props.ContainsKey(property) ? props[property].StringValue : null;

            return table.ExecuteQuery(projectionQuery, resolver, null, null);
        }

        /// <summary>
        /// Deletes the entity.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tableEntity">The table entity.</param>
        public void DeleteEntity<T>(TableEntity tableEntity) where T : TableEntity, new()
        {
            // Create the table client
            CloudTableClient tableClient = _storageAccount.CreateCloudTableClient();

            //Create the CloudTable that represents the "people" table.
            CloudTable table = tableClient.GetTableReference(_tableName);

            // Create a retrieve operation that expects a customer entity.
            TableOperation retrieveOperation = TableOperation.Retrieve<T>(tableEntity.PartitionKey, tableEntity.RowKey);

            // Execute the operation.
            TableResult retrievedResult = table.Execute(retrieveOperation);

            // Assign the result to a CustomerEntity.
            TableEntity deleteEntity = (TableEntity)retrievedResult.Result;

            // Create the Delete TableOperation.
            if (deleteEntity != null)
            {
                TableOperation deleteOperation = TableOperation.Delete(deleteEntity);

                // Execute the operation.
                table.Execute(deleteOperation);
            }
        }

        /// <summary>
        /// Deletes the table.
        /// </summary>
        public void DeleteTable()
        {
            // Create the table client.
            CloudTableClient tableClient = _storageAccount.CreateCloudTableClient();

            //Create the CloudTable that represents the "people" table.
            CloudTable table = tableClient.GetTableReference(_tableName);

            // Delete the table it if exists.
            table.DeleteIfExists();
        }
    }
}