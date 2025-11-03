using HR.Models;
using HR.Models.Dto;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HR.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthorController : ControllerBase
    {
        private readonly IMongoCollection<Author> _authorsCollection;
        private readonly ILogger<AuthorController> _logger;

        public AuthorController(IMongoDatabase database, ILogger<AuthorController> logger)
        {
            _authorsCollection = database.GetCollection<Author>("autores");
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
                    Id = a.Id,
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

        // GET: api/author/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<AuthorViewDto>> GetById(string id)
        {
            try
            {
                var filter = Builders<Author>.Filter.Eq(a => a.Id, id);
                var author = await _authors.Find(filter).FirstOrDefaultAsync();

                if (author == null)
                {
                    return NotFound($"Autor con ID {id} no encontrado");
                }

                var dto = new AuthorViewDto
                {
                    Id = author.Id,
                    Name = author.Name
                };

                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo autor {AuthorId}", id);
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
                var existingFilter = Builders<Author>.Filter.Eq(a => a.Name, dto.Name);
                var existing = await _authors.Find(existingFilter).FirstOrDefaultAsync();
                
                if (existing != null)
                    return Conflict($"Ya existe un autor con el nombre '{dto.Name}'");

                var author = new Author
                {
                    Name = dto.Name
                };

                await _authors.InsertOneAsync(author);

                var responseDto = new AuthorViewDto
                {
                    Id = author.Id,
                    Name = author.Name
                };

                return CreatedAtAction(nameof(GetById), new { id = author.Id }, responseDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando autor");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // PUT: api/author/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] AuthorUpdateDto dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest("Los datos del autor no pueden ser nulos");

                if (string.IsNullOrWhiteSpace(dto.Name))
                    return BadRequest("El nombre es obligatorio");

                var filter = Builders<Author>.Filter.Eq(a => a.Id, id);
                var update = Builders<Author>.Update.Set(a => a.Name, dto.Name);

                var result = await _authors.UpdateOneAsync(filter, update);

                if (result.MatchedCount == 0)
                {
                    return NotFound($"Autor con ID {id} no encontrado");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando autor {AuthorId}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // DELETE: api/author/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var filter = Builders<Author>.Filter.Eq(a => a.Id, id);
                var result = await _authors.DeleteOneAsync(filter);

                if (result.DeletedCount == 0)
                {
                    return NotFound($"Autor con ID {id} no encontrado");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error eliminando autor {AuthorId}", id);
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
                    Id = a.Id,
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
