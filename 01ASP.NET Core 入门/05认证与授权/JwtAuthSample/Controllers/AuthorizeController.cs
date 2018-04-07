using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;//对应 IActionResult

using JwtAuthSample.ViewModels;
using System.Security.Claims;//对应Claim
using JwtAuthSample.Models;
using Microsoft.Extensions.Options;// 读取配置信息
using Microsoft.IdentityModel.Tokens;//SymmetricSecurityKey 对称加密的一种方式  这个命名空间下,会有一些生成token的一些加密算法   对应 SymmetricSecurityKey
using System.Text;
using System.IdentityModel.Tokens.Jwt;// JwtSecurityToken

namespace JwtAuthSample.Controllers
{
   
    [Route("api/[controller]")]
    public class AuthorizeController : Controller
    {

        private JwtSettings _jwtSettings;
        public AuthorizeController(IOptions<JwtSettings> _jwtSettingsAccesser)//对应需要在startup中注册
        {
            _jwtSettings = _jwtSettingsAccesser.Value;
        }
        [HttpPost]
        [Route("token")]
        public IActionResult Token([FromBody]LoginViewModel viewmodel)//接收参数为LoginViewModel
        {
            if(ModelState.IsValid)//合法
            {
                if (!(viewmodel.User == "jesse" && viewmodel.Password == "123456"))//验证参数是否存在
                {
                    return BadRequest();
                }

                //生成token
                var claims = new Claim[]
                {
                    new Claim(ClaimTypes.Name,"jesse"),
                new Claim(ClaimTypes.Role,"admin"),
                new Claim("SuperAdminOnly","true")

                };
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    _jwtSettings.Issuer,
                    _jwtSettings.Audience,
                    claims,
                    DateTime.Now,
                    DateTime.Now.AddMinutes(30),
                    creds);
                return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
            }

            return BadRequest();//不合法
        }

        [HttpGet]
        [Route("test")]
        public IActionResult test()
        {
            return Ok();
        }
    }
}