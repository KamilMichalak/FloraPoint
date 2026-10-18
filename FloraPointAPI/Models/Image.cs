using Google.Cloud.Firestore;

namespace FloraPointAPI.Models
{
    [FirestoreData]
    public class Image
    {
        [FirestoreDocumentId]
        public string Id { get; set; } = string.Empty;

        [FirestoreProperty("data")]
        public string Data { get; set; } = string.Empty;
    }
}