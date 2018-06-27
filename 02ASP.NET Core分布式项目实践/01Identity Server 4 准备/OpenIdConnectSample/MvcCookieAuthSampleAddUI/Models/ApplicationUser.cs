
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;//Key
using System.ComponentModel.DataAnnotations.Schema;//DatabaseGenerated
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Identity;
namespace MvcCookieAuthSampleAddUI.Models
{
    public class ApplicationUser: IdentityUser<int>
    {
        //重写主键
        //[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        //[Key]
        //public override int Id { get; set; }

        public string Avatar { get; set; }
    }
}
