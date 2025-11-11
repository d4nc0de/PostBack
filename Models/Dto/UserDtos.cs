using System.Collections.Generic;

namespace HR.Models.Dto
{
    public class UserViewDto
    {
        public string RUT { get; set; }
        public string Name { get; set; }
    }

    public class UserDetailDto
    {
        public string RUT { get; set; }
        public string Name { get; set; }
        public List<LoanViewDto> ActiveLoans { get; set; } = new List<LoanViewDto>();
        public List<LoanViewDto> LoanHistory { get; set; } = new List<LoanViewDto>();
    }

    public class UserCreateDto
    {
        public string RUT { get; set; }
        public string Name { get; set; }
    }

    public class UserUpdateDto
    {
        public string Name { get; set; }
    }
}