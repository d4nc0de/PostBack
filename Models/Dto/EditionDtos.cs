using System;
using System.Collections.Generic;

namespace HR.Models.Dto
{
    public class EditionViewDto
    {
        public string ISBN { get; set; }
        public DateTime PublicationDate { get; set; }
        public string Language { get; set; }
        public string BookTitle { get; set; }
    }

    public class EditionDetailDto
    {
        public string ISBN { get; set; }
        public DateTime PublicationDate { get; set; }
        public string Language { get; set; }
        public string BookTitle { get; set; }
        public int TotalCopies { get; set; }
        public List<CopyViewDto> Copies { get; set; } = new List<CopyViewDto>();
    }

    public class EditionCreateDto
    {
        public string ISBN { get; set; }
        public DateTime PublicationDate { get; set; }
        public string Language { get; set; }
        public string BookTitle { get; set; }
    }

    public class EditionUpdateDto
    {
        public DateTime PublicationDate { get; set; }
        public string Language { get; set; }
        public string BookTitle { get; set; }
    }
}