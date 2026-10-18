using FloraPointAPI.Models;
using Google.Cloud.Firestore;

namespace FloraPointAPI.Services
{
    public class PlantService : IPlantService
    {
        private readonly FirestoreDb _firestoreDb;

        public PlantService(FirestoreDb firestoreDb)
        {
            _firestoreDb = firestoreDb;
        }

        public async Task<List<Plant>> GetAllPlantsAsync()
        {
            CollectionReference plantsRef = _firestoreDb.Collection("Plants");
            QuerySnapshot snapshot = await plantsRef.GetSnapshotAsync();
            return snapshot.Documents.Select(doc => doc.ConvertTo<Plant>()).ToList();
        }

        public async Task<Plant?> GetPlantByIdAsync(string id)
        {
            DocumentReference docRef = _firestoreDb.Collection("Plants").Document(id);
            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

            if (!snapshot.Exists) return null;

            return snapshot.ConvertTo<Plant>();
        }
    }
}