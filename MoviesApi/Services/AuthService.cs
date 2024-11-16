using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using MoviesApi.Domain.DatabaseContext;
using MoviesApi.Domain.IdentityEntities;
using MoviesApi.Helpers;
using MoviesApi.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading;

namespace MoviesApi.Services
{
    public class AuthService(IServiceProvider _serviceProvider, IConfiguration _config, AES aES,
        UserManager<ApplicationUser> _userManager, RoleManager<ApplicationRole> _roleManager)
    {
        // Register a new user
        public async Task<bool> Register(string FirstName, string LastName, string password, string email, string PhoneNumber, string gender, string role, string Image)
        {
            string combined = LastName + FirstName;
            Random random = new Random();
            string username = new string(combined.OrderBy(_ => random.Next()).Take(6).ToArray());

            ApplicationUser user = new()
            {
                Name = FirstName + ' ' + LastName,
                UserName = username,
                Email = email,
                PhoneNumber = PhoneNumber,
                Gender = gender,
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

                // Now handle the UserImage
                await using (SqlConnection connection = new SqlConnection(CustomHelpers.GetConnectionString(_config, "Default")))
                {
                    await using (SqlCommand command = new SqlCommand("dbo.AddUserImage", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        // Adding the parameters to the command
                        command.Parameters.Add(new SqlParameter("@Image", SqlDbType.VarChar) { Value = Image });
                        command.Parameters.Add(new SqlParameter("@Id", SqlDbType.VarChar) { Value = user.Id.ToString() });

                        await connection.OpenAsync();
                        await command.ExecuteNonQueryAsync();  // Execute the stored procedure
                        await connection.CloseAsync();

                    }
                }

                return true;
            }

            return false;
        }


        // Authenticate a user
        public async Task<ApplicationUser> Authenticate(string email, string password)
        {
            // Find the user by username
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return null;

            // Verify the password
            var result = await _userManager.CheckPasswordAsync(user, password);
            return result ? user : null;
        }



        // Method to encrpyt
        public async Task<encrypt> GenerateEncrpyted(ApplicationUser user)
        {
            // Encrypt the JWT using AES-GCM
            var aes = new AES(_config["AesGcm:Key"]);
            var userJson = JObject.FromObject(user).ToString();
            var (cipherText, tag, nonce) = aes.Encrypt(userJson);

            // Convert encrypted parts to Base64 for safe storage/transmission
            string encryptedToken = Convert.ToBase64String(cipherText);
            string tagBase64 = Convert.ToBase64String(tag);
            string nonceBase64 = Convert.ToBase64String(nonce);

            encrypt encrypt = new encrypt()
            {
                EncryptedToken = encryptedToken,
                TagBase64 = tagBase64,
                NonceBase64 = nonceBase64
            };

            return (encrypt);
        }
        
        public async Task<encrypt> GenerateEncrpytedString(String input)
        {
            // Encrypt the JWT using AES-GCM
            var aes = new AES(_config["AesGcm:Key"]);
            var (cipherText, tag, nonce) = aes.Encrypt(input);

            // Convert encrypted parts to Base64 for safe storage/transmission
            string encryptedToken = Convert.ToBase64String(cipherText);
            string tagBase64 = Convert.ToBase64String(tag);
            string nonceBase64 = Convert.ToBase64String(nonce);

            encrypt encrypt = new encrypt()
            {
                EncryptedToken = encryptedToken,
                TagBase64 = tagBase64,
                NonceBase64 = nonceBase64
            };

            return (encrypt);
        }


        // Method to generate JWT token
        public async Task<string> GenerateJwtToken(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user); // Async call to get roles
            

            var claims = new List<Claim>
    {
        new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            // Generate the JWT token
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: creds
            );

            string jwtToken = new JwtSecurityTokenHandler().WriteToken(token);

            // Encrypt the JWT using AES-GCM
            //var aes = new AES(_config["AesGcm:Key"]);
            //var (cipherText, tag, nonce) = aes.Encrypt(jwtToken);

            //// Convert encrypted parts to Base64 for safe storage/transmission
            //string encryptedToken = Convert.ToBase64String(cipherText);
            //string tagBase64 = Convert.ToBase64String(tag);
            //string nonceBase64 = Convert.ToBase64String(nonce);

            //encrypt encrypt = new encrypt() { 
            //    EncryptedToken = encryptedToken,
            //    TagBase64 = tagBase64,
            //    NonceBase64 = nonceBase64
            //};

            return jwtToken;
        }

        //public async Task<string> GenerateJwtToken(ApplicationUser user)
        //{
        //    var roles = await _userManager.GetRolesAsync(user); // This is an async call

        //    var claims = new List<Claim>
        //    {
        //        new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
        //        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        //    };

        //    foreach (var role in roles)
        //    {
        //        claims.Add(new Claim(ClaimTypes.Role, role));
        //    }

        //    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
        //    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        //    var token = new JwtSecurityToken(
        //        issuer: _config["Jwt:Issuer"],
        //        audience: _config["Jwt:Audience"],
        //        claims: claims,
        //        expires: DateTime.Now.AddMinutes(30),
        //        signingCredentials: creds
        //    );

        //    return new JwtSecurityTokenHandler().WriteToken(token);
        //}


    }
}
