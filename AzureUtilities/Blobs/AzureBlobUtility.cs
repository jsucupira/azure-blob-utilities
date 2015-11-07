using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace AzureUtilities.Blobs
{
    [Export(typeof (IAzureBlobUtility))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class AzureBlobUtility : IAzureBlobUtility
    {
        private string _connectionString;
        private CloudBlobContainer _container;
        private string _containerName;

        private CloudStorageAccount _storageAccount;

        public string ContainerName
        {
            get { return _containerName; }
            set
            {
                if (_containerName != null)
                    throw new InvalidOperationException("ContainerName can only be set once.");
                _containerName = value;
            }
        }

        public IEnumerable<string> BlobList()
        {
            List<string> names = new List<string>();
            IEnumerable<IListBlobItem> items = _container.ListBlobs();
            foreach (IListBlobItem item in items)
            {
                string name = item.Uri.Segments[item.Uri.Segments.Length - 1];
                names.Add(name);
            }
            return names;
        }

        public bool CheckIfFileExists(string fileName)
        {
            // Retrieve reference to a blob named "myblob.txt"
            CloudBlockBlob blockBlob2 = _container.GetBlockBlobReference(fileName);
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

        public void CreateContainer(BlobContainerPublicAccessType accessType = BlobContainerPublicAccessType.Off)
        {
            _container.SetPermissions(new BlobContainerPermissions {PublicAccess = accessType});

            // Create the container if it doesn't already exist.
            _container.CreateIfNotExists();
        }

        public void DeleteBlobs(string blobName)
        {
            // Retrieve reference to a blob named "myblob.txt".
            CloudBlockBlob blockBlob = _container.GetBlockBlobReference(blobName);

            // Delete the blob.
            blockBlob.Delete();
        }

        public void DownloadBlobAsFile(string filePath, string fileName, string newFileName = null)
        {
            FileInfo file;
            if (!string.IsNullOrEmpty(newFileName))
                file = new FileInfo(filePath + @"\" + newFileName);
            else
                file = new FileInfo(filePath + @"\" + fileName);

            if (file.Directory != null) file.Directory.Create(); // If the directory already exists, this method does nothing.
            // Create the blob client.

            // Retrieve reference to a blob named "myblob.txt"
            CloudBlockBlob blockBlob = _container.GetBlockBlobReference(fileName);
            blockBlob.DownloadToFile(file.FullName, FileMode.Create);
        }

        public string DownloadBlobAsText(string fileName)
        {
            CloudBlockBlob blockBlob = _container.GetBlockBlobReference(fileName);
            if (!blockBlob.Exists())
                return null;
            return blockBlob.DownloadText();
        }

        public Uri UploadBlob(string filePath)
        {
            string fileContent = File.ReadAllText(filePath);
            string blobName = Path.GetFileName(filePath);
            return UploadBlob(fileContent, blobName);
        }

        public Uri UploadBlob(string content, string blobName)
        {
            CloudBlockBlob blob = _container.GetBlockBlobReference(blobName);
            blob.UploadText(content);
            return blob.Uri;
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
                    CloudBlobClient client = _storageAccount.CreateCloudBlobClient();
                    _container = client.GetContainerReference(ContainerName);
                    _container.CreateIfNotExists();
                }
                else
                    throw new InvalidOperationException("Connections string can only be set once.");
            }
        }
    }
}