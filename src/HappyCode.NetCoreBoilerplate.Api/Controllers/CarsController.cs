using HappyCode.NetCoreBoilerplate.Core;
using HappyCode.NetCoreBoilerplate.Core.Dtos;
using HappyCode.NetCoreBoilerplate.Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;

namespace HappyCode.NetCoreBoilerplate.Api.Controllers
{
    /// <summary>
    /// Manages car-related operations
    /// </summary>
    [FeatureGate(FeatureFlags.DockerCompose)]
    [Route("api/cars")]
    public class CarsController : ApiControllerBase
    {
        private readonly ICarService _carService;

        public CarsController(ICarService carService)
        {
            _carService = carService;
        }

        /// <summary>
        /// Gets all cars sorted by plate number
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of all cars</returns>
        /// <response code="200">Returns the list of cars</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<CarDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            var result = await _carService.GetAllSortedByPlateAsync(cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Gets Santa's special car (feature flag protected)
        /// </summary>
        /// <returns>Santa's magic sleigh car</returns>
        /// <response code="200">Returns Santa's car</response>
        [FeatureGate(FeatureFlags.Santa)]
        [HttpGet("santa")]
        [ProducesResponseType(typeof(CarDto), StatusCodes.Status200OK)]
        public IActionResult GetSantaCar()
        {
            return Ok(new CarDto
            {
                Id = int.MaxValue,
                Model = "Magic Sleigh",
                Plate = "XMas 12",
            });
        }
    }
}
