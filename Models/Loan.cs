using System;

namespace HR.Models
{
    public class Loan
    {
        public string RUT { get; set; }
        public int CopyNumber { get; set; }
        public DateTime LoanDate { get; set; }
        public DateTime? ReturnDate { get; set; }   
    }
}
