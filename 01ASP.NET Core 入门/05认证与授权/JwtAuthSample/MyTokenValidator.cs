
using System;
using System.Collections.Generic;
using System.Linq;

using System.Threading.Tasks;

using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;//对应ISecurityTokenValidator
namespace JwtAuthSample
{
    public class MyTokenValidator : ISecurityTokenValidator
    {
        public bool CanValidateToken => true;

        public int MaximumTokenSizeInBytes { get; set; }//给外边设置

        public bool CanReadToken(string securityToken)
        {
            return true;
        }

        public ClaimsPrincipal ValidateToken(string securityToken, TokenValidationParameters validationParameters, out SecurityToken validatedToken)
        {
            validatedToken = null;
            var identity = new ClaimsIdentity(JwtBearerDefaults.AuthenticationScheme);
            if (securityToken == "123456")//验证token通过.
            {

                identity.AddClaim(new Claim("name", "jesse"));
                identity.AddClaim(new Claim(ClaimsIdentity.DefaultRoleClaimType, "admin"));
                identity.AddClaim(new Claim("SuperAdminOnly", "true"));
            }

            //验证token不通过的话,返回ClaimsPrincipal类型的空实例
            var principal = new ClaimsPrincipal(identity);
           

            return principal;
        }

    }
}
