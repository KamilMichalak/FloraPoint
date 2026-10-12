using Google.Cloud.Firestore;

namespace FloraPointAPI.Models
{
    [FirestoreData]
    public class Photo
    {
        [FirestoreDocumentId]
        public string Id { get; set; } = string.Empty;

        [FirestoreProperty("plantId")]
        public string PlantId { get; set; } = string.Empty;

        [FirestoreProperty("userId")]
        public string UserId { get; set; } = string.Empty;

        [FirestoreProperty("species")]
        public string Species { get; set; } = string.Empty;

        //zdjecie w base64
        [FirestoreProperty("image")]
        public string Image { get; set; } = string.Empty;

        [FirestoreProperty("location")]
        public Google.Type.LatLng Location { get; set; } = new Google.Type.LatLng();

        //liczba zgłoszeń
        [FirestoreProperty("reportCount")]
        public int ReportCount { get; set; } = 0;

        //użytkownicy którzy zgłosili zdjęcie
        [FirestoreProperty("reportedBy")]
        public List<string> ReportedBy { get; set; } = new List<string>();

        [FirestoreProperty("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}