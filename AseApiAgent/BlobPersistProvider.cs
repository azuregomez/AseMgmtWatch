using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Auth;
using Microsoft.Azure.Storage.Blob;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Skokie.Cloud.AseApiAgent
{

    public class BlobPersistProvider: IPersist
    {
        private CloudBlobClient _blobClient = null;
        private CloudBlobContainer _blobContainer = null;
        public BlobPersistProvider(StorageCredentials credentials, string blobContainerName)
        {

            CloudStorageAccount cloudStorage = new CloudStorageAccount(credentials, true);
            _blobClient = cloudStorage.CreateCloudBlobClient();
            _blobContainer = _blobClient.GetContainerReference(blobContainerName.ToLower());
            bool created = _blobContainer.CreateIfNotExists();
            if (created)
            {
                var permissions = new BlobContainerPermissions();
                permissions.PublicAccess = BlobContainerPublicAccessType.Container;
                _blobContainer.SetPermissions(permissions);
            }
        }

        public AseApiRecord Get(string aseName)
        {
            // download blob
            ICloudBlob blob; 
            try
            {
                blob = _blobContainer.GetBlobReferenceFromServer(aseName);
            }
            catch (StorageException ex)
                        when (ex.Message.Equals("The specified blob does not exist."))
            {
                // blob does not exist, do whatever you need here
                return null;
            }
            Stream target = new MemoryStream();
            blob.DownloadToStream(target);
            target.Position = 0;
            StreamReader reader = new StreamReader(target);
            string text = reader.ReadToEnd();
            // deserialize into AseApiRecord
            AseApiRecord record = JsonConvert.DeserializeObject<AseApiRecord>(text);
            return record;
        }

        public void Save(string aseName, AseApiRecord record)
        {
            // serialize and convert to byte array
            string json = JsonConvert.SerializeObject(record);
            byte[] bytes = Encoding.ASCII.GetBytes(json);
            ICloudBlob blob = _blobContainer.GetBlockBlobReference(aseName);
            // delete if previously exists
            blob.DeleteIfExists(DeleteSnapshotsOption.IncludeSnapshots);
            // upload
            blob.UploadFromByteArray(bytes, 0, bytes.Length);
            blob.Properties.ContentType = "ByteArray";
            blob.SetProperties();
        }
    }
}
