using System;
using System.Collections.Generic;

using System.Linq;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;

namespace MvcCookieAuthSampleAddUI.ViewModels
{
    public class LoginViewModel
    {
        [Required]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        public bool RemeberMe { get; set; }

       
    }
}
