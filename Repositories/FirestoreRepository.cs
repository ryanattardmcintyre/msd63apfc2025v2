using Google.Cloud.Firestore;
using Microsoft.CodeAnalysis;

namespace PFCWebApplication.Repositories
{
    public class FirestoreRepository
    {
        FirestoreDb db;
        public FirestoreRepository(string project) {
            //code that initializes the firestore db
            db = FirestoreDb.Create(project);
        }

        public async Task<WriteResult> AddUser(string email, string firstName, string lastName) {

            DocumentReference docRef = db.Collection("users").Document(email);
            Dictionary<string, object> user = new Dictionary<string, object>
            {
                { "email", email },
                { "firstName", firstName},
                { "lastName", lastName }
            };

            //it saving the user asynchronously...
            return await docRef.SetAsync(user);

        }

        public async Task<bool> DoesUserExist(string email)
        {
            DocumentReference docRef = db.Collection("users").Document(email);
            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();
            if (snapshot.Exists)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        
        /// <summary>
        /// This method will be called everytime there's a new login as a NESTED collection
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public async Task<WriteResult> AddLoginLog(string email, string ip, string description)
        {
            DocumentReference docRef = db.Collection("users").Document(email)
                                       .Collection("logs").Document();
            Dictionary<string, object> log = new Dictionary<string, object>
            {
                { "ipAddress", ip },
                { "timestamp", Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow) },
                { "description", description }
            };

            //it saving the user asynchronously...
            return await docRef.SetAsync(log);

        }

        public async Task<WriteResult> AddFile(string ownerEmail, string uniqueFilename)
        {
            DocumentReference docRef = db.Collection("users").Document(ownerEmail)
                                       .Collection("files").Document(uniqueFilename);
            Dictionary<string, object> log = new Dictionary<string, object>
            {
                { "uploadedOn", Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow) } 
            };

            //it saving the user asynchronously...
            return await docRef.SetAsync(log);
        }

        public async Task<List<string>> GetFilesOfUser(string ownerEmail)
        {
            List<string> files = new List<string>();

            // Get the user's nested files collection
            CollectionReference filesCollection = db.Collection("users").Document(ownerEmail).Collection("files");
            QuerySnapshot filesSnapshot = await filesCollection.GetSnapshotAsync();

            foreach (DocumentSnapshot fileDoc in filesSnapshot.Documents)
            {
                files.Add(fileDoc.Id); // Assuming the document ID represents the file name
            }
            return files;
        }


    }
}
