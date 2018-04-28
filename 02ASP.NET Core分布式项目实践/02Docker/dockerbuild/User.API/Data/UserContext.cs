
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


using Microsoft.EntityFrameworkCore;//对应DbContext
using User.API.Models;

namespace User.API.Data
{
    public class UserContext: DbContext
    {
        //为什么要这么写呢  EF要求这么写， 看看EF的base方法 源码就可以了 就知道背后做了啥？ 也可以猜想下做了什么? 前因后果，来龙去脉
        public UserContext(DbContextOptions<UserContext> options):base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)//重写方法，扩展点，
        {
            modelBuilder.Entity<AppUser>()
                .ToTable("Users")
                .HasKey(u => u.Id);


            base.OnModelCreating(modelBuilder);
        }


        public DbSet<AppUser> Users { get; set; }//干啥用
    }
}
