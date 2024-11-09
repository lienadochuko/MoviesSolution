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
using System.Globalization;
using System.Drawing.Imaging;
using System.Drawing;
using System.Configuration;

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
								Poster = CustomHelpers.GetSafeString(reader, 2),
                                Rating = CustomHelpers.GetSafeString(reader, 3),
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
                    await connection.CloseAsync();
                    return Ok(model);
                }
			}
		}

		[Route("[action]")]
		[HttpGet]
		public async Task<IActionResult> GETFILMBYTITLE(IConfiguration configuration, CancellationToken cancellationToken, string title, int pageNumber, int pageSize)
		{
			var films = new List<Film2>();
			int totalRecords = 0;

			await using (SqlConnection connection = new SqlConnection(CustomHelpers.GetConnectionString(configuration, "Default")))
			{
				await using (SqlCommand command = new SqlCommand("dbo.GETFILMBYTITLE", connection))
				{
					command.CommandType = CommandType.StoredProcedure;

					// Adding the parameters to the command
					command.Parameters.Add(new SqlParameter("@FilmTITLE", SqlDbType.VarChar) { Value = title });
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
						if (!reader.HasRows)
						{
							// If no films found, return 404 Not Found
							return NotFound(new { message = "No films found." });
						}

						while (await reader.ReadAsync())
						{
							var film = new Film2
							{
								FilmID = CustomHelpers.GetSafeInt32(reader, 0),
								Title = CustomHelpers.GetSafeString(reader, 1),
								Poster = CustomHelpers.GetSafeString(reader, 2),
								Rating = CustomHelpers.GetSafeString(reader, 3),
							};

							// Add each film to the list
							films.Add(film);
						}
					}

					// Retrieve the total records from the output parameter
					totalRecords = (int)command.Parameters["@TotalRecords"].Value;

					// Check for specific conditions to return 401 or 403
					if (totalRecords < 0) // Example condition for 401 Unauthorized
					{
						return Unauthorized(new { message = "Unauthorized access." });
					}

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
                    await connection.CloseAsync();
					return Ok(model);
				}
			}
		}


		[Route("[action]")]
		[HttpGet]
        public async Task<IActionResult> GETFILMBYID(IConfiguration configuration, CancellationToken cancellationToken, int Id)
		{
			Film films = new();

            await using (SqlConnection connection = new SqlConnection(CustomHelpers.GetConnectionString(configuration, "Default")))
			{
				await using (SqlCommand command = new SqlCommand("dbo.GETFILMBYID", connection))
				{
					command.CommandType = CommandType.StoredProcedure;

					// Adding the parameters to the command
					command.Parameters.Add(new SqlParameter("@FilmID", SqlDbType.Int) { Value = Id });

                    await connection.OpenAsync();

					// Executing the command and reading the result
					await using (SqlDataReader reader = await command.ExecuteReaderAsync())
					{
                        if (!reader.HasRows) { return NotFound(new { message = "No films found." }); }

						while (await reader.ReadAsync())
						{
                            films = new Film
                            {
                                FilmID = CustomHelpers.GetSafeInt32(reader, 0),
                                Title = CustomHelpers.GetSafeString(reader, 1),
                                ReleaseDate = CustomHelpers.GetDateTimeFormatted(reader, 2),
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
                                Rating = CustomHelpers.GetSafeString(reader, 22),
                            };
                        }
                    }
                    await connection.CloseAsync();
                    return Ok(films);
                }
			}
		}

		[HttpPut("UpdateFilmPosters")]
		public async Task<IActionResult> UpdateFilmPosters(IConfiguration configuration, CancellationToken cancellationToken)
		{
			await using (SqlConnection connection = new SqlConnection(CustomHelpers.GetConnectionString(configuration, "Default")))
			{
				await connection.OpenAsync(cancellationToken);

				// Fetch all films
				string selectQuery = "SELECT FilmID FROM Film";
				SqlCommand selectCommand = new SqlCommand(selectQuery, connection);
				SqlDataReader reader = await selectCommand.ExecuteReaderAsync(cancellationToken);

				var filmIds = new List<int>();
				while (await reader.ReadAsync(cancellationToken))
				{
					filmIds.Add(reader.GetInt32(0));
				}
				reader.Close();
                string base64Image;
                // Update each film with a generated Base64 poster
                foreach (int filmId in filmIds)
                {

                    //if (filmId < 10)
                    //{
                    //    base64Image = await FetchRandomCartoonImageBase64Async(); // Cartoon images for filmId less than 10
                    //}
                    //else if (filmId >= 10 && filmId <= 1670)
                    //{
                        base64Image = await GenerateRandomFourColorBase64Image(); // Random color images for filmId between 10 and 1670
                    //}
                    //else
                    //{
                    //    base64Image = await FetchRandomCartoonImageBase64Async(); // Cartoon images for filmId greater than 1670
                    //}

                    string updateQuery = "UPDATE Film SET Poster = @Poster WHERE FilmID = @FilmID";
                    using (SqlCommand updateCommand = new SqlCommand(updateQuery, connection))
                    {
                        updateCommand.Parameters.AddWithValue("@Poster", base64Image);
                        updateCommand.Parameters.AddWithValue("@FilmID", filmId);
                        await updateCommand.ExecuteNonQueryAsync(cancellationToken);
                    }
                }

                return Ok("Film posters updated successfully.");
			}
		}


		public async Task<string> FormattedDate(DateTime dateTime)
        {
            string dateString = "26/Apr/2007 12:00:00 AM";

            // Parse the date string to DateTime
            DateTime date =DateTime.ParseExact(dateString, "dd/MMM/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture);

            // Format the DateTime to "April 26, 2007"
            string formattedDate = date.ToString("MMMM dd, yyyy", CultureInfo.InvariantCulture);

            //Console.WriteLine(formattedDate); // Output: "April 26, 2007

            return formattedDate; // Output: "April 26, 2007"
        }

        //private async Task<string> GenerateRandomBase64Image()
        //{
        //	int width = 100;  // Width of the image
        //	int height = 100; // Height of the image

        //	using (Bitmap bitmap = new Bitmap(width, height))
        //	{
        //		Random random = new Random();
        //		Color randomColor = Color.FromArgb(random.Next(256), random.Next(256), random.Next(256));

        //		using (Graphics graphics = Graphics.FromImage(bitmap))
        //		{
        //			graphics.Clear(randomColor);
        //		}

        //		using (MemoryStream memoryStream = new MemoryStream())
        //		{
        //			bitmap.Save(memoryStream, ImageFormat.Png);
        //			byte[] imageBytes = memoryStream.ToArray();
        //			return Convert.ToBase64String(imageBytes);
        //		}
        //	}
        //}

        private async Task<string> GenerateRandomFourColorBase64Image()
        {
            int width = 100;  // Width of the image
            int height = 100; // Height of the image

            using (Bitmap bitmap = new Bitmap(width, height))
            {
                Random random = new Random();

                // Generate four random colors
                Color color1 = Color.FromArgb(random.Next(256), random.Next(256), random.Next(256));
                Color color2 = Color.FromArgb(random.Next(256), random.Next(256), random.Next(256));
                Color color3 = Color.FromArgb(random.Next(256), random.Next(256), random.Next(256));
                Color color4 = Color.FromArgb(random.Next(256), random.Next(256), random.Next(256));

                using (Graphics graphics = Graphics.FromImage(bitmap))
                {
                    // Fill each quadrant with a different color
                    graphics.FillRectangle(new SolidBrush(color1), 0, 0, width / 2, height / 2);         // Top-left
                    graphics.FillRectangle(new SolidBrush(color2), width / 2, 0, width / 2, height / 2); // Top-right
                    graphics.FillRectangle(new SolidBrush(color3), 0, height / 2, width / 2, height / 2); // Bottom-left
                    graphics.FillRectangle(new SolidBrush(color4), width / 2, height / 2, width / 2, height / 2); // Bottom-right
                }

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    bitmap.Save(memoryStream, ImageFormat.Png);
                    byte[] imageBytes = memoryStream.ToArray();
                    return Convert.ToBase64String(imageBytes);
                }
            }
        }


        private string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            Random random = new Random();
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private async Task<string> FetchRandomCartoonImageBase64Async()
        {
            string uniqueString = GenerateRandomString(8); // Generates an 8-character random string
            string apiUrl = $"https://robohash.org/{uniqueString}.png?set=set4";

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(apiUrl);

                    // Check if the response was successful
                    if (response.IsSuccessStatusCode)
                    {
                        byte[] imageBytes = await response.Content.ReadAsByteArrayAsync();
                        return Convert.ToBase64String(imageBytes);
                    }
                    else
                    {
                        // Log the unsuccessful status code
                        Console.WriteLine($"API call unsuccessful: {response.StatusCode}. Using fallback image.");
                    }
                }
                catch (Exception ex)
                {
                    // Log the exception details
                    Console.WriteLine($"Error fetching image from API: {ex.Message}. Using fallback image.");
                }

                // If API call fails or any exception occurs, return a generated Base64 image
                //return await GenerateRandomBase64Image();
                return null;
            }
        }
    }
}
