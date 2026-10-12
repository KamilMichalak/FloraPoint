using FloraPointAPI.DTOs;
using FloraPointAPI.Models;
using Google.Cloud.Firestore;

namespace FloraPointAPI.Services
{
    public class PhotoService : IPhotoService
    {
        private readonly FirestoreDb _firestoreDb;
        private readonly IImageProcessingService _imageProcessingService;

        public PhotoService(FirestoreDb firestoreDb, IImageProcessingService imageProcessingService)
        {
            _firestoreDb = firestoreDb;
            _imageProcessingService = imageProcessingService;
        }

        public async Task<List<Photo>> GetAllPhotosAsync()
        {
            CollectionReference photosRef = _firestoreDb.Collection("Photos");
            QuerySnapshot snapshot = await photosRef.GetSnapshotAsync();
            return snapshot.Documents.Select(doc => doc.ConvertTo<Photo>()).ToList();
        }

        public async Task<Photo?> GetPhotoByIdAsync(string id)
        {
            DocumentReference docRef = _firestoreDb.Collection("Photos").Document(id);
            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

            if (!snapshot.Exists) return null;

            return snapshot.ConvertTo<Photo>();
        }

        public async Task<List<Photo>> GetPhotosByPlantIdAsync(string plantId)
        {
            CollectionReference photosRef = _firestoreDb.Collection("Photos");
            Query query = photosRef.WhereEqualTo("plantId", plantId);
            QuerySnapshot snapshot = await query.GetSnapshotAsync();

            return snapshot.Documents.Select(doc => doc.ConvertTo<Photo>()).ToList();
        }

        public async Task<List<Photo>> GetPhotosInAreaAsync(
    double minLat,
    double maxLat,
    double minLng,
    double maxLng,
    List<string>? plantIds = null)
        {
            CollectionReference photosRef = _firestoreDb.Collection("Photos");

            QuerySnapshot snapshot = await photosRef.GetSnapshotAsync();
            
            //filtr po lokalizacji tutaj, nie przy samym pobieraniu z bazy bo nazwy typów się mieszają i nic nie zwraca
            var photos = snapshot.Documents
                .Select(doc => doc.ConvertTo<Photo>())
                .Where(p => p.Location != null
                         && p.Location.Latitude >= minLat
                         && p.Location.Latitude <= maxLat
                         && p.Location.Longitude >= minLng
                         && p.Location.Longitude <= maxLng);

            //filtr po rośliach
            if (plantIds != null && plantIds.Any())
            {
                photos = photos.Where(p => plantIds.Contains(p.PlantId));
            }

            return photos.ToList();
        }

        public async Task<string> AddPhotoAsync(CreatePhotoDto dto)
        {
            string base64String = string.Empty;

            //skrypt do kompresji
            if (dto.ImageFile != null && dto.ImageFile.Length > 0)
            {
                base64String = await _imageProcessingService.CompressAndConvertAsync(dto.ImageFile);
            }

            var photo = new Photo
            {
                PlantId = dto.PlantId,
                UserId = dto.UserId,
                Species = dto.Species,
                Image = base64String,
                Location = dto.Location,
                CreatedAt = DateTime.UtcNow,
                ReportCount = 0,
                ReportedBy = new List<string>()
            };

            CollectionReference photosRef = _firestoreDb.Collection("Photos");
            DocumentReference docRef = await photosRef.AddAsync(photo);
            return docRef.Id;
        }


        public async Task<bool> ReportPhotoAsync(string photoId, string userId)
        {
            DocumentReference docRef = _firestoreDb.Collection("Photos").Document(photoId);
            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

            if (!snapshot.Exists) return false;

            Photo photo = snapshot.ConvertTo<Photo>();

            //odrzuć jeśli użytkownik już zgłosił
            if (photo.ReportedBy != null && photo.ReportedBy.Contains(userId))
            {
                return false;
            }

            //jeśli nikt nie zgłosił inicjuj listę
            photo.ReportedBy ??= new List<string>();
            photo.ReportedBy.Add(userId);
            photo.ReportCount += 1;

            await docRef.UpdateAsync(new Dictionary<string, object>
            {
                { "reportedBy", photo.ReportedBy },
                { "reportCount", photo.ReportCount }
            });

            return true;
        }

        public async Task<bool> DeletePhotoAsync(string photoId, string userId)
        {
            DocumentReference docRef = _firestoreDb.Collection("Photos").Document(photoId);
            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

            if (!snapshot.Exists) return false;

            Photo photo = snapshot.ConvertTo<Photo>();

            //tylko ten kto stworzył może usunąć
            if (photo.UserId != userId)
            {
                return false;
            }

            await docRef.DeleteAsync();
            return true;
        }
    }
}