namespace FloraPointAPI.DTOs
{
    public class CreatePhotoDto
    {
        //żeby nie trzeba było podawać id i reportów przy tworzeniu + do wysyłąnia zdjęcia

        /// <summary>
        /// Unique ID of the photographed plant
        /// </summary>
        public string PlantId { get; set; } = string.Empty;

        /// <summary>
        /// Unique ID of the user sending the photo
        /// </summary>
        public string UserId { get; set; } = string.Empty;
        /// <summary>
        /// Image file
        /// </summary>
        public IFormFile ImageFile { get; set; } = null!;
        /// <summary>
        /// Species name
        /// </summary>
        public string Species { get; set; } = string.Empty;
        /// <summary>
        /// Coordinates of the place where the photo was taken
        /// </summary>
        public Google.Type.LatLng Location { get; set; } = new Google.Type.LatLng();
    }
}