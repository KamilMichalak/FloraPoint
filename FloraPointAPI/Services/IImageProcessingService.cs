using Microsoft.AspNetCore.Http;

namespace FloraPointAPI.Services
{
    public interface IImageProcessingService
    {
        Task<string> CompressAndConvertAsync(IFormFile file);
    }
}