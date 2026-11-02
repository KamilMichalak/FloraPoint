using FloraPointAPI.DTOs;
using FloraPointAPI.Models;
using FloraPointAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace FloraPointAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PhotosController : ControllerBase
    {
        private readonly IPhotoService _photoService;
        private readonly IPlantService _plantService;

        // plant potrzebny zeby sprawdzic czy istnieje taka roslina przy dodawaniu zdjecia + do wyszukiwania konkretnych roslin
        public PhotosController(IPhotoService photoService, IPlantService plantService)
        {
            _photoService = photoService;
            _plantService = plantService;
        }

        /// <summary>
        /// Returns all photos from the database
        /// </summary>
        /// <returns>List of all photos.</returns>
        /// <response code="200">Returns the list of photos.</response>
        [HttpGet]
        [ProducesResponseType(typeof(List<Photo>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllPhotos()
        {
            var photos = await _photoService.GetAllPhotosAsync();
            return Ok(photos);
        }

        /// <summary>
        /// Returns a specific photo by its ID
        /// </summary>
        /// <param name="id">The unique identifier of the photo</param>
        /// <returns>The requested photo object.</returns>
        /// <response code="200">Returns the photo.</response>
        /// <response code="404">When the photo with the specified ID does not exist.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Photo), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPhotoById(string id)
        {
            var photo = await _photoService.GetPhotoByIdAsync(id);
            if (photo == null)
            {
                return NotFound(new { message = "Nie znaleziono zdjęcia o podanym ID." });
            }

            return Ok(photo);
        }

        /// <summary>
        /// Returns all photos of a specific plant
        /// </summary>
        /// <param name="plantId">The identifier of the plant</param>
        /// <returns>List of photos for the specified plant.</returns>
        /// <response code="200">Returns the list of matching photos.</response>
        /// <response code="404">When the plant with the specified ID does not exist.</response>
        [HttpGet("plant/{plantId}")]
        [ProducesResponseType(typeof(List<Photo>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPhotosByPlantId(string plantId)
        {
            var plantExists = await _plantService.GetPlantByIdAsync(plantId);
            if (plantExists == null)
            {
                return NotFound(new { message = "Nie znaleziono rośliny o podanym ID." });
            }

            var photos = await _photoService.GetPhotosByPlantIdAsync(plantId);
            return Ok(photos);
        }

        /// <summary>
        /// Returns all photos from a specified area
        /// </summary>
        /// <remarks>
        /// Calculates an area using min/max latitude and longitude and uses geohashes to narrow down
        /// the database query. Optionally filters by a list of plant IDs.
        /// </remarks>
        /// <param name="areaDto">Object containing min/max latitude and longitude and optional plant IDs</param>
        /// <returns>List of photos located within the area.</returns>
        /// <response code="200">Returns the list of matching photos.</response>
        /// <response code="400">When parameters are invalid.</response>
        /// <response code="404">When a specified plant does not exist.</response>
        [HttpPost("area-search")]
        [ProducesResponseType(typeof(List<Photo>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPhotosInArea([FromBody] AreaSearchDto areaDto)
        {
            if (areaDto == null)
            {
                return BadRequest(new { message = "Nieprawidłowe parametry." });
            }

            var PlantIds = areaDto.PlantIds?.Where(id => !string.IsNullOrWhiteSpace(id)).ToList();
            if (PlantIds != null && PlantIds.Any())
            {
                foreach (var plantId in PlantIds)
                {
                    var plantExists = await _plantService.GetPlantByIdAsync(plantId);
                    if (plantExists == null)
                    {
                        return NotFound(new { message = $"Nie znaleziono rośliny o ID: {plantId}." });
                    }
                }
            }

            var photos = await _photoService.GetPhotosInAreaAsync(
                areaDto.MinLatitude,
                areaDto.MaxLatitude,
                areaDto.MinLongitude,
                areaDto.MaxLongitude,
                areaDto.PlantIds
            );

            return Ok(photos);
        }

        /// <summary>
        /// Creates a new photo in the database
        /// </summary>
        /// <remarks>
        /// Compresses the uploaded image file, converts it to Base64, and saves it in a separate 'Images' collection. 
        /// Automatically encodes the location into a level-7 geohash for spatial queries.
        /// </remarks>
        /// <param name="photoDto">Form containing image, plant ID, user ID, species, and location</param>
        /// <returns>Created photo object.</returns>
        /// <response code="201">Returns the created photo.</response>
        /// <response code="400">When data is invalid or file is missing.</response>
        /// <response code="404">When the specified plant does not exist.</response>
        [HttpPost]
        [ProducesResponseType(typeof(Photo), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreatePhoto([FromForm] CreatePhotoDto photoDto)
        {
            if (photoDto == null || photoDto.ImageFile == null || photoDto.ImageFile.Length == 0)
            {
                return BadRequest(new { message = "Nieprawidłowe dane lub brak pliku." });
            }

            //sprawdź czy istnieje
            var plantExists = await _plantService.GetPlantByIdAsync(photoDto.PlantId);
            if (plantExists == null)
            {
                return NotFound(new { message = "Nie znaleziono rośliny o podanym ID." });
            }

       
            var createdId = await _photoService.AddPhotoAsync(photoDto);
            var createdPhoto = await _photoService.GetPhotoByIdAsync(createdId);

            return CreatedAtAction(nameof(GetPhotoById), new { id = createdId }, createdPhoto);
        }

        /// <summary>
        /// Reports a photo
        /// </summary>
        /// <param name="id">The identifier of the photo to report</param>
        /// <param name="actionDto">Object containing the user ID of the reporter</param>
        /// <returns>Success message.</returns>
        /// <response code="200">When the photo is successfully reported.</response>
        /// <response code="400">When UserId is missing or photo was already reported by this user.</response>
        /// <response code="404">When the photo does not exist.</response>
        [HttpPost("{id}/report")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ReportPhoto(string id, [FromBody] PhotoActionDto actionDto)
        {
            if (string.IsNullOrWhiteSpace(actionDto.UserId))
            {
                return BadRequest(new { message = "UserId jest wymagany." });
            }

            var photo = await _photoService.GetPhotoByIdAsync(id);
            if (photo == null)
            {
                return NotFound(new { message = "Nie znaleziono zdjęcia o podanym ID." });
            }

            var success = await _photoService.ReportPhotoAsync(id, actionDto.UserId);
            if (!success)
            {
                return BadRequest(new { message = "Zdjęcie zostało już wcześniej zgłoszone przez tego użytkownika." });
            }

            return Ok(new { message = "Zdjęcie zostało pomyślnie zgłoszone." });
        }

        /// <summary>
        /// Deletes a photo by its ID
        /// </summary>
        /// <remarks>
        /// Checks if the user is the creator of the photo.
        /// If authorized, it deletes both the photo metadata and the associated image document.
        /// </remarks>
        /// <param name="id">The unique identifier of the photo to delete</param>
        /// <param name="actionDto">Object containing user ID</param>
        /// <returns>Success message.</returns>
        /// <response code="200">When the photo is successfully deleted.</response>
        /// <response code="400">When UserId is missing.</response>
        /// <response code="403">When the user does not have permission to delete the photo.</response>
        /// <response code="404">When the photo does not exist.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeletePhoto(string id, [FromBody] PhotoActionDto actionDto)
        {
            if (string.IsNullOrWhiteSpace(actionDto.UserId))
            {
                return BadRequest(new { message = "UserId jest wymagany." });
            }

            var photo = await _photoService.GetPhotoByIdAsync(id);
            if (photo == null)
            {
                return NotFound(new { message = "Nie znaleziono zdjęcia o podanym ID." });
            }

            if (photo.UserId != actionDto.UserId)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "Brak uprawnień." });
            }

            await _photoService.DeletePhotoAsync(id, actionDto.UserId);
            return Ok(new { message = "Zdjęcie zostało pomyślnie usunięte." });
        }

        /// <summary>
        /// Returns base64 image string for a specific photo
        /// </summary>
        /// <param name="id">The unique identifier of the photo</param>
        /// <returns>Object containing the base64 image data string.</returns>
        /// <response code="200">Returns the base64 image data.</response>
        /// <response code="404">When the image or photo does not exist.</response>
        [HttpGet("{id}/image")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPhotoImage(string id)
        {
            var base64Image = await _photoService.GetPhotoImageAsync(id);
            if (string.IsNullOrEmpty(base64Image))
            {
                return NotFound(new { message = "Nie znaleziono obrazka dla podanego ID." });
            }

            return Ok(new { image = base64Image });
        }




        /// <summary>
        /// Returns all photos in given distance from specified point
        /// </summary>
        /// <remarks>
        /// Makes use of geohashes to narrow down the search area. Radius is to be provided in meters. 
        /// Optionally, user may specify which plants' photos to look for.
        /// </remarks>
        /// <param name="dto">Object containing point's coordinates, radius in meters and optional plant's ids</param>
        /// <returns>List of photos located in the area.</returns>
        /// <response code="200">Returns list of matching photos.</response>
        /// <response code="400">When the radius is negative, equal zero or is too big.</response>
        /// <response code="404">When specified plant does not exist in the database.</response>
        [HttpPost("radius-search")]
        [ProducesResponseType(typeof(List<Photo>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPhotosInRadius([FromBody] RadiusSearchDto dto)
        {
            if (dto.Radius <= 0)
            {
                return BadRequest(new ErrorDto { Message = "Promień musi być większy niż 0." });
            }

            if (dto.Radius > 2000000)
            {
                return BadRequest(new ErrorDto { Message = "Maksymalny dopuszczalny promień to 2000 km." });
            }


            var PlantIds = dto.PlantIds?.Where(id => !string.IsNullOrWhiteSpace(id)).ToList();
            if (PlantIds != null && PlantIds.Any())
            {
                foreach (var plantId in PlantIds)
                {
                    var plantExists = await _plantService.GetPlantByIdAsync(plantId);

                    if (plantExists == null)
                    {
                        return NotFound(new { message = $"Nie znaleziono rośliny o ID: {plantId}." });

                    }
                }
            }

            var photos = await _photoService.GetPhotosInRadiusAsync(dto.CenterLat,dto.CenterLng,dto.Radius,dto.PlantIds);

            return Ok(photos);
        }
    }
}