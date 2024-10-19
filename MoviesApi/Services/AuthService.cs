using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using MoviesApi.Domain.DatabaseContext;
using MoviesApi.Domain.IdentityEntities;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MoviesApi.Services
{
    public class AuthService(IServiceProvider _serviceProvider, IConfiguration _config,
        UserManager<ApplicationUser> _userManager, RoleManager<ApplicationRole> _roleManager)
    {
        // Register a new user
        public async Task<bool> Register(string FirstName, string LastName, string password, string email, string PhoneNumber, string role)
        {
            string username = LastName.Substring(0, 1) + FirstName;

            ApplicationUser user = new()
            {
                Name = FirstName + ' ' + LastName,
                UserName = username,
                Email = email,
                PhoneNumber = PhoneNumber
            };

            // Create the user with UserManager
            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                // Check if role exists, if not, create it
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new ApplicationRole { Name = role });
                }

                // Assign the role to the user
                await _userManager.AddToRoleAsync(user, role);

                return true;
            }

            return false;
        }

        // Authenticate a user
        public async Task<ApplicationUser> Authenticate(string username, string password)
        {
            // Find the user by username
            var user = await _userManager.FindByNameAsync(username);
            if (user == null) return null;

            // Verify the password
            var result = await _userManager.CheckPasswordAsync(user, password);
            return result ? user : null;
        }

        // Method to generate JWT token
        public async Task<string> GenerateJwtToken(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user); // This is an async call

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


    }
}
