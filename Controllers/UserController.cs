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
    public class UserController : ControllerBase
    {
        private readonly IMongoCollection<User> _users;
        private readonly IMongoCollection<Loan> _loans;
        private readonly ILogger<UserController> _logger;

        public UserController(IMongoDatabase database, ILogger<UserController> logger)
        {
            _users = database.GetCollection<User>("usuarios");
            _loans = database.GetCollection<Loan>("prestamos");
            _logger = logger;
        }

        // GET: api/user
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserViewDto>>> GetAll()
        {
            try
            {
                var users = await _users.Find(_ => true).ToListAsync();

                var dto = users.Select(u => new UserViewDto
                {
                    RUT = u.RUT,
                    Name = u.Name
                }).ToList();

                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo usuarios");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // GET: api/user/{rut}
        [HttpGet("{rut}")]
        public async Task<ActionResult<UserViewDto>> GetByRUT(string rut)
        {
            try
            {
                var user = await _users.Find(u => u.RUT == rut).FirstOrDefaultAsync();

                if (user == null)
                    return NotFound($"Usuario con RUT {rut} no encontrado");

                var dto = new UserViewDto
                {
                    RUT = user.RUT,
                    Name = user.Name
                };

                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo usuario con RUT {RUT}", rut);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // GET: api/user/{rut}/detail (con préstamos)
        [HttpGet("{rut}/detail")]
        public async Task<ActionResult<UserDetailDto>> GetDetailByRUT(string rut)
        {
            try
            {
                var user = await _users.Find(u => u.RUT == rut).FirstOrDefaultAsync();

                if (user == null)
                    return NotFound($"Usuario con RUT {rut} no encontrado");

                // Obtener préstamos activos
                var activeLoans = await _loans
                    .Find(l => l.RUT == user.RUT && l.ReturnDate == null)
                    .ToListAsync();

                // Obtener historial de préstamos
                var loanHistory = await _loans
                    .Find(l => l.RUT == user.RUT && l.ReturnDate != null)
                    .SortByDescending(l => l.LoanDate)
                    .Limit(10)
                    .ToListAsync();

                var dto = new UserDetailDto
                {
                    RUT = user.RUT,
                    Name = user.Name,
                    ActiveLoans = activeLoans.Select(l => new LoanViewDto
                    {
                        RUT = l.RUT,
                        CopyNumber = l.CopyNumber,
                        LoanDate = l.LoanDate,
                        ReturnDate = l.ReturnDate,
                        IsActive = true
                    }).ToList(),
                    LoanHistory = loanHistory.Select(l => new LoanViewDto
                    {
                        RUT = l.RUT,
                        CopyNumber = l.CopyNumber,
                        LoanDate = l.LoanDate,
                        ReturnDate = l.ReturnDate,
                        IsActive = false
                    }).ToList()
                };

                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo detalle del usuario {RUT}", rut);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // POST: api/user
        [HttpPost]
        public async Task<ActionResult<UserViewDto>> Create([FromBody] UserCreateDto dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest("Los datos del usuario no pueden ser nulos");

                if (string.IsNullOrWhiteSpace(dto.RUT))
                    return BadRequest("El RUT es obligatorio");

                if (string.IsNullOrWhiteSpace(dto.Name))
                    return BadRequest("El nombre es obligatorio");

                // Verificar si ya existe un usuario con ese RUT
                var existingUser = await _users
                    .Find(u => u.RUT == dto.RUT)
                    .FirstOrDefaultAsync();

                if (existingUser != null)
                    return Conflict($"Ya existe un usuario con RUT {dto.RUT}");

                var user = new User
                {
                    RUT = dto.RUT,
                    Name = dto.Name
                };

                await _users.InsertOneAsync(user);

                var responseDto = new UserViewDto
                {
                    RUT = user.RUT,
                    Name = user.Name
                };

                return CreatedAtAction(nameof(GetByRUT), new { rut = user.RUT }, responseDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando usuario");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // PUT: api/user/{rut}
        [HttpPut("{rut}")]
        public async Task<IActionResult> Update(string rut, [FromBody] UserUpdateDto dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest("Los datos del usuario no pueden ser nulos");

                if (string.IsNullOrWhiteSpace(dto.Name))
                    return BadRequest("El nombre es obligatorio");

                var filter = Builders<User>.Filter.Eq(u => u.RUT, rut);
                var update = Builders<User>.Update.Set(u => u.Name, dto.Name);

                var result = await _users.UpdateOneAsync(filter, update);

                if (result.MatchedCount == 0)
                    return NotFound($"Usuario con RUT {rut} no encontrado");

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando usuario {RUT}", rut);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // DELETE: api/user/{rut}
        [HttpDelete("{rut}")]
        public async Task<IActionResult> Delete(string rut)
        {
            try
            {
                // Verificar si tiene préstamos activos
                var activeLoans = await _loans
                    .CountDocumentsAsync(l => l.RUT == rut && l.ReturnDate == null);

                if (activeLoans > 0)
                    return BadRequest("No se puede eliminar un usuario con préstamos activos");

                var filter = Builders<User>.Filter.Eq(u => u.RUT, rut);
                var result = await _users.DeleteOneAsync(filter);

                if (result.DeletedCount == 0)
                    return NotFound($"Usuario con RUT {rut} no encontrado");

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error eliminando usuario {RUT}", rut);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // GET: api/user/search?name={name}
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<UserViewDto>>> Search([FromQuery] string name)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                    return BadRequest("El parámetro de búsqueda no puede estar vacío");

                var filter = Builders<User>.Filter.Regex(u => u.Name, new MongoDB.Bson.BsonRegularExpression(name, "i"));
                var users = await _users.Find(filter).ToListAsync();

                var dto = users.Select(u => new UserViewDto
                {
                    RUT = u.RUT,
                    Name = u.Name
                }).ToList();

                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error buscando usuarios con nombre '{Name}'", name);
                return StatusCode(500, "Error interno del servidor");
            }
        }
    }
}
