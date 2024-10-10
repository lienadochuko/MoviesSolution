using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MoviesApi.DatabaseContext;
using System.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddDbContext<ApplicationDbContext>
(
    options =>
    {
        options.UseSqlServer(new SqlConnection(builder.Configuration.GetConnectionString("Default")));
    }
);

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseHsts();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
  