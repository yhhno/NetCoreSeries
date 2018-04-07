using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JwtAuthSample.Models
{
    public class JwtSettings//配置的一个映射实体类
    {
        public string Issuer { get; set; }//token是谁颁发的
        public  string Audience { get; set; }//token可以给那些客户端使用
        public string SecretKey { get; set; }
    }
}
