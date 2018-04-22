using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MvcCookieAuthSampleAddUI.ViewModels
{
    public class InputConsetViewModel
    {
        public string  Button { get; set; }//接收 授权 或者取消
        public IEnumerable<string> ScopesConsented { get; set; }
        public bool RemeberConsent { get; set; }
        public string ReturnUrl { get; set; }
    }
}
