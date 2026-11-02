using FloraPointAPI.DTOs;
using FloraPointAPI.Models;
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

        /// <summary>
        /// Returns all plants from the database
        /// </summary>
        /// <returns>List of all plants.</returns>
        /// <response code="200">Returns the list of plants.</response>
        [HttpGet]
        [ProducesResponseType(typeof(List<Plant>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllPlants()
        {
            var plants = await _plantService.GetAllPlantsAsync();
            return Ok(plants);
        }

        /// <summary>
        /// Returns a specific plant by its unique ID
        /// </summary>
        /// <param name="id">The unique identifier of the plant</param>
        /// <returns>The specified plant object.</returns>
        /// <response code="200">Returns the plant.</response>
        /// <response code="404">When the plant with the specified ID does not exist.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Plant), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status404NotFound)]
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