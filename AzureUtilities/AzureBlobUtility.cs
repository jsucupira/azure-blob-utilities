using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace AzureUtilities
{
    /// <summary>
    /// Class AzureBlobUtility.
    /// </summary>
    public class AzureBlobUtility
    {
        /// <summary>
        /// The ma x_ bloc k_ size
        /// </summary>
        private const int MAX_BLOCK_SIZE = 4000000; // Approx. 4MB chunk size
        /// <summary>
        /// The _storage account
        /// </summary>
        private readonly CloudStorageAccount _storageAccount;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureBlobUtility"/> class.
        /// </summary>
        /// <param name="accountConnectionString">The account connection string.</param>
        public AzureBlobUtility(string accountConnectionString)
        {
            _storageAccount = CloudStorageAccount.Parse(accountConnectionString);
        }

        /// <summary>
        /// BLOBs the list.
        /// </summary>
        /// <param name="containerName">Name of the container.</param>
        /// <returns>IEnumerable&lt;System.String&gt;.</returns>
        public IEnumerable<string> BlobList(string containerName)
        {
            List<string> list = new List<string>();
            // Retrieve storage account from connection string.
            // Create the blob client. 
            CloudBlobClient blobClient = _storageAccount.CreateCloudBlobClient();

            // Retrieve reference to a previously created container.
            CloudBlobContainer container = blobClient.GetContainerReference(containerName);

            // Loop over items within the container and output the length and URI.
            foreach (IListBlobItem item in container.ListBlobs(null, false))
            {
                CloudBlockBlob blockBlob = item as CloudBlockBlob;
                if (blockBlob != null)
                {
                    CloudBlockBlob blob = blockBlob;
                    list.Add(blob.Uri.ToString());
                    Console.WriteLine("Block blob of length {0}: {1}", blob.Properties.Length, blob.Uri);
                }
                else
                {
                    CloudPageBlob blob = item as CloudPageBlob;
                    if (blob != null)
                    {
                        CloudPageBlob pageBlob = blob;
                        list.Add(pageBlob.Uri.ToString());
                        Console.WriteLine("Page blob of length {0}: {1}", pageBlob.Properties.Length, pageBlob.Uri);
                    }
                    else
                    {
                        CloudBlobDirectory blobDirectory = item as CloudBlobDirectory;
                        if (blobDirectory != null)
                        {
                            CloudBlobDirectory directory = blobDirectory;
                            list.Add(directory.Uri.ToString());
                            Console.WriteLine("Directory: {0}", directory.Uri);
                        }
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// Checks if file exists.
        /// </summary>
        /// <param name="containerName">Name of the container.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public bool CheckIfFileExists(string containerName, string fileName)
        {
            // Create the blob client.
            CloudBlobClient blobClient = _storageAccount.CreateCloudBlobClient();

            // Retrieve reference to a previously created container.
            CloudBlobContainer container = blobClient.GetContainerReference(containerName);

            // Retrieve reference to a blob named "myblob.txt"
            CloudBlockBlob blockBlob2 = container.GetBlockBlobReference(fileName);
            try
            {
                blockBlob2.FetchAttributes();
                return true;
            }
            catch (StorageException e)
            {
                if (e.Message.Contains("404"))
                    return false;
                throw;
            }
        }

        /// <summary>
        /// Creates the container.
        /// </summary>
        /// <param name="accessType">Type of the access.</param>
        /// <param name="containerName">Name of the container.</param>
        public void CreateContainer(BlobContainerPublicAccessType accessType, string containerName)
        {
            // Retrieve storage account from connection string.
            // Create the blob client.
            CloudBlobClient blobClient = _storageAccount.CreateCloudBlobClient();

            // Retrieve a reference to a container. 
            CloudBlobContainer container = blobClient.GetContainerReference(containerName);
            container.SetPermissions(new BlobContainerPermissions {PublicAccess = accessType});

            // Create the container if it doesn't already exist.
            container.CreateIfNotExists();
        }

        /// <summary>
        /// Deletes the blobs.
        /// </summary>
        /// <param name="containerName">Name of the container.</param>
        /// <param name="blobName">Name of the BLOB.</param>
        public void DeleteBlobs(string containerName, string blobName)
        {
            // Retrieve storage account from connection string.
            // Create the blob client.
            CloudBlobClient blobClient = _storageAccount.CreateCloudBlobClient();

            // Retrieve reference to a previously created container.
            CloudBlobContainer container = blobClient.GetContainerReference(containerName);

            // Retrieve reference to a blob named "myblob.txt".
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(blobName);

            // Delete the blob.
            blockBlob.Delete();
        }

        /// <summary>
        /// Downloads the BLOB as file.
        /// </summary>
        /// <param name="containerName">Name of the container.</param>
        /// <param name="filePath">The file path.</param>
        /// <param name="fileName">Name of the file.</param>
        public void DownloadBlobAsFile(string containerName, string filePath, string fileName)
        {
            string content = DownloadBlobAsText(containerName, fileName);

            FileInfo file = new FileInfo(filePath);
            if (file.Directory != null)
                file.Directory.Create(); // If the directory already exists, this method does nothing.
            File.WriteAllText(file.FullName, content, Encoding.UTF8);
        }

        /// <summary>
        /// Downloads the BLOB as text.
        /// </summary>
        /// <param name="containerName">Name of the container.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <returns>System.String.</returns>
        public string DownloadBlobAsText(string containerName, string fileName)
        {
            // Create the blob client.
            CloudBlobClient blobClient = _storageAccount.CreateCloudBlobClient();

            // Retrieve reference to a previously created container.
            CloudBlobContainer container = blobClient.GetContainerReference(containerName);

            // Retrieve reference to a blob named "myblob.txt"
            CloudBlockBlob blockBlob2 = container.GetBlockBlobReference(fileName);

            string text;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                blockBlob2.DownloadToStream(memoryStream);
                text = Encoding.UTF8.GetString(memoryStream.ToArray());
            }
            return text;
        }

        /// <summary>
        /// Gets the file blocks.
        /// </summary>
        /// <param name="fileContent">Content of the file.</param>
        /// <returns>IEnumerable&lt;FileBlock&gt;.</returns>
        private IEnumerable<FileBlock> GetFileBlocks(byte[] fileContent)
        {
            HashSet<FileBlock> hashSet = new HashSet<FileBlock>();
            if (fileContent.Length == 0)
                return new HashSet<FileBlock>();

            int blockId = 0;
            int ix = 0;

            int currentBlockSize = MAX_BLOCK_SIZE;

            while (currentBlockSize == MAX_BLOCK_SIZE)
            {
                if ((ix + currentBlockSize) > fileContent.Length)
                    currentBlockSize = fileContent.Length - ix;

                byte[] chunk = new byte[currentBlockSize];
                Array.Copy(fileContent, ix, chunk, 0, currentBlockSize);

                hashSet.Add(
                    new FileBlock
                    {
                        Content = chunk,
                        Id = Convert.ToBase64String(BitConverter.GetBytes(blockId))
                    });

                ix += currentBlockSize;
                blockId++;
            }

            return hashSet;
        }

        /// <summary>
        /// Uploads the BLOB.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="containerName">Name of the container.</param>
        /// <returns>Uri.</returns>
        public Uri UploadBlob(string filePath, string containerName)
        {
            byte[] fileContent = File.ReadAllBytes(filePath);
            string blobName = Path.GetFileName(filePath);

            return UploadBlob(fileContent, containerName, blobName);
        }

        /// <summary>
        /// Uploads the BLOB.
        /// </summary>
        /// <param name="fileContent">Content of the file.</param>
        /// <param name="containerName">Name of the container.</param>
        /// <param name="blobName">Name of the BLOB.</param>
        /// <returns>Uri.</returns>
        public Uri UploadBlob(byte[] fileContent, string containerName, string blobName)
        {
            CloudBlobClient blobClient = _storageAccount.CreateCloudBlobClient();

            CloudBlobContainer container = blobClient.GetContainerReference(containerName);

            CloudBlockBlob blob = container.GetBlockBlobReference(blobName);

            HashSet<string> blockList = new HashSet<string>();
            foreach (FileBlock block in GetFileBlocks(fileContent))
            {
                blob.PutBlock(block.Id, new MemoryStream(block.Content, true), null);
                blockList.Add(block.Id);
            }

            blob.PutBlockList(blockList);

            return blob.Uri;
        }
    }
}