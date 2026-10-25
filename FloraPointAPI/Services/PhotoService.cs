using FloraPointAPI.DTOs;
using FloraPointAPI.Models;
using FloraPointAPI.Scripts;
using Geohash;
using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Http.HttpResults;

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

        public async Task<List<Photo>> GetPhotosInAreaAsync(double minLat,double maxLat,double minLng,double maxLng,List<string>? plantIds = null)
        {

            CollectionReference photosRef = _firestoreDb.Collection("Photos");



            //oblicznie parametrów okręgu
            double midLat = (minLat + maxLat) / 2;
            double midLng = (minLng + maxLng) / 2;
            double lat = (maxLat - minLat) * 111000; // na metry
            double lng = (maxLng - minLng) * 111000 *Math.Cos(midLat * Math.PI / 180.0);
            double diagonal = Math.Sqrt((lat * lat)+(lng * lng));
            double radius = diagonal / 2.0;

            
            var radiusHasher = new RadiusHasher();
            int precision = 1;
            HashSet<string> hashes = new HashSet<string>();

            
            while (precision< 7)
            {
                var newHashes = radiusHasher.GetHashes(midLat, midLng, radius, precision + 1);

                if (newHashes.Count <= 30)
                {
                    precision++;
                    hashes = newHashes;
                }
                else
                {
                    break;
                }
            }

            // podstawowo precyzja 1 jak się nie wykonało
            if (!hashes.Any())
            {
                hashes = radiusHasher.GetHashes(midLat, midLng, radius, precision);
            }

            var uHashes = hashes.ToList();



            //debug
            //Console.WriteLine($"precyzja: {precision}, liczba hashy: {uHashes.Count}");
            //Console.WriteLine($"hashe: {string.Join(", ", uHashes)}");

            if (!uHashes.Any())
            {
                return new List<Photo>();
            }

            // w bazie jest precyzja 7 więc szukanie przedziałowe
            var tasks = uHashes.Select(async h =>
            {
                Query query = photosRef
                    .WhereGreaterThanOrEqualTo("geohash", h)
                    .WhereLessThanOrEqualTo("geohash", h + "~");

                var snapshot = await query.GetSnapshotAsync();
                return snapshot.Documents;
            });

            var results = await Task.WhenAll(tasks);
            //łączenie wyników/usuwanie duplikatów
            var documents = results
                .SelectMany(docs => docs)
                .GroupBy(doc => doc.Id)
                .Select(g => g.First())
                .ToList();

            //filtr po lokalizacji tutaj, nie przy samym pobieraniu z bazy bo nazwy typów się mieszają i nic nie zwraca
            var photos = documents
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

            //zapisanie obrazka w innej kolekcji
            var imageObj = new Image { Data = base64String };
            CollectionReference imagesRef = _firestoreDb.Collection("Images");
            DocumentReference imageDocRef = await imagesRef.AddAsync(imageObj);

            var geohasher = new Geohasher();
            var calculatedGeohash = geohasher.Encode(dto.Location.Latitude, dto.Location.Longitude, 7);

            var photo = new Photo
            {
                PlantId = dto.PlantId,
                UserId = dto.UserId,
                Species = dto.Species,
                ImageId = imageDocRef.Id,
                Location = dto.Location,
                Geohash = calculatedGeohash,
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

            //usuwanie obrazka
            if (!string.IsNullOrEmpty(photo.ImageId))
            {
                DocumentReference imgDocRef = _firestoreDb.Collection("Images").Document(photo.ImageId);
                await imgDocRef.DeleteAsync();
            }

            //usuwanie reszty
            await docRef.DeleteAsync();
            return true;
        }

        public async Task<string?> GetPhotoImageAsync(string photoId)
        {
            //najpierw pobierz całe zdjęcie
            DocumentReference photoDocRef = _firestoreDb.Collection("Photos").Document(photoId);
            DocumentSnapshot photoSnapshot = await photoDocRef.GetSnapshotAsync();

            if (!photoSnapshot.Exists) return null;

            var photo = photoSnapshot.ConvertTo<Photo>();
            if (string.IsNullOrEmpty(photo.ImageId)) return null;

            //pobierz sam obrazek po id
            DocumentReference imgDocRef = _firestoreDb.Collection("Images").Document(photo.ImageId);
            DocumentSnapshot imgSnapshot = await imgDocRef.GetSnapshotAsync();

            if (!imgSnapshot.Exists) return null;

            var image = imgSnapshot.ConvertTo<Image>();
            return image.Data;
        }


        public async Task<List<Photo>> GetPhotosInRadiusAsync(double centerLat, double centerLng, double radiusMeters, List<string>? plantIds = null)
        {
            CollectionReference photosRef = _firestoreDb.Collection("Photos");

            var radiusHasher = new RadiusHasher();
            int precision = 1;
            HashSet<string> hashes = new HashSet<string>();



            while (precision < 7)
            {
                var newHashes = radiusHasher.GetHashes(centerLat, centerLng, radiusMeters, precision + 1);

                if (newHashes.Count <= 30)
                {
                    precision++;
                    hashes = newHashes;
                }
                else
                {
                    break;
                }
            }

            if (!hashes.Any())
            {
                hashes = radiusHasher.GetHashes(centerLat, centerLng, radiusMeters, precision);
            }

            var uHashes = hashes.ToList();

            if (!uHashes.Any())
            {
                return new List<Photo>();
            }

            var tasks = uHashes.Select(async h =>
            {
                Query query = photosRef
                    .WhereGreaterThanOrEqualTo("geohash", h)
                    .WhereLessThanOrEqualTo("geohash", h + "~");

                var snapshot = await query.GetSnapshotAsync();
                return snapshot.Documents;
            });

            var results = await Task.WhenAll(tasks);

            var documents = results
                .SelectMany(docs => docs)
                .GroupBy(doc => doc.Id)
                .Select(g => g.First())
                .ToList();

            var photos = documents
                .Select(doc => doc.ConvertTo<Photo>())
                .Where(p => p.Location != null && GeoUtils.CalculateDistanceMeters(centerLat, centerLng, p.Location.Latitude, p.Location.Longitude) <= radiusMeters);


            if (plantIds != null && plantIds.Any())
            {
                photos = photos.Where(p => plantIds.Contains(p.PlantId));
            }

            return photos.ToList();
        }
    }
}