using System;

namespace HR.Models
{
    public class Loan
    {
        public string rut { get; set; }
        public int copy_number { get; set; }
        public DateTime loanDate { get; set; }
        public DateTime dueDate { get; set; }   
    }
}
