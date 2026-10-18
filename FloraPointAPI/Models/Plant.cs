using Google.Cloud.Firestore;

namespace FloraPointAPI.Models
{
    [FirestoreData]
    public class Plant
    {
        [FirestoreDocumentId]
        public string Id { get; set; } = string.Empty;

        [FirestoreProperty("species")]
        public string Species { get; set; } = string.Empty;

        [FirestoreProperty("description")]
        public string Description { get; set; } = string.Empty;

        //zdjecie w base64
        [FirestoreProperty("image")]
        public string Image { get; set; } = string.Empty;

    }
}