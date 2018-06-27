
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims; //对应Claim
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer4.Models; //JwtClaimTypes
using IdentityServer4.Services;//对应IProfileService
using Microsoft.AspNetCore.Identity;//对应UserManager
using MvcCookieAuthSampleAddUI.Models;

namespace MvcCookieAuthSampleAddUI.Services
{
    //identity.server4和asp.netcore identity 为我们封装了很多东西，方便了我们。
    public class ProfileService : IProfileService// IdentityServer4中定义了抽象，让我们可以自定义实现
    {
        //identity.server4和asp.netcore identity 为我们封装了很多东西，方便了我们。
        private UserManager<ApplicationUser> _userManager;//与user的管理有关, 增删改查

        public ProfileService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
            
        }

        private async Task<List<Claim>> GetClaimsFromUserAsync(ApplicationUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtClaimTypes.Subject,user.Id.ToString()),
                new Claim(JwtClaimTypes.PreferredUserName,user.UserName)

            };

            //角色信息,角色信息需要在用户添加时，赋值， 否则为空
            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                claims.Add(new Claim(JwtClaimTypes.Role, role));
            }

            if (!string.IsNullOrWhiteSpace(user.Avatar))
            {
                claims.Add(new Claim("avatar", user.Avatar));
            }

            return claims;
        }

        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var subjectId = context.Subject.Claims.FirstOrDefault(c => c.Type == "sub").Value;

            //identity.server4和asp.netcore identity 为我们封装了很多东西，方便了我们。
            var user = await _userManager.FindByIdAsync(subjectId);//此处有个问题，异步方法本应该添加await关键字的，但是此方法没有async，就不知道怎么办了？
            var claims = await GetClaimsFromUserAsync(user);
            context.IssuedClaims = claims;
        }

        public async Task IsActiveAsync(IsActiveContext context) //虽然接口定义中没有加async，但是在实现中可以添加async
        {
            context.IsActive = false;
            var subjectId = context.Subject.Claims.FirstOrDefault(c => c.Type == "sub").Value;
            //identity.server4和asp.netcore identity 为我们封装了很多东西，方便了我们。
            var user = await  _userManager.FindByIdAsync(subjectId);//此处有个问题，异步方法本应该添加await关键字的，但是此方法没有async，就不知道怎么办了？
            //if(user!=null)
            //{
            //    context.IsActive = true;
            //}
            context.IsActive = user != null;
        }
    }
}
