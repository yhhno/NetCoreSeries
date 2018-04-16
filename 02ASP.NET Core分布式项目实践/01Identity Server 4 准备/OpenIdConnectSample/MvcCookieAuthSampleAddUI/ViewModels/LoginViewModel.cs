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
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

       
    }
}
