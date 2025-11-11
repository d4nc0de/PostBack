using System;

namespace HR.Models.Dto
{
    public class CopyViewDto
    {
        public int CopyNumber { get; set; }
        public string ISBN { get; set; }
    }

    public class CopyCreateDto
    {
        public int CopyNumber { get; set; }
        public string ISBN { get; set; }
    }

    public class CopyUpdateDto
    {
        public string ISBN { get; set; }
    }

    public class CopyAvailabilityDto
    {
        public int CopyNumber { get; set; }
        public string ISBN { get; set; }
        public bool IsAvailable { get; set; }
        public DateTime? LoanDate { get; set; }
        public string BorrowerRUT { get; set; }
    }
}