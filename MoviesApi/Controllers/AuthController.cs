using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoviesApi.Helpers;
using MoviesApi.Models;
using MoviesApi.Services;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace MoviesApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class AuthController(AuthService _authService, IConfiguration configuration) : ControllerBase 
    {

        [Route("[action]")]
        [HttpPost]
        public async Task<IActionResult> Register([FromBody] RegisterDTO model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _authService.Register(model.FirstName, model.LastName, model.Password, model.Email, model.PhoneNumber, model.Gender.ToString(), model.UserType.ToString(), model.Image);
            if (!result) return BadRequest("User could not be registered.");

            return Ok("User registered successfully.");
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> Login([FromBody] LoginDTO model)
        {
            var user = await _authService.Authenticate(model.Email, model.Password);
            var userImage = "";
            if (user == null) return Unauthorized("Invalid credentials.");
            
            await using (SqlConnection connection = new SqlConnection(CustomHelpers.GetConnectionString(configuration, "Default")))
            {
                await using (SqlCommand command = new SqlCommand("dbo.GetUserImage", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    // Adding the parameters to the command
                    command.Parameters.Add(new SqlParameter("@Id", SqlDbType.VarChar) { Value = user.Id.ToString() });

                    await connection.OpenAsync();

                    // Executing the command and reading the result
                    await using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        if (!reader.HasRows) { return NotFound(new { message = "No image for this user found." }); }

                        while (await reader.ReadAsync())
                        {
                            userImage = CustomHelpers.GetSafeString(reader, 0);
                            };
                        }
                    }
                    await connection.CloseAsync();
                }
            string token = await _authService.GenerateJwtToken(user);
            encrypt userJson = await _authService.GenerateEncrpyted(user);
            encrypt userImageJson = await _authService.GenerateEncrpytedString(userImage);
            return Ok(new { token, userJson, userImageJson });
        }
    }
}
