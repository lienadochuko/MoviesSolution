using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MoviesApi.Domain.Repositories;
using MoviesApi.Helpers;
using MoviesApi.Models;
using Newtonsoft.Json.Linq;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using System.Data;
using System.Data.SqlClient;

namespace MoviesApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize(Roles = "User, Admin, Developer")]
    public class MoviesController(IDataRepository dataRepository, ILogger<MoviesController> logger) : ControllerBase
    {
        [Route("[action]")]
        //[Route("/")]
        [HttpGet]
        public async Task<IActionResult> GetMovies()
        {
            var movies = new[]
            {
                new { Title = "The Shawshank Redemption", Director = "Frank Darabont" },
                new { Title = "The Godfather", Director = "Francis Ford Coppola" },
                new { Title = "The Dark Knight", Director = "Christopher Nolan" }
            };

            return Ok(movies);
        }

        [Route("[action]")]
        [HttpGet]
        public async Task<IActionResult> GetCertificateID(IConfiguration configuration, CancellationToken cancellationToken)
        {
            var certificates = dataRepository.DynamicRetrival("dbo.GETCERTIFICATES", CustomHelpers.GetConnectionString(configuration, "Default"), null, reader =>
            {
                return new CertificateDTO
                {
                    CertificateID = CustomHelpers.GetSafeInt32(reader, 0),
                    Certificate = CustomHelpers.GetSafeString(reader, 1),
                };
            }, cancellationToken);

            IEnumerable<CertificateDTO> certificate = await certificates;

            return Ok(certificate);
        }

        [Route("[action]")]
        [HttpGet]
        public async Task<IActionResult> GetCoutriesID(IConfiguration configuration, CancellationToken cancellationToken)
        {
            var countries = dataRepository.DynamicRetrival("dbo.GETCOUNTRIES", CustomHelpers.GetConnectionString(configuration, "Default"), null, reader =>
            {
                return new CountryDTO
                {
                    CountryID = CustomHelpers.GetSafeInt32(reader, 0),
                    Country = CustomHelpers.GetSafeString(reader, 1),
                };
            }, cancellationToken);

            IEnumerable<CountryDTO> country = await countries;

            return Ok(country);
        }

        [Route("[action]")]
        [HttpGet]
        public async Task<IActionResult> GetDirectorsID(IConfiguration configuration, CancellationToken cancellationToken)
        {
            var directors = dataRepository.DynamicRetrival("dbo.GETDIRECTORS", CustomHelpers.GetConnectionString(configuration, "Default"), null, reader =>
            {
                return new DirectorDTO
                {
                    DirectorID = CustomHelpers.GetSafeInt32(reader, 0),
                    FullName = CustomHelpers.GetSafeString(reader, 1),
                };
            }, cancellationToken);

            return Ok(directors);
        }

        [Route("[action]")]
        [HttpGet]
        public async Task<IActionResult> GetGenresID(IConfiguration configuration, CancellationToken cancellationToken)
        {
            var Genres = dataRepository.DynamicRetrival("dbo.GETGENRES", CustomHelpers.GetConnectionString(configuration, "Default"), null, reader =>
            {
                return new GenreDTO
                {
                    GenreID = CustomHelpers.GetSafeInt32(reader, 0),
                    Genre = CustomHelpers.GetSafeString(reader, 1),
                };
            }, cancellationToken);

            IEnumerable<GenreDTO> Genre = await Genres;

            return Ok(Genre);
        }

        [Route("[action]")]
        [HttpGet]
        public async Task<IActionResult> GetLanguageID(IConfiguration configuration, CancellationToken cancellationToken)
        {
            var Languages = dataRepository.DynamicRetrival("dbo.GETSTUDIOS", CustomHelpers.GetConnectionString(configuration, "Default"), null, reader =>
            {
                return new LanguageDTO
                {
                    LanguageID = CustomHelpers.GetSafeInt32(reader, 0),
                    Language = CustomHelpers.GetSafeString(reader, 1),
                };
            }, cancellationToken);

            return Ok(Languages);
        }

        [Route("[action]")]
        [HttpGet]
        public async Task<IActionResult> GetStudiosID(IConfiguration configuration, CancellationToken cancellationToken)
        {
            var Studios = dataRepository.DynamicRetrival("dbo.GETSTUDIOS", CustomHelpers.GetConnectionString(configuration, "Default"), null, reader =>
            {
                return new StudioDTO
                {
                    StudioID = CustomHelpers.GetSafeInt32(reader, 0),
                    Studio = CustomHelpers.GetSafeString(reader, 1),
                };
            }, cancellationToken);

            return Ok(Studios);
        }

		[Route("[action]")]
		[HttpGet]
		public async Task<IActionResult> GETFILMS(IConfiguration configuration, CancellationToken cancellationToken, int pageNumber, int pageSize)
		{
			var films = new List<Film2>();
            int totalRecords = 0;

            await using (SqlConnection connection = new SqlConnection(CustomHelpers.GetConnectionString(configuration, "Default")))
			{
				await using (SqlCommand command = new SqlCommand("dbo.GETFILMS2", connection))
				{
					command.CommandType = CommandType.StoredProcedure;

					// Adding the parameters to the command
					command.Parameters.Add(new SqlParameter("@PageNumber", SqlDbType.Int) { Value = pageNumber });
					command.Parameters.Add(new SqlParameter("@PageSize", SqlDbType.Int) { Value = pageSize });
                    
                    // Add output parameter for total records
                    SqlParameter totalRecordsParam = new SqlParameter("@TotalRecords", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };
                    command.Parameters.Add(totalRecordsParam);

                    await connection.OpenAsync();

					// Executing the command and reading the result
					await using (SqlDataReader reader = await command.ExecuteReaderAsync())
					{
						while (await reader.ReadAsync())
						{
							var film = new Film2
							{
								FilmID = CustomHelpers.GetSafeInt32(reader, 0),
								Title = CustomHelpers.GetSafeString(reader, 1),
								ReleaseDate = CustomHelpers.GetDateTime(reader, 2),
								DirectorID = CustomHelpers.GetSafeInt32(reader, 3),
								Director = CustomHelpers.GetSafeString(reader, 4),
								StudioID = CustomHelpers.GetSafeInt32(reader, 5),
								Studio = CustomHelpers.GetSafeString(reader, 6),
								Review = CustomHelpers.GetSafeString(reader, 7),
								CountryID = CustomHelpers.GetSafeInt32(reader, 8),
								Country = CustomHelpers.GetSafeString(reader, 9),
								LanguageID = CustomHelpers.GetSafeInt32(reader, 10),
								Language = CustomHelpers.GetSafeString(reader, 11),
								GenreID = CustomHelpers.GetSafeInt32(reader, 12),
								Genre = CustomHelpers.GetSafeString(reader, 13),
								RunTimeMinutes = CustomHelpers.GetSafeInt16(reader, 14),
								CertificateID = CustomHelpers.GetSafeInt32(reader, 15),
								Certificate = CustomHelpers.GetSafeString(reader, 16),
								BudgetDollars = CustomHelpers.GetSafeInt64(reader, 17),
								BoxOfficeDollars = CustomHelpers.GetSafeInt64(reader, 18),
								OscarNominations = CustomHelpers.GetSafeByte(reader, 19),
								OscarWins = CustomHelpers.GetSafeByte(reader, 20),
								Poster = CustomHelpers.GetSafeString(reader, 21),
							};

							// Add each film to the list
							films.Add(film);
                        }
                    }
                        // Retrieve the total records from the output parameter
                        totalRecords = (int)command.Parameters["@TotalRecords"].Value;

                    // Calculate total pages
                    int totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

                    // Prepare ViewModel with pagination data
                    var model = new PaginatedFilmViewModel
                    {
                        Films = films,
                        CurrentPage = pageNumber,
                        PageSize = pageSize,
                        TotalItems = totalRecords,
                        TotalPages = totalPages
                    };

                    return Ok(model);
                }
			}
		}

	}
}
