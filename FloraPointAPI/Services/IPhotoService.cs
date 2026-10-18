using FloraPointAPI.DTOs;
using FloraPointAPI.Models;

namespace FloraPointAPI.Services
{
    public interface IPhotoService
    {
        Task<List<Photo>> GetAllPhotosAsync();
        Task<Photo?> GetPhotoByIdAsync(string id);
        Task<List<Photo>> GetPhotosByPlantIdAsync(string plantId);

        //wyszukiwanie po lokalizacji (+ konkretna roślina)
        Task<List<Photo>> GetPhotosInAreaAsync(double minLat, double maxLat, double minLng, double maxLng, List<string>? plantIds = null);

        //przy dodawaniu zdjecia kompresja
        Task<string> AddPhotoAsync(CreatePhotoDto dto);
        Task<bool> ReportPhotoAsync(string photoId, string userId);
        Task<bool> DeletePhotoAsync(string photoId, string userId);

        Task<string?> GetPhotoImageAsync(string photoId);
    }
}