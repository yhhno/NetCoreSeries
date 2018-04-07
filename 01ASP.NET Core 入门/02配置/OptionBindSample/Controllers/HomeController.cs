using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace OptionBindSample.Controllers
{
    public class HomeController : Controller
    {
        //仅仅是在视图中使用option,这个代码有点多,我们可以直接在视图中通过依赖注入使用option,我们直接在视图中把配置从依赖注入中读取出来
        private readonly Class _myclass;
        ////13
        //public HomeController(IOptions<Class> classAccesser)//依赖注入的方式

        //14 热更新
        public HomeController(IOptionsSnapshot<Class> classAccesser)//依赖注入的方式
        {
            _myclass = classAccesser.Value;
        }

        public IActionResult Index()
        {
            return View(_myclass);//在视图中使用这个模型类实例_myclass
        }
        public IActionResult Index2()
        {
            return View();
        }
    }
}