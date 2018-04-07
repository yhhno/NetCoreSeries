using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace JwtAuthSample.ViewModels
{
    public class LoginViewModel
    {
        [Required]//添加model,注释标签
        public string User { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
