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
    public class ReportController : ControllerBase
    {
        private readonly IMongoCollection<Copy> _copies;
        private readonly IMongoCollection<Edition> _editions;
        private readonly IMongoCollection<Book> _books;
        private readonly IMongoCollection<Author> _authors;
        private readonly IMongoCollection<Loan> _loans;
        private readonly IMongoCollection<User> _users;
        private readonly ILogger<ReportController> _logger;

        public ReportController(IMongoDatabase database, ILogger<ReportController> logger)
        {
            _copies = database.GetCollection<Copy>("copias");
            _editions = database.GetCollection<Edition>("ediciones");
            _books = database.GetCollection<Book>("libros");
            _authors = database.GetCollection<Author>("autores");
            _loans = database.GetCollection<Loan>("prestamos");
            _users = database.GetCollection<User>("usuarios");
            _logger = logger;
        }

        // GET: api/report/copies-full-info
        // CONSULTA 1: Listado de copias con información de AUTOR, LIBRO, EDICIÓN y COPIA
        [HttpGet("copies-full-info")]
        public async Task<ActionResult<IEnumerable<CopyFullInfoDto>>> GetCopiesFullInfo()
        {
            try
            {
                // Obtener todas las copias
                var copies = await _copies.Find(_ => true).ToListAsync();

                var result = new List<CopyFullInfoDto>();

                foreach (var copy in copies)
                {
                    // Obtener edición de la copia
                    var edition = await _editions
                        .Find(e => e.ISBN == copy.ISBN)
                        .FirstOrDefaultAsync();

                    if (edition == null) continue;

                    // Obtener libro de la edición
                    var book = await _books
                        .Find(b => b.Title == edition.BookTitle)
                        .FirstOrDefaultAsync();

                    if (book == null) continue;

                    // Obtener autores del libro (basado en la relación escribe_libro)
                    // Nota: Esto requiere que tengas una forma de relacionar libros con autores
                    // Por simplicidad, aquí buscaremos todos los autores
                    var authors = await _authors.Find(_ => true).ToListAsync();

                    var dto = new CopyFullInfoDto
                    {
                        // Información de la COPIA
                        CopyNumber = copy.CopyNumber,
                        
                        // Información de la EDICIÓN
                        ISBN = edition.ISBN,
                        PublicationDate = edition.PublicationDate,
                        Language = edition.Language,
                        
                        // Información del LIBRO
                        BookTitle = book.Title,
                        
                        // Información de AUTORES
                        Authors = authors.Select(a => a.Name).ToList()
                    };

                    result.Add(dto);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo listado completo de copias");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // GET: api/report/user-loans/{rut}
        // CONSULTA 2: Listado de libros prestados por un usuario
        [HttpGet("user-loans/{rut}")]
        public async Task<ActionResult<IEnumerable<UserLoanDto>>> GetUserLoans(string rut)
        {
            try
            {
                // Verificar que el usuario existe
                var user = await _users.Find(u => u.RUT == rut).FirstOrDefaultAsync();
                
                if (user == null)
                    return NotFound($"Usuario con RUT {rut} no encontrado");

                // Obtener préstamos activos del usuario
                var loans = await _loans
                    .Find(l => l.RUT == rut && l.ReturnDate == null)
                    .ToListAsync();

                var result = new List<UserLoanDto>();

                foreach (var loan in loans)
                {
                    // Obtener copia
                    var copy = await _copies
                        .Find(c => c.CopyNumber == loan.CopyNumber)
                        .FirstOrDefaultAsync();

                    if (copy == null) continue;

                    // Obtener edición
                    var edition = await _editions
                        .Find(e => e.ISBN == copy.ISBN)
                        .FirstOrDefaultAsync();

                    if (edition == null) continue;

                    // Obtener libro
                    var book = await _books
                        .Find(b => b.Title == edition.BookTitle)
                        .FirstOrDefaultAsync();

                    if (book == null) continue;

                    // Obtener autores
                    var authors = await _authors.Find(_ => true).ToListAsync();

                    var dto = new UserLoanDto
                    {
                        // Información del usuario
                        UserRUT = user.RUT,
                        UserName = user.Name,
                        
                        // Información del préstamo
                        LoanDate = loan.LoanDate,
                        CopyNumber = loan.CopyNumber,
                        
                        // Información del libro
                        BookTitle = book.Title,
                        ISBN = edition.ISBN,
                        Language = edition.Language,
                        PublicationDate = edition.PublicationDate,
                        
                        // Información de autores
                        Authors = authors.Select(a => a.Name).ToList()
                    };

                    result.Add(dto);
                }

                if (!result.Any())
                    return Ok(new { Message = $"El usuario {user.Name} no tiene préstamos activos", Loans = result });

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo préstamos del usuario {RUT}", rut);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // GET: api/report/all-user-loans/{rut}
        // CONSULTA 2 (variante): Incluye historial completo de préstamos
        [HttpGet("all-user-loans/{rut}")]
        public async Task<ActionResult<UserLoansReportDto>> GetAllUserLoans(string rut)
        {
            try
            {
                var user = await _users.Find(u => u.RUT == rut).FirstOrDefaultAsync();
                
                if (user == null)
                    return NotFound($"Usuario con RUT {rut} no encontrado");

                // Obtener TODOS los préstamos (activos e históricos)
                var allLoans = await _loans
                    .Find(l => l.RUT == rut)
                    .SortByDescending(l => l.LoanDate)
                    .ToListAsync();

                var activeLoans = new List<UserLoanDto>();
                var returnedLoans = new List<UserLoanDto>();

                foreach (var loan in allLoans)
                {
                    var copy = await _copies.Find(c => c.CopyNumber == loan.CopyNumber).FirstOrDefaultAsync();
                    if (copy == null) continue;

                    var edition = await _editions.Find(e => e.ISBN == copy.ISBN).FirstOrDefaultAsync();
                    if (edition == null) continue;

                    var book = await _books.Find(b => b.Title == edition.BookTitle).FirstOrDefaultAsync();
                    if (book == null) continue;

                    var authors = await _authors.Find(_ => true).ToListAsync();

                    var dto = new UserLoanDto
                    {
                        UserRUT = user.RUT,
                        UserName = user.Name,
                        LoanDate = loan.LoanDate,
                        ReturnDate = loan.ReturnDate,
                        CopyNumber = loan.CopyNumber,
                        BookTitle = book.Title,
                        ISBN = edition.ISBN,
                        Language = edition.Language,
                        PublicationDate = edition.PublicationDate,
                        Authors = authors.Select(a => a.Name).ToList()
                    };

                    if (loan.ReturnDate == null)
                        activeLoans.Add(dto);
                    else
                        returnedLoans.Add(dto);
                }

                var report = new UserLoansReportDto
                {
                    UserRUT = user.RUT,
                    UserName = user.Name,
                    TotalActiveLoans = activeLoans.Count,
                    TotalReturnedLoans = returnedLoans.Count,
                    ActiveLoans = activeLoans,
                    ReturnedLoans = returnedLoans
                };

                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo reporte completo de préstamos del usuario {RUT}", rut);
                return StatusCode(500, "Error interno del servidor");
            }
        }
    }
}