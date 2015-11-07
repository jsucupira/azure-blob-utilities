using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureUtilities.Tables
{
    /// <summary>
    /// Interface IAzureTableUtility
    /// </summary>
    public interface IAzureTableUtility
    {
        /// <summary>
        /// Creates the table.
        /// </summary>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        bool CreateTable();

        /// <summary>
        /// Deletes the batch.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <returns>IList&lt;TableResult&gt;.</returns>
        IList<TableResult> DeleteBatch(IEnumerable<TableEntity> entities);

        /// <summary>
        /// Deletes the entity.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tableEntity">The table entity.</param>
        void DeleteEntity<T>(TableEntity tableEntity) where T : TableEntity, new();

        /// <summary>
        /// Deletes the table.
        /// </summary>
        void DeleteTable();

        /// <summary>
        /// Executes the query.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="exQuery">The ex query.</param>
        /// <returns>List&lt;T&gt;.</returns>
        List<T> ExecuteQuery<T>(TableQuery<T> exQuery) where T : TableEntity, new();

        /// <summary>
        /// Finds the by.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="partitionKey">The partition key.</param>
        /// <param name="rowKey">The row key.</param>
        /// <returns>IEnumerable&lt;T&gt;.</returns>
        T FindBy<T>(string partitionKey, string rowKey) where T : TableEntity, new();

        /// <summary>
        /// Finds the by partition key.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="partitionKey">The partition key.</param>
        /// <returns>IEnumerable&lt;T&gt;.</returns>
        IEnumerable<T> FindByPartitionKey<T>(string partitionKey) where T : TableEntity, new();

        /// <summary>
        /// Adds the item to table.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>TableResult.</returns>
        TableResult Insert(TableEntity item);

        /// <summary>
        /// Queries the by property.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="property">The property.</param>
        /// <returns>IEnumerable&lt;System.String&gt;.</returns>
        IEnumerable<string> QueryByProperty<T>(string property) where T : TableEntity, new();

        /// <summary>
        /// Updates the item.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tableEntity">The table entity.</param>
        void UpdateItem<T>(TableEntity tableEntity) where T : TableEntity, new();

        /// <summary>
        /// Upserts the batch.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <returns>IList&lt;TableResult&gt;.</returns>
        IList<TableResult> UpsertBatch(IEnumerable<TableEntity> entities);

        /// <summary>
        /// Upsets the specified table entity.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tableEntity">The table entity.</param>
        /// <returns>TableResult.</returns>
        TableResult Upset<T>(TableEntity tableEntity) where T : TableEntity, new();

        string ConnectionString { get; set; }

        string TableName { get; set; }
    }
}