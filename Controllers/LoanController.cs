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
    public class LoanController : ControllerBase
    {
        private readonly IMongoCollection<Loan> _loans;
        private readonly IMongoCollection<User> _users;
        private readonly IMongoCollection<Copy> _copies;
        private readonly ILogger<LoanController> _logger;

        public LoanController(IMongoDatabase database, ILogger<LoanController> logger)
        {
            _loans = database.GetCollection<Loan>("prestamos");
            _users = database.GetCollection<User>("usuarios");
            _copies = database.GetCollection<Copy>("copias");
            _logger = logger;
        }

        // GET: api/loan
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LoanViewDto>>> GetAll()
        {
            try
            {
                var loans = await _loans.Find(_ => true).ToListAsync();

                var dto = loans.Select(l => new LoanViewDto
                {
                    RUT = l.RUT,
                    CopyNumber = l.CopyNumber,
                    LoanDate = l.LoanDate,
                    ReturnDate = l.ReturnDate,
                    IsActive = l.ReturnDate == null
                }).ToList();

                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo préstamos");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // GET: api/loan/active
        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<LoanViewDto>>> GetActiveLoans()
        {
            try
            {
                var loans = await _loans
                    .Find(l => l.ReturnDate == null)
                    .ToListAsync();

                var dto = loans.Select(l => new LoanViewDto
                {
                    RUT = l.RUT,
                    CopyNumber = l.CopyNumber,
                    LoanDate = l.LoanDate,
                    ReturnDate = l.ReturnDate,
                    IsActive = true
                }).ToList();

                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo préstamos activos");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // GET: api/loan/user/{rut}
        [HttpGet("user/{rut}")]
        public async Task<ActionResult<IEnumerable<LoanViewDto>>> GetByUserRUT(string rut)
        {
            try
            {
                var loans = await _loans
                    .Find(l => l.RUT == rut)
                    .SortByDescending(l => l.LoanDate)
                    .ToListAsync();

                if (!loans.Any())
                    return NotFound($"No se encontraron préstamos para el usuario con RUT {rut}");

                var dto = loans.Select(l => new LoanViewDto
                {
                    RUT = l.RUT,
                    CopyNumber = l.CopyNumber,
                    LoanDate = l.LoanDate,
                    ReturnDate = l.ReturnDate,
                    IsActive = l.ReturnDate == null
                }).ToList();

                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo préstamos del usuario {RUT}", rut);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // GET: api/loan/copy/{copyNumber}
        [HttpGet("copy/{copyNumber}")]
        public async Task<ActionResult<IEnumerable<LoanViewDto>>> GetByCopyNumber(int copyNumber)
        {
            try
            {
                var loans = await _loans
                    .Find(l => l.CopyNumber == copyNumber)
                    .SortByDescending(l => l.LoanDate)
                    .ToListAsync();

                if (!loans.Any())
                    return NotFound($"No se encontraron préstamos para la copia número {copyNumber}");

                var dto = loans.Select(l => new LoanViewDto
                {
                    RUT = l.RUT,
                    CopyNumber = l.CopyNumber,
                    LoanDate = l.LoanDate,
                    ReturnDate = l.ReturnDate,
                    IsActive = l.ReturnDate == null
                }).ToList();

                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo préstamos de la copia {CopyNumber}", copyNumber);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // GET: api/loan/{rut}/{copyNumber}
        [HttpGet("{rut}/{copyNumber}")]
        public async Task<ActionResult<LoanViewDto>> GetByRUTAndCopyNumber(string rut, int copyNumber)
        {
            try
            {
                var loan = await _loans
                    .Find(l => l.RUT == rut && l.CopyNumber == copyNumber && l.ReturnDate == null)
                    .FirstOrDefaultAsync();

                if (loan == null)
                    return NotFound($"No se encontró un préstamo activo para RUT {rut} y copia {copyNumber}");

                var dto = new LoanViewDto
                {
                    RUT = loan.RUT,
                    CopyNumber = loan.CopyNumber,
                    LoanDate = loan.LoanDate,
                    ReturnDate = loan.ReturnDate,
                    IsActive = true
                };

                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo préstamo {RUT}/{CopyNumber}", rut, copyNumber);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // POST: api/loan
        [HttpPost]
        public async Task<ActionResult<LoanViewDto>> Create([FromBody] LoanCreateDto dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest("Los datos del préstamo no pueden ser nulos");

                if (string.IsNullOrWhiteSpace(dto.RUT))
                    return BadRequest("El RUT es obligatorio");

                if (dto.CopyNumber <= 0)
                    return BadRequest("El número de copia debe ser mayor a 0");

                // Verificar que el usuario existe
                var user = await _users.Find(u => u.RUT == dto.RUT).FirstOrDefaultAsync();
                if (user == null)
                    return NotFound($"Usuario con RUT {dto.RUT} no encontrado");

                // Verificar que la copia existe
                var copy = await _copies
                    .Find(c => c.CopyNumber == dto.CopyNumber && c.ISBN == dto.ISBN)
                    .FirstOrDefaultAsync();

                if (copy == null)
                    return NotFound($"Copia número {dto.CopyNumber} con ISBN {dto.ISBN} no encontrada");

                // Verificar que la copia no está prestada
                var activeLoan = await _loans
                    .Find(l => l.CopyNumber == dto.CopyNumber && l.ReturnDate == null)
                    .FirstOrDefaultAsync();

                if (activeLoan != null)
                    return Conflict($"La copia número {dto.CopyNumber} ya está prestada");

                // Verificar que el usuario no tiene ya esa copia
                var userHasCopy = await _loans
                    .Find(l => l.RUT == dto.RUT && l.CopyNumber == dto.CopyNumber && l.ReturnDate == null)
                    .FirstOrDefaultAsync();

                if (userHasCopy != null)
                    return Conflict($"El usuario ya tiene prestada la copia número {dto.CopyNumber}");

                var loan = new Loan
                {
                    RUT = dto.RUT,
                    CopyNumber = dto.CopyNumber,
                    LoanDate = DateTime.UtcNow,
                    ReturnDate = null
                };

                await _loans.InsertOneAsync(loan);

                var responseDto = new LoanViewDto
                {
                    RUT = loan.RUT,
                    CopyNumber = loan.CopyNumber,
                    LoanDate = loan.LoanDate,
                    ReturnDate = loan.ReturnDate,
                    IsActive = true
                };

                return CreatedAtAction(
                    nameof(GetByRUTAndCopyNumber),
                    new { rut = loan.RUT, copyNumber = loan.CopyNumber },
                    responseDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando préstamo");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // PUT: api/loan/{rut}/{copyNumber}/return
        [HttpPut("{rut}/{copyNumber}/return")]
        public async Task<IActionResult> ReturnLoan(string rut, int copyNumber, [FromBody] LoanReturnDto dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest("Los datos de devolución no pueden ser nulos");

                // Buscar el préstamo activo
                var loan = await _loans
                    .Find(l => l.RUT == rut && l.CopyNumber == copyNumber && l.ReturnDate == null)
                    .FirstOrDefaultAsync();

                if (loan == null)
                    return NotFound($"No se encontró un préstamo activo para RUT {rut} y copia {copyNumber}");

                // Validar fecha de devolución
                var returnDate = dto.ReturnDate == default(DateTime) ? DateTime.UtcNow : dto.ReturnDate;

                if (returnDate < loan.LoanDate)
                    return BadRequest("La fecha de devolución no puede ser anterior a la fecha de préstamo");

                var filter = Builders<Loan>.Filter.And(
                    Builders<Loan>.Filter.Eq(l => l.RUT, rut),
                    Builders<Loan>.Filter.Eq(l => l.CopyNumber, copyNumber),
                    Builders<Loan>.Filter.Eq(l => l.ReturnDate, null)
                );

                var update = Builders<Loan>.Update.Set(l => l.ReturnDate, returnDate);

                var result = await _loans.UpdateOneAsync(filter, update);

                if (result.MatchedCount == 0)
                    return NotFound($"No se encontró un préstamo activo para RUT {rut} y copia {copyNumber}");

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error devolviendo préstamo {RUT}/{CopyNumber}", rut, copyNumber);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // DELETE: api/loan/{rut}/{copyNumber}
        [HttpDelete("{rut}/{copyNumber}")]
        public async Task<IActionResult> Delete(string rut, int copyNumber)
        {
            try
            {
                // Solo se pueden eliminar préstamos ya devueltos (con fines de limpieza de histórico)
                var filter = Builders<Loan>.Filter.And(
                    Builders<Loan>.Filter.Eq(l => l.RUT, rut),
                    Builders<Loan>.Filter.Eq(l => l.CopyNumber, copyNumber),
                    Builders<Loan>.Filter.Ne(l => l.ReturnDate, null) // Solo devueltos
                );

                var result = await _loans.DeleteOneAsync(filter);

                if (result.DeletedCount == 0)
                    return NotFound($"No se encontró un préstamo devuelto para RUT {rut} y copia {copyNumber}");

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error eliminando préstamo {RUT}/{CopyNumber}", rut, copyNumber);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // GET: api/loan/overdue?days={days}
        [HttpGet("overdue")]
        public async Task<ActionResult<IEnumerable<LoanViewDto>>> GetOverdueLoans([FromQuery] int days = 30)
        {
            try
            {
                var overdueDate = DateTime.UtcNow.AddDays(-days);

                var loans = await _loans
                    .Find(l => l.ReturnDate == null && l.LoanDate < overdueDate)
                    .ToListAsync();

                var dto = loans.Select(l => new LoanViewDto
                {
                    RUT = l.RUT,
                    CopyNumber = l.CopyNumber,
                    LoanDate = l.LoanDate,
                    ReturnDate = l.ReturnDate,
                    IsActive = true,
                    DaysOverdue = (int)(DateTime.UtcNow - l.LoanDate).TotalDays
                }).ToList();

                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo préstamos vencidos");
                return StatusCode(500, "Error interno del servidor");
            }
        }
    }
}
