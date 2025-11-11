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
    public class EditionController : ControllerBase
    {
        private readonly IMongoCollection<Edition> _editions;
        private readonly IMongoCollection<Copy> _copies;
        private readonly ILogger<EditionController> _logger;

        public EditionController(IMongoDatabase database, ILogger<EditionController> logger)
        {
            _editions = database.GetCollection<Edition>("ediciones");
            _copies = database.GetCollection<Copy>("copias");
            _logger = logger;
        }

        // GET: api/edition
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EditionViewDto>>> GetAll()
        {
            try
            {
                var editions = await _editions.Find(_ => true).ToListAsync();

                var dto = editions.Select(e => new EditionViewDto
                {
                    ISBN = e.ISBN,
                    PublicationDate = e.PublicationDate,
                    Language = e.Language,
                    BookTitle = e.BookTitle
                }).ToList();

                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo ediciones");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // GET: api/edition/{isbn}
        [HttpGet("{isbn}")]
        public async Task<ActionResult<EditionViewDto>> GetByISBN(string isbn)
        {
            try
            {
                var edition = await _editions
                    .Find(e => e.ISBN == isbn)
                    .FirstOrDefaultAsync();

                if (edition == null)
                    return NotFound($"Edición con ISBN {isbn} no encontrada");

                var dto = new EditionViewDto
                {
                    ISBN = edition.ISBN,
                    PublicationDate = edition.PublicationDate,
                    Language = edition.Language,
                    BookTitle = edition.BookTitle
                };

                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo edición {ISBN}", isbn);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // GET: api/edition/{isbn}/detail (con copias)
        [HttpGet("{isbn}/detail")]
        public async Task<ActionResult<EditionDetailDto>> GetDetailByISBN(string isbn)
        {
            try
            {
                var edition = await _editions
                    .Find(e => e.ISBN == isbn)
                    .FirstOrDefaultAsync();

                if (edition == null)
                    return NotFound($"Edición con ISBN {isbn} no encontrada");

                // Obtener copias de esta edición
                var copies = await _copies
                    .Find(c => c.ISBN == isbn)
                    .ToListAsync();

                var dto = new EditionDetailDto
                {
                    ISBN = edition.ISBN,
                    PublicationDate = edition.PublicationDate,
                    Language = edition.Language,
                    BookTitle = edition.BookTitle,
                    TotalCopies = copies.Count,
                    Copies = copies.Select(c => new CopyViewDto
                    {
                        CopyNumber = c.CopyNumber,
                        ISBN = c.ISBN
                    }).ToList()
                };

                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo detalle de edición {ISBN}", isbn);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // GET: api/edition/book/{bookTitle}
        [HttpGet("book/{bookTitle}")]
        public async Task<ActionResult<IEnumerable<EditionViewDto>>> GetByBookTitle(string bookTitle)
        {
            try
            {
                var editions = await _editions
                    .Find(e => e.BookTitle == bookTitle)
                    .ToListAsync();

                var dto = editions.Select(e => new EditionViewDto
                {
                    ISBN = e.ISBN,
                    PublicationDate = e.PublicationDate,
                    Language = e.Language,
                    BookTitle = e.BookTitle
                }).ToList();

                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo ediciones del libro {BookTitle}", bookTitle);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // POST: api/edition
        [HttpPost]
        public async Task<ActionResult<EditionViewDto>> Create([FromBody] EditionCreateDto dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest("Los datos de la edición no pueden ser nulos");

                if (string.IsNullOrWhiteSpace(dto.ISBN))
                    return BadRequest("El ISBN es obligatorio");

                if (string.IsNullOrWhiteSpace(dto.Language))
                    return BadRequest("El idioma es obligatorio");

                if (string.IsNullOrWhiteSpace(dto.BookTitle))
                    return BadRequest("El título del libro es obligatorio");

                if (dto.PublicationDate == default(DateTime))
                    return BadRequest("La fecha de publicación es obligatoria");

                // Verificar si ya existe una edición con ese ISBN
                var existingEdition = await _editions
                    .Find(e => e.ISBN == dto.ISBN)
                    .FirstOrDefaultAsync();

                if (existingEdition != null)
                    return Conflict($"Ya existe una edición con ISBN {dto.ISBN}");

                var edition = new Edition
                {
                    ISBN = dto.ISBN,
                    PublicationDate = dto.PublicationDate,
                    Language = dto.Language,
                    BookTitle = dto.BookTitle
                };

                await _editions.InsertOneAsync(edition);

                var responseDto = new EditionViewDto
                {
                    ISBN = edition.ISBN,
                    PublicationDate = edition.PublicationDate,
                    Language = edition.Language,
                    BookTitle = edition.BookTitle
                };

                return CreatedAtAction(nameof(GetByISBN), new { isbn = edition.ISBN }, responseDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando edición");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // PUT: api/edition/{isbn}
        [HttpPut("{isbn}")]
        public async Task<IActionResult> Update(string isbn, [FromBody] EditionUpdateDto dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest("Los datos de la edición no pueden ser nulos");

                if (string.IsNullOrWhiteSpace(dto.Language))
                    return BadRequest("El idioma es obligatorio");

                if (string.IsNullOrWhiteSpace(dto.BookTitle))
                    return BadRequest("El título del libro es obligatorio");

                if (dto.PublicationDate == default(DateTime))
                    return BadRequest("La fecha de publicación es obligatoria");

                var filter = Builders<Edition>.Filter.Eq(e => e.ISBN, isbn);
                var update = Builders<Edition>.Update
                    .Set(e => e.PublicationDate, dto.PublicationDate)
                    .Set(e => e.Language, dto.Language)
                    .Set(e => e.BookTitle, dto.BookTitle);

                var result = await _editions.UpdateOneAsync(filter, update);

                if (result.MatchedCount == 0)
                    return NotFound($"Edición con ISBN {isbn} no encontrada");

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando edición {ISBN}", isbn);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // DELETE: api/edition/{isbn}
        [HttpDelete("{isbn}")]
        public async Task<IActionResult> Delete(string isbn)
        {
            try
            {
                // Verificar si tiene copias asociadas
                var copiesCount = await _copies.CountDocumentsAsync(c => c.ISBN == isbn);

                if (copiesCount > 0)
                    return BadRequest("No se puede eliminar una edición con copias asociadas");

                var filter = Builders<Edition>.Filter.Eq(e => e.ISBN, isbn);
                var result = await _editions.DeleteOneAsync(filter);

                if (result.DeletedCount == 0)
                    return NotFound($"Edición con ISBN {isbn} no encontrada");

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error eliminando edición {ISBN}", isbn);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // GET: api/edition/search?language={language}&bookTitle={bookTitle}
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<EditionViewDto>>> Search(
            [FromQuery] string language, 
            [FromQuery] string bookTitle,
            [FromQuery] int? year)
        {
            try
            {
                var filterBuilder = Builders<Edition>.Filter;
                var filter = filterBuilder.Empty;

                if (!string.IsNullOrWhiteSpace(language))
                {
                    filter &= filterBuilder.Regex(e => e.Language, new MongoDB.Bson.BsonRegularExpression(language, "i"));
                }

                if (!string.IsNullOrWhiteSpace(bookTitle))
                {
                    filter &= filterBuilder.Regex(e => e.BookTitle, new MongoDB.Bson.BsonRegularExpression(bookTitle, "i"));
                }

                if (year.HasValue)
                {
                    var startDate = new DateTime(year.Value, 1, 1);
                    var endDate = new DateTime(year.Value, 12, 31, 23, 59, 59);
                    filter &= filterBuilder.Gte(e => e.PublicationDate, startDate) 
                            & filterBuilder.Lte(e => e.PublicationDate, endDate);
                }

                if (filter == filterBuilder.Empty)
                    return BadRequest("Debe proporcionar al menos un parámetro de búsqueda");

                var editions = await _editions.Find(filter).ToListAsync();

                var dto = editions.Select(e => new EditionViewDto
                {
                    ISBN = e.ISBN,
                    PublicationDate = e.PublicationDate,
                    Language = e.Language,
                    BookTitle = e.BookTitle
                }).ToList();

                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error buscando ediciones");
                return StatusCode(500, "Error interno del servidor");
            }
        }
    }
}
