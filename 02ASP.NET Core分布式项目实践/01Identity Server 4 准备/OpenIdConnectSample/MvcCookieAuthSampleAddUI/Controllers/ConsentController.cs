using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;//对应 Controller

using MvcCookieAuthSampleAddUI.ViewModels;

using IdentityServer4.Services;
using IdentityServer4.Stores;
using IdentityServer4.Models;//对应AuthorizationRequest request,Client client,Resources resources
using MvcCookieAuthSampleAddUI.Services;

namespace MvcCookieAuthSampleAddUI.Controllers
{
    public class ConsentController : Controller
    {

        private readonly ConsentService _consentService;
        public ConsentController(ConsentService consentService)
        {
            _consentService = consentService;
        }


  
        [HttpGet]
        public async  Task<IActionResult> Index(string returnUrl)
        {
            //逻辑清晰，就生成model
            //var model = await BuildConsentViewModel(returnUrl);//专人做专事
            var model = await _consentService.BuildConsentViewModelAsync(returnUrl);//专人做专事

            if (model==null)
            {
                //会加上错误处理，当一个都没选的情况下，或者说根据url没有获取到我们的model
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Index(InputConsetViewModel viewModel)
        {

            //return View();//这句代码说明当用户什么都没有选的时候，我们还会继续 return index的view，还需要丢一个model  get方法里的一样 


            //处理url
            var result = await _consentService.ProcessConsent(viewModel); //await 有啥问题？
            if(result.IsRedirect)//当url存在时，IsRedirect有啥妙用，
            {
                return Redirect(result.RedirectUrl);
            }

            //处理错误信息，传递到view中
            if (!string.IsNullOrEmpty(result.ValidationError))
            {
                ModelState.AddModelError("", result.ValidationError);//key对应 对应的项
            }

            //处理url不存在,此时要返回view（model），但此时的viewmodel不是 get方法的model
            return View(result.ViewModel);//我们的记录会保存下来，供下次使用
        }
    }
}