using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using AzureUtilities.Blobs;
using Microsoft.WindowsAzure.Storage.Blob;

namespace AzureUtilities.Mock
{
    [Export(typeof (IAzureBlobUtility))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class MockBlobStorage : IAzureBlobUtility
    {
        private string _connectionString;
        private string _containerName;

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
            CheckContainer();
            string key = $"BlobStorage|{ContainerName}";
            List<string> keys = MockValues.GetAllKeys(key);
            List<string> blobNames = new List<string>();
            foreach (string fullKey in keys)
            {
                List<string> parts = fullKey.Split('|').ToList();
                blobNames.Add(parts.Last());
            }
            return blobNames;
        }

        public bool CheckIfFileExists(string fileName)
        {
            CheckContainer();
            return BlobList().Any(t => t.Equals(fileName, StringComparison.OrdinalIgnoreCase));
        }

        public void CreateContainer(BlobContainerPublicAccessType accessType = BlobContainerPublicAccessType.Off)
        {
            CheckContainer();
        }

        public void DeleteBlobs(string blobName)
        {
            CheckContainer();
            throw new NotImplementedException();
        }

        public void DownloadBlobAsFile(string filePath, string fileName, string overrideName = "")
        {
            CheckContainer();
            FileInfo file;
            if (!string.IsNullOrEmpty(overrideName))
                file = new FileInfo(filePath + @"\" + overrideName);
            else
                file = new FileInfo(filePath + @"\" + fileName);

            file.Directory?.Create();

            string key = $"BlobStorage|{ContainerName}|{fileName}";
            object content = MockValues.GetValue(key);
            using (StreamWriter fs = File.CreateText(file.FullName))
                fs.Write(content);
        }

        public string DownloadBlobAsText(string fileName)
        {
            CheckContainer();
            string key = $"BlobStorage|{ContainerName}|{fileName}";
            return MockValues.GetValue(key).ToString();
        }

        public Uri UploadBlob(string filePath)
        {
            CheckContainer();
            string fileContent = File.ReadAllText(filePath);
            string blobName = Path.GetFileName(filePath);
            return UploadBlob(fileContent, blobName);
        }

        public Uri UploadBlob(string content, string blobName)
        {
            CheckContainer();
            string key = $"BlobStorage|{ContainerName}|{blobName}";
            MockValues.SetValue(key, content);
            return new Uri("http://www.blob.com/" + ContainerName + "/" + blobName);
        }

        public string ConnectionString
        {
            get { return _connectionString; }
            set
            {
                if (string.IsNullOrEmpty(_connectionString))
                    _connectionString = value;
                else
                    throw new InvalidOperationException("Connections string can only be set once.");
            }
        }

        private void CheckContainer()
        {
            if (string.IsNullOrEmpty(ContainerName))
                throw new ApplicationException("Container name cannot be null");

            if (string.IsNullOrEmpty(ConnectionString))
                throw new ApplicationException("ConnectionString cannot be null");
        }
    }
}