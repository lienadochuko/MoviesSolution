using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoviesApi.Models
{
    public class LoginDTO
    {
        [Required(ErrorMessage = "Please input your email")]
        [EmailAddress(ErrorMessage = "Email should be a proper Email")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required(ErrorMessage = "Please input your password")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

    }
}
