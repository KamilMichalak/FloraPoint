using FloraPointAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace FloraPointAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlantsController : ControllerBase
    {
        private readonly IPlantService _plantService;

        public PlantsController(IPlantService plantService)
        {
            _plantService = plantService;
        }

        [HttpGet]
        [ProducesResponseType(200)]
        public async Task<IActionResult> GetAllPlants()
        {
            var plants = await _plantService.GetAllPlantsAsync();
            return Ok(plants);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetPlantById(string id)
        {
            var plant = await _plantService.GetPlantByIdAsync(id);
            if (plant == null)
            {
                return NotFound(new { message = "Nie znaleziono rośliny o podanym ID." });
            }

            return Ok(plant);
        }
    }
}