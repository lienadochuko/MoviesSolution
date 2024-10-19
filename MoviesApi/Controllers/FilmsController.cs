using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoviesApi.Domain.DatabaseContext;
using MoviesApi.Helpers;
using MoviesApi.Models;

namespace MoviesApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "User, Admin, Developer")]
    public class FilmsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public FilmsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Films
        [Route("[action]")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Film>>> GetFilms()
        {
            try
            {
                var films = await _context.Film.OrderBy(temp => temp.Title).ToListAsync();
                if (films == null || !films.Any())
                {
                    return NotFound("No films found in the database.");
                }

                return films;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
            return BadRequest();
        }

        // GET: api/Films/5
        [Route("[action]")]
        [HttpGet("[action]/{id}")]
        public async Task<ActionResult<Film>> GetAFilm(int id)
        {
            if(id <= 0)
            {
                return BadRequest();
            }
            var film = await _context.Film.FindAsync(id);

            if (film == null)
            {
                return NotFound();
            }

            return film;
        }

        // PUT: api/Films/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Route("[action]")]
        [HttpPut("[action]/{FilmID}")]
        public async Task<IActionResult> PutFilm(int FilmID, Film film)
        {
            if (FilmID != film.FilmID)
            {
                return BadRequest();
            }
            ValidationHelper.ModelValidation(film);

            _context.Entry(film).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FilmExists(FilmID))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Films
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Route("[action]")]
        [HttpPost]
        public async Task<ActionResult<Film>> PostFilm(Film film)
        {
            _context.Film.Add(film);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetAFilm", new { id = film.FilmID }, film);
        }

        // DELETE: api/Films/5
        [Route("[action]")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFilm(int id)
        {
            var film = await _context.Film.FindAsync(id);
            if (film == null)
            {
                return NotFound();
            }

            _context.Film.Remove(film);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool FilmExists(int id)
        {
            return _context.Film.Any(e => e.FilmID == id);
        }
    }
}
