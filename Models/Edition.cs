using System;

namespace HR.Models
{
    public class Edition
    {
        public string ISBN { get; set; }
        public DateTime PublicationDate { get; set; }
        public string Language { get; set; }
        public string BookTitle { get; set; }
    }
}