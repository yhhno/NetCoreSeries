using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MvcCookieAuthSampleAddUI.ViewModels
{
    public class ProcessConsentResult
    {
        public string RedirectUrl { get; set; }
        public bool IsRedirect => RedirectUrl != null;//标识，当RedirectUrl不为null的时候，跳转  有啥妙用呢？

        public string  ValidationError { get; set; }

        public ConsentViewModel ViewModel { get; set; }
    }
}
