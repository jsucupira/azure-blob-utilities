using System;
using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage.Blob;

namespace AzureUtilities.Blobs
{
    /// <summary>
    /// Interface IAzureBlobUtility
    /// </summary>
    public interface IAzureBlobUtility
    {
        /// <summary>
        /// BLOBs the list.
        /// </summary>
        /// <returns>IEnumerable&lt;System.String&gt;.</returns>
        IEnumerable<string> BlobList();

        /// <summary>
        /// Checks if file exists.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        bool CheckIfFileExists(string fileName);

        /// <summary>
        /// Creates the container.
        /// </summary>
        /// <param name="accessType">Type of the access.</param>
        void CreateContainer(BlobContainerPublicAccessType accessType = BlobContainerPublicAccessType.Off);

        /// <summary>
        /// Deletes the blobs.
        /// </summary>
        /// <param name="blobName">Name of the BLOB.</param>
        void DeleteBlobs(string blobName);

        /// <summary>
        /// Downloads the BLOB as file.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="newFileName">New name of the file.</param>
        void DownloadBlobAsFile(string filePath, string fileName, string newFileName = null);

        /// <summary>
        /// Downloads the BLOB as text.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns>System.String.</returns>
        string DownloadBlobAsText(string fileName);

        /// <summary>
        /// Uploads the BLOB.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns>Uri.</returns>
        Uri UploadBlob(string filePath);

        /// <summary>
        /// Uploads the BLOB.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="blobName">Name of the BLOB.</param>
        /// <returns>Uri.</returns>
        Uri UploadBlob(string content, string blobName);

        /// <summary>
        /// Gets or sets the connection string.
        /// </summary>
        /// <value>The connection string.</value>
        string ConnectionString { get; set; }
        /// <summary>
        /// Gets or sets the name of the container.
        /// </summary>
        /// <value>The name of the container.</value>
        string ContainerName { get; set; }
    }
}