// Models/Dto/PostViewDto.cs
using System;
using System.Collections.Generic;

namespace HR.Models.Dto
{
    public class PostViewDto
    {
        public int idp { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public string category { get; set; }
        public UserViewDto CreatedBy { get; set; }
        public List<CommentViewDto> comments { get; set; }
    }

    public class UserViewDto
    {
        public int id { get; set; }
        public string name { get; set; }
        public string mail { get; set; }
    }

    public class CommentViewDto
    {
        public int consec { get; set; }
        public string content { get; set; }
        public DateTime createdAt { get; set; }
        public bool likeNotLike { get; set; }
        public DateTime? authorAt { get; set; } // según tu modelo fechorAut
    }
}
