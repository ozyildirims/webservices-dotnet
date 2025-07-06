using System.Net;
using HappyCode.NetCoreBoilerplate.Api.BackgroundServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HappyCode.NetCoreBoilerplate.Api.Controllers
{
    /// <summary>
    /// Manages ping and health check operations
    /// </summary>
    [AllowAnonymous]
    [Route("api/pings")]
    public class PingsController : ApiControllerBase
    {
        private readonly IPingService _pingService;

        public PingsController(IPingService pingService)
        {
            _pingService = pingService;
        }

        /// <summary>
        /// Gets the current website ping status code
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Website status code</returns>
        /// <response code="200">Returns the website status code</response>
        [HttpGet("website")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public Task<IActionResult> GetWebsitePingStatusCodeAsync(
            CancellationToken cancellationToken = default)
        {
            var result = _pingService.WebsiteStatusCode;
            return Task.FromResult<IActionResult>(Ok($"{(int)result} ({result})"));
        }

        /// <summary>
        /// Gets a random HTTP status code
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Random status code</returns>
        /// <response code="200">Returns a random status code</response>
        [HttpGet("random")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public Task<IActionResult> GetRandomStatusCodeAsync(
            CancellationToken cancellationToken = default)
        {
            var random = new Random(Guid.NewGuid().GetHashCode());
            int pretender;
            do
            {
                pretender = random.Next(100, 600);
            } while (!Enum.IsDefined(typeof(HttpStatusCode), pretender));
            return Task.FromResult<IActionResult>(Ok($"{pretender} ({(HttpStatusCode)pretender})"));
        }
    }
}
