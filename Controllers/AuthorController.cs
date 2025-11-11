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
    public class AuthorController : ControllerBase
    {
        private readonly IMongoCollection<Author> _authors;
        private readonly ILogger<AuthorController> _logger;

        public AuthorController(IMongoDatabase database, ILogger<AuthorController> logger)
        {
            _authors = database.GetCollection<Author>("autores");
            _logger = logger;
        }

        // GET: api/author
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AuthorViewDto>>> GetAll()
        {
            try
            {
                var authors = await _authors.Find(_ => true).ToListAsync();
                
                var dto = authors.Select(a => new AuthorViewDto
                {
                    Name = a.Name
                }).ToList();

                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo autores");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // GET: api/author/{name}
        [HttpGet("{name}")]
        public async Task<ActionResult<AuthorViewDto>> GetByName(string name)
        {
            try
            {
                var author = await _authors.Find(a => a.Name == name).FirstOrDefaultAsync();

                if (author == null)
                {
                    return NotFound($"Autor con nombre '{name}' no encontrado");
                }

                var dto = new AuthorViewDto
                {
                    Name = author.Name
                };

                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo autor {AuthorName}", name);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // POST: api/author
        [HttpPost]
        public async Task<ActionResult<AuthorViewDto>> Create([FromBody] AuthorCreateDto dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest("Los datos del autor no pueden ser nulos");

                if (string.IsNullOrWhiteSpace(dto.Name))
                    return BadRequest("El nombre es obligatorio");

                // Verificar si ya existe
                var existing = await _authors.Find(a => a.Name == dto.Name).FirstOrDefaultAsync();
                
                if (existing != null)
                    return Conflict($"Ya existe un autor con el nombre '{dto.Name}'");

                var author = new Author
                {
                    Name = dto.Name
                };

                await _authors.InsertOneAsync(author);

                var responseDto = new AuthorViewDto
                {
                    Name = author.Name
                };

                return CreatedAtAction(nameof(GetByName), new { name = author.Name }, responseDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando autor");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // PUT: api/author/{name}
        [HttpPut("{name}")]
        public async Task<IActionResult> Update(string name, [FromBody] AuthorUpdateDto dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest("Los datos del autor no pueden ser nulos");

                if (string.IsNullOrWhiteSpace(dto.Name))
                    return BadRequest("El nombre es obligatorio");

                // Verificar si el autor existe
                var existingAuthor = await _authors.Find(a => a.Name == name).FirstOrDefaultAsync();
                
                if (existingAuthor == null)
                    return NotFound($"Autor con nombre '{name}' no encontrado");

                // Verificar si el nuevo nombre ya existe (si es diferente)
                if (name != dto.Name)
                {
                    var duplicateAuthor = await _authors.Find(a => a.Name == dto.Name).FirstOrDefaultAsync();
                    if (duplicateAuthor != null)
                        return Conflict($"Ya existe un autor con el nombre '{dto.Name}'");
                }

                var filter = Builders<Author>.Filter.Eq(a => a.Name, name);
                var update = Builders<Author>.Update.Set(a => a.Name, dto.Name);

                var result = await _authors.UpdateOneAsync(filter, update);

                if (result.MatchedCount == 0)
                {
                    return NotFound($"Autor con nombre '{name}' no encontrado");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando autor {AuthorName}", name);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // DELETE: api/author/{name}
        [HttpDelete("{name}")]
        public async Task<IActionResult> Delete(string name)
        {
            try
            {
                var filter = Builders<Author>.Filter.Eq(a => a.Name, name);
                var result = await _authors.DeleteOneAsync(filter);

                if (result.DeletedCount == 0)
                {
                    return NotFound($"Autor con nombre '{name}' no encontrado");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error eliminando autor {AuthorName}", name);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // GET: api/author/search?name={name}
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<AuthorViewDto>>> Search([FromQuery] string name)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                    return BadRequest("El parámetro de búsqueda no puede estar vacío");

                var filter = Builders<Author>.Filter.Regex(a => a.Name, new MongoDB.Bson.BsonRegularExpression(name, "i"));
                var authors = await _authors.Find(filter).ToListAsync();

                var dto = authors.Select(a => new AuthorViewDto
                {
                    Name = a.Name
                }).ToList();

                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error buscando autores con nombre '{Name}'", name);
                return StatusCode(500, "Error interno del servidor");
            }
        }
    }
}
