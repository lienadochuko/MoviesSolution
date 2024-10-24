using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MoviesApi.Domain.DatabaseContext;
using MoviesApi.Domain.IdentityEntities;
using MoviesApi.Domain.Repositories;
using MoviesApi.Services;
using MoviesApi.StartUpExtention;
using Serilog;
using System.Data.SqlClient;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();
// Serilog
builder.Host.UseSerilog((HostBuilderContext context,
    IServiceProvider services, LoggerConfiguration loggerConfiguration) =>
{
    loggerConfiguration
    .ReadFrom.Configuration(context.Configuration) // Read configuration settings from built-in IConfiguration
    .ReadFrom.Services(services); // Read out current app's services and make them available to Serilog
});

builder.Services.ConfigureServices(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseHsts();
app.UseHttpLogging(); 

app.UseRouting();
app.UseCors();

//// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//app.UseSwagger();
//  app.UseSwaggerUI();
//}

app.UseHttpsRedirection();
// Ensure authentication is called before authorization
app.UseAuthentication(); 
app.UseAuthorization();

app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action}/{id?}");

app.Run();
