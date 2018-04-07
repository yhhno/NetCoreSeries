
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


using Microsoft.AspNetCore.Identity.EntityFrameworkCore;//对应啥?IdentityDbContext
using Microsoft.EntityFrameworkCore;//对应啥? DbContextOptions
using MvcCookieAuthSampleAddUI.Models;
namespace MvcCookieAuthSampleAddUI.Data
{
    public class ApplicationDbContext:IdentityDbContext<ApplicationUser,ApplicationUserRole,int>//为什么要有泛型参数呢?
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options):base(options)//需要一个构造函数来接收options
        {

        }
    }
}
