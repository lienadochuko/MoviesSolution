 using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MoviesApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MoviesController : ControllerBase
    {
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
    }
}
