using CarRentalBackend.Models;
using CarRentalBackend.ModelsDto;
using CarRentalBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CarRentalBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        [HttpPost("register")]
        public async Task<ActionResult<User>> Register([FromBody] RegisterUserDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await authService.RegisterAsync(request);

            if (user is null)
            {
                return BadRequest("User already exists.");
            }

            return Ok(user);
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDto>> Login([FromBody] UserDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await authService.LoginAsync(request);

            if (response is null)
            {
                return BadRequest("Bad password or user does not exist.");
            }

            return Ok(response);
        }

        [HttpPost("refresh-token")]
        public async Task<ActionResult<TokenResponseDto>> RefreshToken([FromBody] RefreshTokenRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await authService.RefreshTokensAsync(request);
            if (result is null || result.AccessToken is null || result.RefreshToken is null)
            {
                return Unauthorized("Invalid refresh token");
            }

            return Ok(result);
        }

        [Authorize]
        [HttpGet("me")]
        public ActionResult<GetCurrentUserResponseDto> GetCurrentUser()
        {
            var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var email = HttpContext.User.FindFirst(ClaimTypes.Email)?.Value;
            var role = HttpContext.User.FindFirst(ClaimTypes.Role)?.Value;

            if (userId == null || email == null || role == null)
            {
                return Unauthorized("User is not authenticated.");
            }

            return new GetCurrentUserResponseDto
            {
                Id = userId,
                Email = email,
                Role = role
            };
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("admin")]
        public IActionResult AdminOnly()
        {
            return Ok("You are an admin!");
        }
    }
}
