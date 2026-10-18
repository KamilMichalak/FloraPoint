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

        //pobierz wszystkie zdjęcia
        [HttpGet]
        [ProducesResponseType(200)]
        public async Task<IActionResult> GetAllPhotos()
        {
            var photos = await _photoService.GetAllPhotosAsync();
            return Ok(photos);
        }

        //pobierz zdjęcie po ID
        [HttpGet("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetPhotoById(string id)
        {
            var photo = await _photoService.GetPhotoByIdAsync(id);
            if (photo == null)
            {
                return NotFound(new { message = "Nie znaleziono zdjęcia o podanym ID." });
            }

            return Ok(photo);
        }

        //pobierz zdjęcia konkretnej rośliny
        [HttpGet("plant/{plantId}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
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

        //pobierz zdjęcia z danego obszaru (+ danej rośliny)
        [HttpPost("area")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetPhotosInArea([FromBody] AreaSearchDto areaDto)
        {
            if (areaDto == null)
            {
                return BadRequest(new { message = "Nieprawidłowe parametry wyszukiwania obszaru." });
            }

            if (areaDto.PlantIds != null && areaDto.PlantIds.Count > 0)
            {
                foreach (var plantId in areaDto.PlantIds)
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

        //dodaj nowe zdjęcie
        [HttpPost]
        [ProducesResponseType(typeof(Photo), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
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

        //zgłoś zdjęcie
        [HttpPost("{id}/report")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
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

        //usuń zdjęcie
        [HttpDelete("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
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

        [HttpGet("{id}/image")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetPhotoImage(string id)
        {
            var base64Image = await _photoService.GetPhotoImageAsync(id);
            if (string.IsNullOrEmpty(base64Image))
            {
                return NotFound(new { message = "Nie znaleziono obrazka dla podanego ID." });
            }

            return Ok(new { image = base64Image });
        }
    }
}