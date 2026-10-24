
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Sanay3y.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        [Authorize]
        [HttpGet("me")]
        public IActionResult GetCurrentUser()
        {
            return Ok(new
            {
                id = User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? User.FindFirstValue("sub"),

                fullName = User.FindFirstValue(ClaimTypes.Name)
                    ?? User.Identity?.Name,

                phoneNumber = User.FindFirstValue("phoneNumber"),

                email = User.FindFirstValue(ClaimTypes.Email)
                    ?? User.FindFirstValue("email"),

                role = User.FindFirstValue(ClaimTypes.Role)
            });
        }
    }
}
