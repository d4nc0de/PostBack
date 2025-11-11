using HR.Models;
using HR.Models.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HR.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookController : ControllerBase
    {
        private readonly IMongoCollection<Book> _books;
        private readonly ILogger<BookController> _logger;

        public BookController(IMongoDatabase database, ILogger<BookController> logger)
        {
            _books = database.GetCollection<Book>("libros");
            _logger = logger;
        }

        // GET: api/book
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BookViewDto>>> GetAll()
        {
            try
            {
                var books = await _books.Find(_ => true).ToListAsync();

                var dto = books.Select(b => new BookViewDto
                {
                    Title = b.Title
                }).ToList();

                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo libros");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // GET: api/book/{title}
        [HttpGet("{title}")]
        public async Task<ActionResult<BookViewDto>> GetByTitle(string title)
        {
            try
            {
                var book = await _books.Find(b => b.Title == title).FirstOrDefaultAsync();

                if (book == null)
                    return NotFound($"Libro con título '{title}' no encontrado");

                var dto = new BookViewDto
                {
                    Title = book.Title
                };

                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo libro {BookTitle}", title);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // POST: api/book
        [HttpPost]
        public async Task<ActionResult<BookViewDto>> Create([FromBody] BookCreateDto dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest("Los datos del libro no pueden ser nulos");

                if (string.IsNullOrWhiteSpace(dto.Title))
                    return BadRequest("El título es obligatorio");

                // Verificar si ya existe un libro con ese título
                var existingBook = await _books
                    .Find(b => b.Title == dto.Title)
                    .FirstOrDefaultAsync();

                if (existingBook != null)
                    return Conflict($"Ya existe un libro con el título '{dto.Title}'");

                var book = new Book
                {
                    Title = dto.Title
                };

                await _books.InsertOneAsync(book);

                var responseDto = new BookViewDto
                {
                    Title = book.Title
                };

                return CreatedAtAction(nameof(GetByTitle), new { title = book.Title }, responseDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando libro");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // PUT: api/book/{title}
        [HttpPut("{title}")]
        public async Task<IActionResult> Update(string title, [FromBody] BookUpdateDto dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest("Los datos del libro no pueden ser nulos");

                if (string.IsNullOrWhiteSpace(dto.Title))
                    return BadRequest("El título es obligatorio");

                // Verificar si el libro existe
                var existingBook = await _books.Find(b => b.Title == title).FirstOrDefaultAsync();
                
                if (existingBook == null)
                    return NotFound($"Libro con título '{title}' no encontrado");

                // Verificar si el nuevo título ya existe (si es diferente)
                if (title != dto.Title)
                {
                    var duplicateBook = await _books.Find(b => b.Title == dto.Title).FirstOrDefaultAsync();
                    if (duplicateBook != null)
                        return Conflict($"Ya existe un libro con el título '{dto.Title}'");
                }

                var filter = Builders<Book>.Filter.Eq(b => b.Title, title);
                var update = Builders<Book>.Update.Set(b => b.Title, dto.Title);

                var result = await _books.UpdateOneAsync(filter, update);

                if (result.MatchedCount == 0)
                    return NotFound($"Libro con título '{title}' no encontrado");

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando libro {BookTitle}", title);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // DELETE: api/book/{title}
        [HttpDelete("{title}")]
        public async Task<IActionResult> Delete(string title)
        {
            try
            {
                var filter = Builders<Book>.Filter.Eq(b => b.Title, title);
                var result = await _books.DeleteOneAsync(filter);

                if (result.DeletedCount == 0)
                    return NotFound($"Libro con título '{title}' no encontrado");

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error eliminando libro {BookTitle}", title);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // GET: api/book/search?title={title}
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<BookViewDto>>> Search([FromQuery] string title)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(title))
                    return BadRequest("El parámetro de búsqueda no puede estar vacío");

                var filter = Builders<Book>.Filter.Regex(b => b.Title, new MongoDB.Bson.BsonRegularExpression(title, "i"));
                var books = await _books.Find(filter).ToListAsync();

                var dto = books.Select(b => new BookViewDto
                {
                    Title = b.Title
                }).ToList();

                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error buscando libros con título '{Title}'", title);
                return StatusCode(500, "Error interno del servidor");
            }
        }
    }
}
