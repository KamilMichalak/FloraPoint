using FloraPointAPI.Models;

namespace FloraPointAPI.Services
{
    public interface IPlantService
    {
        Task<List<Plant>> GetAllPlantsAsync();
        Task<Plant?> GetPlantByIdAsync(string id);
    }
}