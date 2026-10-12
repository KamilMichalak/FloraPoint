using System.Diagnostics;
using System.IO;
using Microsoft.AspNetCore.Http;

namespace FloraPointAPI.Services
{
    public class ImageProcessingService : IImageProcessingService
    {
        public async Task<string> CompressAndConvertAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("Plik jest pusty lub nieprawidłowy.", nameof(file));
            }

            var tempInputPath = Path.GetTempFileName();
            var tempOutputPath = Path.GetTempFileName() + ".jpg";

            try
            {
                //tymczasowo zapisz na dysk
                using (var stream = new FileStream(tempInputPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                //sciezka do skryptu
                var scriptPath = Path.Combine(Directory.GetCurrentDirectory(), "Scripts", "ImgProcessor.py");

                //konfiguracja uruchomienia
                var startInfo = new ProcessStartInfo
                {
                    FileName = "python",
                    Arguments = $"\"{scriptPath}\" \"{tempInputPath}\" \"{tempOutputPath}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(startInfo))
                {
                    if (process != null)
                    {
                        string errors = await process.StandardError.ReadToEndAsync();
                        await process.WaitForExitAsync();

                        if (process.ExitCode != 0)
                        {
                            throw new Exception($"Błąd kompresji: {errors}");
                        }
                    }
                    else
                    {
                        throw new Exception("Nie udało się uruchomić procesu.");
                    }
                }

                //konwertuj na base64
                var compressedBytes = await File.ReadAllBytesAsync(tempOutputPath);
                string base64Data = Convert.ToBase64String(compressedBytes);

                return $"data:image/jpeg;base64,{base64Data}";
            }
            finally
            {
                //usun pliki tymczasowe
                if (File.Exists(tempInputPath)) File.Delete(tempInputPath);
                if (File.Exists(tempOutputPath)) File.Delete(tempOutputPath);
            }
        }
    }
}