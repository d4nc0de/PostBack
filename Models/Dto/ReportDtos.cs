using System;
using System.Collections.Generic;

namespace HR.Models.Dto
{
    // DTO para CONSULTA 1: Copias con información completa
    public class CopyFullInfoDto
    {
        // COPIA
        public int CopyNumber { get; set; }
        
        // EDICIÓN
        public string ISBN { get; set; }
        public DateTime PublicationDate { get; set; }
        public string Language { get; set; }
        
        // LIBRO
        public string BookTitle { get; set; }
        
        // AUTORES
        public List<string> Authors { get; set; } = new List<string>();
    }

    // DTO para CONSULTA 2: Préstamos de un usuario
    public class UserLoanDto
    {
        // USUARIO
        public string UserRUT { get; set; }
        public string UserName { get; set; }
        
        // PRÉSTAMO
        public DateTime LoanDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public int CopyNumber { get; set; }
        
        // LIBRO
        public string BookTitle { get; set; }
        
        // EDICIÓN
        public string ISBN { get; set; }
        public string Language { get; set; }
        public DateTime PublicationDate { get; set; }
        
        // AUTORES
        public List<string> Authors { get; set; } = new List<string>();
    }

    // DTO para reporte completo de usuario
    public class UserLoansReportDto
    {
        public string UserRUT { get; set; }
        public string UserName { get; set; }
        public int TotalActiveLoans { get; set; }
        public int TotalReturnedLoans { get; set; }
        public List<UserLoanDto> ActiveLoans { get; set; } = new List<UserLoanDto>();
        public List<UserLoanDto> ReturnedLoans { get; set; } = new List<UserLoanDto>();
    }
}