using System;

namespace HR.Models.Dto
{
    public class LoanViewDto
    {
        public string RUT { get; set; }
        public int CopyNumber { get; set; }
        public DateTime LoanDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public bool IsActive { get; set; }
        public int DaysOverdue { get; set; }
    }

    public class LoanCreateDto
    {
        public string RUT { get; set; }
        public int CopyNumber { get; set; }
        public string ISBN { get; set; } // Para validar que la copia existe
    }

    public class LoanReturnDto
    {
        public DateTime ReturnDate { get; set; }
    }
}