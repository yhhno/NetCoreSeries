using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MvcCookieAuthSampleAddUI.ViewModels
{
    //看官方文档，
    public class ScopeViewModel//这些属性都是根据identityserver4的要求来的， 因为我们的页面要和identityserver4提供的功能类库交互呀， 我们只是调用，当然要传递符合要求的东西，  
    {
        public  string Name { get; set; }//
        public  string DisplayName { get; set; }//
        public string Description { get; set; }//描述
        public bool Emphasize { get; set; }//是否强调
        public bool Required { get; set; }//是不是必须的

        public bool Checked { get; set; }//是否选择
    }
}
