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
    public class CopyController : ControllerBase
    {
        private readonly IMongoCollection<Copy> _copies;
        private readonly IMongoCollection<Loan> _loans;
        private readonly ILogger<CopyController> _logger;

        public CopyController(IMongoDatabase database, ILogger<CopyController> logger)
        {
            _copies = database.GetCollection<Copy>("copias");
            _loans = database.GetCollection<Loan>("prestamos");
            _logger = logger;
        }

        // GET: api/copy
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CopyViewDto>>> GetAll()
        {
            try
            {
                var copies = await _copies.Find(_ => true).ToListAsync();

                var dto = copies.Select(c => new CopyViewDto
                {
                    CopyNumber = c.CopyNumber,
                    ISBN = c.ISBN
                }).ToList();

                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo copias");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // GET: api/copy/{copyNumber}/{isbn}
        [HttpGet("{copyNumber}/{isbn}")]
        public async Task<ActionResult<CopyViewDto>> GetByCopyNumberAndISBN(int copyNumber, string isbn)
        {
            try
            {
                var copy = await _copies
                    .Find(c => c.CopyNumber == copyNumber && c.ISBN == isbn)
                    .FirstOrDefaultAsync();

                if (copy == null)
                    return NotFound($"Copia número {copyNumber} con ISBN {isbn} no encontrada");

                var dto = new CopyViewDto
                {
                    CopyNumber = copy.CopyNumber,
                    ISBN = copy.ISBN
                };

                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo copia {CopyNumber}/{ISBN}", copyNumber, isbn);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // GET: api/copy/isbn/{isbn}
        [HttpGet("isbn/{isbn}")]
        public async Task<ActionResult<IEnumerable<CopyViewDto>>> GetByISBN(string isbn)
        {
            try
            {
                var copies = await _copies
                    .Find(c => c.ISBN == isbn)
                    .ToListAsync();

                var dto = copies.Select(c => new CopyViewDto
                {
                    CopyNumber = c.CopyNumber,
                    ISBN = c.ISBN
                }).ToList();

                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo copias con ISBN {ISBN}", isbn);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // GET: api/copy/{copyNumber}/{isbn}/availability
        [HttpGet("{copyNumber}/{isbn}/availability")]
        public async Task<ActionResult<CopyAvailabilityDto>> GetAvailability(int copyNumber, string isbn)
        {
            try
            {
                var copy = await _copies
                    .Find(c => c.CopyNumber == copyNumber && c.ISBN == isbn)
                    .FirstOrDefaultAsync();

                if (copy == null)
                    return NotFound($"Copia número {copyNumber} con ISBN {isbn} no encontrada");

                // Verificar si está prestada
                var activeLoan = await _loans
                    .Find(l => l.CopyNumber == copyNumber && l.ReturnDate == null)
                    .FirstOrDefaultAsync();

                var dto = new CopyAvailabilityDto
                {
                    CopyNumber = copy.CopyNumber,
                    ISBN = copy.ISBN,
                    IsAvailable = activeLoan == null,
                    LoanDate = activeLoan?.LoanDate,
                    BorrowerRUT = activeLoan?.RUT
                };

                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verificando disponibilidad de copia {CopyNumber}/{ISBN}", copyNumber, isbn);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // POST: api/copy
        [HttpPost]
        public async Task<ActionResult<CopyViewDto>> Create([FromBody] CopyCreateDto dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest("Los datos de la copia no pueden ser nulos");

                if (dto.CopyNumber <= 0)
                    return BadRequest("El número de copia debe ser mayor a 0");

                if (string.IsNullOrWhiteSpace(dto.ISBN))
                    return BadRequest("El ISBN es obligatorio");

                // Verificar si ya existe una copia con ese número y ISBN
                var existingCopy = await _copies
                    .Find(c => c.CopyNumber == dto.CopyNumber && c.ISBN == dto.ISBN)
                    .FirstOrDefaultAsync();

                if (existingCopy != null)
                    return Conflict($"Ya existe una copia número {dto.CopyNumber} con ISBN {dto.ISBN}");

                var copy = new Copy
                {
                    CopyNumber = dto.CopyNumber,
                    ISBN = dto.ISBN
                };

                await _copies.InsertOneAsync(copy);

                var responseDto = new CopyViewDto
                {
                    CopyNumber = copy.CopyNumber,
                    ISBN = copy.ISBN
                };

                return CreatedAtAction(
                    nameof(GetByCopyNumberAndISBN), 
                    new { copyNumber = copy.CopyNumber, isbn = copy.ISBN }, 
                    responseDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando copia");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // PUT: api/copy/{copyNumber}/{isbn}
        [HttpPut("{copyNumber}/{isbn}")]
        public async Task<IActionResult> Update(int copyNumber, string isbn, [FromBody] CopyUpdateDto dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest("Los datos de la copia no pueden ser nulos");

                if (string.IsNullOrWhiteSpace(dto.ISBN))
                    return BadRequest("El ISBN es obligatorio");

                // Verificar si la copia existe
                var existingCopy = await _copies
                    .Find(c => c.CopyNumber == copyNumber && c.ISBN == isbn)
                    .FirstOrDefaultAsync();

                if (existingCopy == null)
                    return NotFound($"Copia número {copyNumber} con ISBN {isbn} no encontrada");

                // Verificar si el nuevo ISBN ya existe para este número de copia (si cambió)
                if (isbn != dto.ISBN)
                {
                    var duplicateCopy = await _copies
                        .Find(c => c.CopyNumber == copyNumber && c.ISBN == dto.ISBN)
                        .FirstOrDefaultAsync();

                    if (duplicateCopy != null)
                        return Conflict($"Ya existe una copia número {copyNumber} con ISBN {dto.ISBN}");
                }

                var filter = Builders<Copy>.Filter.And(
                    Builders<Copy>.Filter.Eq(c => c.CopyNumber, copyNumber),
                    Builders<Copy>.Filter.Eq(c => c.ISBN, isbn)
                );

                var update = Builders<Copy>.Update.Set(c => c.ISBN, dto.ISBN);

                var result = await _copies.UpdateOneAsync(filter, update);

                if (result.MatchedCount == 0)
                    return NotFound($"Copia número {copyNumber} con ISBN {isbn} no encontrada");

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando copia {CopyNumber}/{ISBN}", copyNumber, isbn);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // DELETE: api/copy/{copyNumber}/{isbn}
        [HttpDelete("{copyNumber}/{isbn}")]
        public async Task<IActionResult> Delete(int copyNumber, string isbn)
        {
            try
            {
                // Verificar si tiene préstamos activos
                var activeLoan = await _loans
                    .Find(l => l.CopyNumber == copyNumber && l.ReturnDate == null)
                    .FirstOrDefaultAsync();

                if (activeLoan != null)
                    return BadRequest("No se puede eliminar una copia con préstamos activos");

                var filter = Builders<Copy>.Filter.And(
                    Builders<Copy>.Filter.Eq(c => c.CopyNumber, copyNumber),
                    Builders<Copy>.Filter.Eq(c => c.ISBN, isbn)
                );

                var result = await _copies.DeleteOneAsync(filter);

                if (result.DeletedCount == 0)
                    return NotFound($"Copia número {copyNumber} con ISBN {isbn} no encontrada");

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error eliminando copia {CopyNumber}/{ISBN}", copyNumber, isbn);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // GET: api/copy/search?isbn={isbn}&copyNumber={copyNumber}
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<CopyViewDto>>> Search([FromQuery] string isbn, [FromQuery] int? copyNumber)
        {
            try
            {
                var filterBuilder = Builders<Copy>.Filter;
                var filter = filterBuilder.Empty;

                if (!string.IsNullOrWhiteSpace(isbn))
                {
                    filter &= filterBuilder.Regex(c => c.ISBN, new MongoDB.Bson.BsonRegularExpression(isbn, "i"));
                }

                if (copyNumber.HasValue)
                {
                    filter &= filterBuilder.Eq(c => c.CopyNumber, copyNumber.Value);
                }

                if (filter == filterBuilder.Empty)
                    return BadRequest("Debe proporcionar al menos un parámetro de búsqueda");

                var copies = await _copies.Find(filter).ToListAsync();

                var dto = copies.Select(c => new CopyViewDto
                {
                    CopyNumber = c.CopyNumber,
                    ISBN = c.ISBN
                }).ToList();

                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error buscando copias");
                return StatusCode(500, "Error interno del servidor");
            }
        }
    }
}
