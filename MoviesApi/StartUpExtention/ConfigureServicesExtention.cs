using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MoviesApi.Domain.DatabaseContext;
using MoviesApi.Domain.IdentityEntities;
using MoviesApi.Domain.Repositories;
using MoviesApi.Services;
using System;
using System.Configuration;
using System.Text;

namespace MoviesApi.StartUpExtention
{
    public static class ConfigureServicesExtention
    {
        public static IServiceCollection ConfigureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Add Controllers
            services.AddControllers();

            // Configure Identity
            services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireDigit = true;
                options.Password.RequiredUniqueChars = 3;
                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
                options.Lockout.MaxFailedAccessAttempts = 10;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            // JWT authentication configuration
            // CORS
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(policyBuilder =>
                {
                    policyBuilder
                        .AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });

            // JWT Authentication
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(options =>
                {
                     options.TokenValidationParameters = new TokenValidationParameters
                     {
                         ValidateIssuer = true,
                         ValidateAudience = true,
                         ValidateLifetime = true,
                         ValidateIssuerSigningKey = true,
                         ValidIssuer = configuration["Jwt:Issuer"],
                         ValidAudience = configuration["Jwt:Audience"],
                         IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]))
                     };

                     options.Events = new JwtBearerEvents
                     {
                         OnMessageReceived = context =>
                         {
                             var aes = context.HttpContext.RequestServices.GetRequiredService<AES>();

                             if (context.Request.Headers.TryGetValue("Authorization", out var encryptedTokenBase64) &&
                                 context.Request.Headers.TryGetValue("Tag", out var tagBase64) &&
                                 context.Request.Headers.TryGetValue("Nonce", out var nonceBase64))
                             {
                                 try
                                 {
                                     // Remove "Bearer " prefix from token
                                     string encryptedToken = encryptedTokenBase64.ToString().Substring(7);

                                     byte[] cipherText = Convert.FromBase64String(encryptedToken);
                                     byte[] tag = Convert.FromBase64String(tagBase64);
                                     byte[] nonce = Convert.FromBase64String(nonceBase64);

                                     // Decrypt the token
                                     string decryptedToken = aes.Decrypt(cipherText, tag, nonce);
                                     context.Token = decryptedToken;
                                 }
                                 catch (Exception)
                                 {
                                     context.Fail("Invalid encrypted token");
                                 }
                             }

                             return Task.CompletedTask;
                         }
                     };
                 });


            services.AddAuthorization(options =>
            {
                options.AddPolicy("RequireAuthenticatedUser", policy =>
                {
                    policy.RequireAuthenticatedUser();
                });
            });

            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Auth/Login";  // Change to the path you want
            });

            // Other services like database context
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("Default")));

            // Register AuthService - Dependency Injection
            services.AddScoped<IDataRepository, DataRepository>();
            services.AddScoped<AuthService>();
            services.AddSingleton(new AES(configuration["AesGcm:Key"]));

            // HTTP Logging
            services.AddHttpLogging(options =>
            {
                options.LoggingFields = Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.RequestProperties |
                                        Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.RequestPropertiesAndHeaders;
            });

            return services;
        }
    }
}
