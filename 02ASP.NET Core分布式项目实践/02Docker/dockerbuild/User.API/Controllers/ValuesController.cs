using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;



using Microsoft.EntityFrameworkCore;//SingleOrDefaultAsync
using User.API.Data;

namespace User.API.Controllers
{
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        private UserContext _userContext;//私有变量
        public ValuesController(UserContext userContext)//依赖注入 实例
        {
            _userContext = userContext;
        }


        // GET api/values
        [HttpGet]
        public async Task<IActionResult>  Get()
        {

            //返回一个匿名对象
            return Json(new
            {
                message = "welcome to gitlab ci build",
                user = await _userContext.Users.SingleOrDefaultAsync(u => u.Name == "jesse")
            });//如果不存在，返回null
        } 


        //// GET api/values
        //[HttpGet]
        //public IEnumerable<string> Get()
        //{
        //    return new string[] { "value1", "value2" };
        //}

        //// GET api/values/5
        //[HttpGet("{id}")]
        //public string Get(int id)
        //{
        //    return "value";
        //}

        //// POST api/values
        //[HttpPost]
        //public void Post([FromBody]string value)
        //{
        //}

        //// PUT api/values/5
        //[HttpPut("{id}")]
        //public void Put(int id, [FromBody]string value)
        //{
        //}

        //// DELETE api/values/5
        //[HttpDelete("{id}")]
        //public void Delete(int id)
        //{
        //}
    }
}
