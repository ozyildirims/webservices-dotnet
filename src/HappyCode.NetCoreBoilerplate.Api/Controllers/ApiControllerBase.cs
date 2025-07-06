using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HappyCode.NetCoreBoilerplate.Api.Controllers
{
    [Produces("application/json")]
    [ApiController]
    public abstract class ApiControllerBase : ControllerBase
    {
        protected Guid UserId
        {
            get
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                return userIdClaim != null ? Guid.Parse(userIdClaim) : Guid.Empty;
            }
        }
    }
}
