using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;//对应 Controller

using MvcCookieAuthSampleAddUI.ViewModels;

using IdentityServer4.Services;
using IdentityServer4.Stores;
using IdentityServer4.Models;//对应AuthorizationRequest request,Client client,Resources resources

namespace MvcCookieAuthSampleAddUI.Controllers
{
    public class ConsentController : Controller
    {
        private readonly IClientStore _clientStore;//实例在哪里赋值呢，构造函数里，
        private readonly IResourceStore _resourceStore;
        private readonly IIdentityServerInteractionService _iIdentityServerInteractionService;

        //构造函数的参数，通过依赖注入传值，  就构造函数被调用时，由mvc调用依赖注入容器，获取实例传入
        public ConsentController(
            IClientStore clientStore,
            IResourceStore resourceStore,
            IIdentityServerInteractionService iIdentityServerInteractionService)
        {
            _clientStore = clientStore;
            _resourceStore = resourceStore;
            _iIdentityServerInteractionService = iIdentityServerInteractionService;
        }



        private async Task<ConsentViewModel> BuildConsentViewModel(string returnUrl)
        {
            //step1 获取一个request
            //异步方法，必须要await调用， 不然返回值的属性，获取不到
            var request = await _iIdentityServerInteractionService.GetAuthorizationContextAsync(returnUrl);//类库方法需要一个参数，从那里来呢
            if (request == null)//逻辑严谨，时刻判断空值，防止bug
                return null;
            //step2 request获取到后，获取client 和resource
            var client = await _clientStore.FindEnabledClientByIdAsync(request.ClientId);//异步方法，必须要await调用， 不然返回值的属性，获取不到，因为返回的是一个Task
            //需要判断client是否为空


            var resources = await _resourceStore.FindEnabledResourcesByScopeAsync(request.ScopesRequested);//此Resource包括了IdentityResource和APIResource

            var vm= CreateConsentViewModel(request, client, resources);//返回TResult类型就可以了。
            vm.ReturnUrl = returnUrl;
            return vm;
        }

        private ConsentViewModel CreateConsentViewModel(AuthorizationRequest request,Client client,Resources resources
            )//Resources 与Resource的区别
        {
            var vm = new ConsentViewModel();
            vm.ClientName = client.ClientName;
            vm.ClientLogoUrl = client.LogoUri;
            vm.ClientUrl = client.ClientUri;
            vm.AllowRememberConsent = client.AllowRememberConsent;//这些都是从请求的url中传过来的，  如果为true时，仅需第一次点授权，之后都是自动授权  那同理自动登录咋实现的呢？


            vm.IdentityScopes = resources.IdentityResources.Select(i => CreateScopeViewModel(i));//遍历所有元素，对每一元素施加function，当然func可以操作 元素，也可以不操作元素，但遍历必须进行的

            vm.ResourceScopes = resources.ApiResources.SelectMany(i => i.Scopes).Select(x => CreateScopeViewModel(x));
            //select list<list<T>>
            //SelectMany  list < T >

            return vm;
        }

        private ScopeViewModel CreateScopeViewModel(IdentityResource identityresource)//处理单个IdentityResouce
        {
            return new ScopeViewModel
            {
                Name = identityresource.Name,
                DisplayName = identityresource.DisplayName,
                Description = identityresource.Description,
                Required = identityresource.Required,
                Checked = identityresource.Required,
                Emphasize = identityresource.Emphasize

            };
        }

        private ScopeViewModel CreateScopeViewModel(Scope scope)//处理单个Scope
        {
            return new ScopeViewModel
            {
                Name = scope.Name,
                DisplayName = scope.DisplayName,
                Description = scope.Description,
                Required = scope.Required,
                Checked = scope.Required,
                Emphasize = scope.Emphasize

            };
        }

        [HttpGet]
        public async  Task<IActionResult> Index(string returnUrl)
        {
            //逻辑清晰，就生成model
            var model = await BuildConsentViewModel(returnUrl);//专人做专事

           if(model==null)
            {

            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Index(InputConsetViewModel viewModel)
        {
            return View();
        }
    }
}