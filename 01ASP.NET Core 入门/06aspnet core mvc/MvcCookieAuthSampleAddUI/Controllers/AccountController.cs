﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;


using Microsoft.AspNetCore.Authentication; //对应啥  SignInAsync
using Microsoft.AspNetCore.Authentication.Cookies;//对应啥 CookieAuthenticationDefaults.AuthenticationScheme
using Microsoft.AspNetCore.Authorization;// 对应啥  [Authorize]
using System.Security.Claims;
using MvcCookieAuthSampleAddUI.ViewModels;
using Microsoft.AspNetCore.Identity; //对应UserManager SigninManager
using MvcCookieAuthSampleAddUI.Data;
using MvcCookieAuthSampleAddUI.Models;
using MvcCookieAuthSampleAddUI.Controllers;

namespace MvcCookieAuthSample.Controllers
{
    public class AccountController : Controller
    {
        private UserManager<ApplicationUser> _userManager;//与User的管理有关, 增删改查
        private SignInManager<ApplicationUser> _signInManager;//与登录的管理有关,


        //ReturnUrl 需要做判断, 存在多个地方,所以 抽象个私有方法, IActionResult 可以多次传递,
        //但是,但看到多个传递时,自己表示很迷茫,看不明白, 这说明了什么?
        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))//是否本地url的判断
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction(nameof(HomeController.Index), "home");
        }

        //为什么要抽象出方法.
        private void AddErrors(IdentityResult identityResult)
        {
            foreach (var error in identityResult.Errors)
            {
                //ModelState在ControllerBase中定义的
                ModelState.AddModelError(string.Empty, error.Description);//错误也是键值对,比如给某个字段添加错误信息.
            }
        }

        public AccountController(UserManager<ApplicationUser> userManager,SignInManager<ApplicationUser> signInManager)//通过依赖注入,获取实例
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        
        public IActionResult Register(string returnUrl =null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel viewModel, string returnUrl = null)
        {

            if (ModelState.IsValid) //model验证成功,此处是EF绑定的验证通过,内部处理还可能有错误
            {

                //有了viewmodel我们就可以创建我们的用户
                var identityUser = new ApplicationUser
                {
                    Email = viewModel.Email,
                    UserName = viewModel.Email,
                    NormalizedUserName = viewModel.Email
                };

                //因为CreateAsync为异步方法,所有要用await来接收, 所有Register也要修改为异步方法
                var identityResult = await _userManager.CreateAsync(identityUser, viewModel.Password);
                if (identityResult.Succeeded)//创建成功
                {
                    //_signInManager.SignInAsync是这个HttpContext.SignInAsync()方法的封装
                    await _signInManager.SignInAsync(identityUser, new AuthenticationProperties { IsPersistent = true });
                    //return RedirectToAction("Index", "Home");
                    return RedirectToLocal(returnUrl);//ReturnUrl 需要做判断, 存在多个地方,所以 抽象个私有方法, IActionResult 可以多次传递
                }
                else//创建失败,此处是不符合Identity的密码规则.
                {
                    AddErrors(identityResult);
                }

            }

            return View();//model验证不成功,,modelstate中的错误信息显示在view中
        }
        public IActionResult Login(string returnUrl=null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel viewModel,string returnUrl=null)
        {
            if (ModelState.IsValid) //model验证成功 此处是EF绑定的验证通过,内部处理还可能有错误
            {
                //校验用户是否合法
                var user = await _userManager.FindByEmailAsync(viewModel.Email);
                if (user == null)//用户不合法
                {


                }

                //用户合法,
                //登录
                await _signInManager.SignInAsync(user, new AuthenticationProperties { IsPersistent = true });
                //return RedirectToAction("Index", "Home");
                return RedirectToLocal(returnUrl);//ReturnUrl 需要做判断, 存在多个地方,所以 抽象个私有方法, IActionResult 可以多次传递
            }
            return View();//model验证不成功,,modelstate中的错误信息显示在view中
        }

        public IActionResult MakeLogin()
        {
            var claims = new List<Claim> {
                new Claim(ClaimTypes.Name,"jesse"),
                new Claim(ClaimTypes.Role,"admin")
            };

            var claimIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);//用户的一个身份,
            HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimIdentity));


            //return View();
            return Ok();//它会变成一个webapi
        }

        //[HttpPost]
        public IActionResult LogOut()
        {
            //HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            _signInManager.SignOutAsync();
            //return View();
            //return Ok();
            return RedirectToAction("Index", "Home");
        }
    }
}