namespace FloraPointAPI.DTOs
{
    public class CreatePhotoDto
    {
        //żeby nie trzeba było podawać id i reportów przy tworzeniu + do wysyłąnia zdjęcia
        public string PlantId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public IFormFile ImageFile { get; set; } = null!;
        public string Species { get; set; } = string.Empty;
        public Google.Type.LatLng Location { get; set; } = new Google.Type.LatLng();
    }
}