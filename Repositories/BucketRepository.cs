using System.Security.AccessControl;
using System.Text;
using Google.Apis.Storage.v1.Data;
using Google.Cloud.Storage.V1;

namespace PFCWebApplication.Repositories
{
    public class BucketRepository
    {

        string _bucketName;
        public BucketRepository(string bucketName)
        {
            _bucketName = bucketName;
        }

        public async Task<Google.Apis.Storage.v1.Data.Object> UploadFile(string uniqueFilename, MemoryStream msUpload)
        {
            msUpload.Position = 0; //reset the position of the stream to upload all the data starting from pos 0
            var storage = StorageClient.Create();
            return await storage.UploadObjectAsync(_bucketName, uniqueFilename, "application/octet-stream", msUpload);
        }

        public async Task<Google.Apis.Storage.v1.Data.Object> GrantAccess(string uniqueFilename, string emailAddress, string role = "READER")
        {
            var storage = StorageClient.Create();
            var storageObject = storage.GetObject(_bucketName, uniqueFilename, new GetObjectOptions
            {
                Projection = Projection.Full
            });

            storageObject.Acl.Add(new ObjectAccessControl
            {
                Bucket = _bucketName,
                Entity = $"user-{emailAddress}",
                Role = role,
            });

            var updatedObject = storage.UpdateObjectAsync(storageObject);
     
            return await updatedObject;
        }

    }
}
