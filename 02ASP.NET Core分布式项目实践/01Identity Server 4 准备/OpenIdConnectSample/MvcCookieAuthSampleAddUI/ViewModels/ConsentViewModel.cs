﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MvcCookieAuthSampleAddUI.ViewModels
{
    public class ConsentViewModel:InputConsetViewModel
    {
        public string ClientId { get; set; }
        public string ClientName { get; set; }
        public string ClientLogoUrl { get; set; }
        public string ClientUrl { get; set; }
     

        public IEnumerable<ScopeViewModel> IdentityScopes
        { get; set; }//可供个人信息，供选择 

        public IEnumerable<ScopeViewModel> ResourceScopes

        { get; set; }//可供Api信息，供选择 


        //因为这两项在InputConsetViewModel中也出现了，我们可以使用继承来避免重复
        //public bool AllowRememberConsent { get; set; }//是否记住
        //public string ReturnUrl { get; set; }

    }
}
