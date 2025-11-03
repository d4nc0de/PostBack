using System;

namespace HR.Models
{
    public class Edition
    {
        public string ISBN { get; set; }
        public DateTime publicationDate { get; set; }
        public string language { get; set; }
        public string bookTitle { get; set; }
    }
}