
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using IdentityServer4.Services;
using IdentityServer4.Stores;
using IdentityServer4.Models;//对应AuthorizationRequest request,Client client,Resources resources
using MvcCookieAuthSampleAddUI.ViewModels;

namespace MvcCookieAuthSampleAddUI.Services
{
    public class ConsentService
    {
        private readonly IClientStore _clientStore;//实例在哪里赋值呢，构造函数里，
        private readonly IResourceStore _resourceStore;
        private readonly IIdentityServerInteractionService _iIdentityServerInteractionService;

        //构造函数的参数，通过依赖注入传值，  就构造函数被调用时，由mvc调用依赖注入容器，获取实例传入
        public ConsentService(//优化后，修改构造器名称，此时的依赖注入，要手工添加，类似控制器的依赖注入，mvc帮我们做了。   在startup中添加services.AddScoped<ConsentService>();
            IClientStore clientStore,
            IResourceStore resourceStore,
            IIdentityServerInteractionService iIdentityServerInteractionService)
        {
            _clientStore = clientStore;
            _resourceStore = resourceStore;
            _iIdentityServerInteractionService = iIdentityServerInteractionService;
        }





        #region Private Methods
        private ConsentViewModel CreateConsentViewModel(AuthorizationRequest request, Client client, Resources resources,InputConsetViewModel model
       )//Resources 与Resource的区别
        {
            //获取选中的项，如果为null的话，就初始化一个空的IEnumerable<string> 也可以new一个string 的list
            var selectedScopes = model?.ScopesConsented ?? Enumerable.Empty<string>();
            var RemeberConsent= model?.RemeberConsent??true; //model? 如果model有值的话，就执行model.RemeberConsent.  默认勾选此选项

             var vm = new ConsentViewModel();
            vm.ClientName = client.ClientName;
            vm.ClientLogoUrl = client.LogoUri;
            vm.ClientUrl = client.ClientUri;
            //vm.AllowRememberConsent = client.AllowRememberConsent;//这些都是从请求的url中传过来的，  如果为true时，仅需第一次点授权，之后都是自动授权  那同理自动登录咋实现的呢？
            //vm.RemeberConsent = client.AllowRememberConsent;//这些都是从请求的url中传过来的，  如果为true时，仅需第一次点授权，之后都是自动授权  那同理自动登录咋实现的呢？
            vm.RemeberConsent = model.RemeberConsent;//这些都是从请求的url中传过来的，  如果为true时，仅需第一次点授权，之后都是自动授权  那同


            vm.IdentityScopes = resources.IdentityResources.Select(i => CreateScopeViewModel(i,selectedScopes.Contains(i.Name)||model==null));//遍历所有元素，对每一元素施加function，当然func可以操作 元素，也可以不操作元素，但遍历必须进行的

            vm.ResourceScopes = resources.ApiResources.SelectMany(i => i.Scopes).Select(x => CreateScopeViewModel(x,selectedScopes.Contains(x.Name)||model==null)); //第一次进入index的get页面时，InputConsetViewModel为null，此时默认全选所有选项
            //select list<list<T>>
            //SelectMany  list < T >

            return vm;
        }

        private ScopeViewModel CreateScopeViewModel(IdentityResource identityresource,bool check)//处理单个IdentityResouce
        {
            return new ScopeViewModel
            {
                Name = identityresource.Name,
                DisplayName = identityresource.DisplayName,
                Description = identityresource.Description,
                Required = identityresource.Required,
                Checked = check||identityresource.Required,//当你是必须或者选中任一情况下，都设置为true
                Emphasize = identityresource.Emphasize

            };
        }

        private ScopeViewModel CreateScopeViewModel(Scope scope,bool check)//处理单个Scope
        {
            return new ScopeViewModel
            {
                Name = scope.Name,
                DisplayName = scope.DisplayName,
                Description = scope.Description,
                Required = scope.Required,
                Checked = check||scope.Required,//当你是必须或者选中任一情况下，都设置为true
                Emphasize = scope.Emphasize

            };
        }
        #endregion



        public async Task<ConsentViewModel> BuildConsentViewModelAsync(string returnUrl,InputConsetViewModel model=null)//model默认为null，如果model有值，我们可以拿到哪些东西，可以拿到用户选中的东西
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

            //var vm = CreateConsentViewModel(request, client, resources);//返回TResult类型就可以了。
            var vm = CreateConsentViewModel(request, client, resources,model);//返回TResult类型就可以了。
            vm.ReturnUrl = returnUrl;
            return vm;
        }


        public async Task<ProcessConsentResult> ProcessConsent(InputConsetViewModel model)
        {
            ConsentResponse consentResponse = null;
            var result = new ProcessConsentResult();
            if (model.Button == "no")
            {
                consentResponse = ConsentResponse.Denied;
            }
            else if (model.Button == "yes")
            {
                if (model.ScopesConsented != null && model.ScopesConsented.Any())
                {
                    consentResponse = new ConsentResponse
                    {
                        RememberConsent = model.RemeberConsent,
                        ScopesConsented = model.ScopesConsented,
                    };
                }
                //如果一项都没有选的情况下，给你一些错误信息,当然我们这里不会出现这种情况，因为默认必选opid
                result.ValidationError = "请至少选中一个权限";
            }



            if (consentResponse != null)
            {
                var request = await _iIdentityServerInteractionService.GetAuthorizationContextAsync(model.ReturnUrl);
                await _iIdentityServerInteractionService.GrantConsentAsync(request, consentResponse);//inform identityserver user's consent


                //return Redirect(viewModel.ReturnUrl);//少了return
                result.RedirectUrl = model.ReturnUrl;
            }
            else//consentResponse == null的情况下封装一个viewmodel
            {
                //var consentViewModel = await BuildConsentViewModelAsync(model.ReturnUrl);
                var consentViewModel = await BuildConsentViewModelAsync(model.ReturnUrl, model);
                result.ViewModel = consentViewModel;
            }

            return result;
        }
    }
}
