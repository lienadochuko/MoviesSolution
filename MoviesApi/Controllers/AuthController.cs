using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoviesApi.Models;
using MoviesApi.Services;

namespace MoviesApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class AuthController(AuthService _authService) : ControllerBase 
    {

        [Route("[action]")]
        [HttpPost]
        public async Task<IActionResult> Register([FromBody] RegisterDTO model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _authService.Register(model.FirstName, model.LastName, model.Password, model.Email, model.PhoneNumber, model.role);
            if (!result) return BadRequest("User could not be registered.");

            return Ok("User registered successfully.");
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> Login([FromBody] LoginDTO model)
        {
            var user = await _authService.Authenticate(model.Email, model.Password);
            if (user == null) return Unauthorized("Invalid credentials.");

            var token = _authService.GenerateJwtToken(user);
            return Ok(new { token, user });
        }
    }
}
