﻿using InventoryPOS.Models;
using InventoryPOS.Models.ViewModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace InventoryPOS.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<AppUser> signInManager;
		private readonly UserManager<AppUser> userManager;

		public AccountController(SignInManager<AppUser> signInManager,UserManager<AppUser> userManager)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
		public async Task<IActionResult> LoginAsync(LoginVM model)
		{
            if(ModelState.IsValid)
            {

                var result = await signInManager.PasswordSignInAsync(model.Username!, model.Password!,model.RememberMe,false);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }
                ModelState.AddModelError("","Invalid Login Attempt");
                return View(model);
            }
			return View();
		}
		public IActionResult Register()
        {
           return View();
        }

        [HttpPost]
		public async Task<IActionResult> RegisterAsync(RegisterVM model)
		{
            if (ModelState.IsValid)
            {
                AppUser user = new AppUser()
                {
                    Name = model.Name,
                    UserName = model.Email,
                    Email = model.Email,
                    Address = model.Address
                };

                var result = await userManager.CreateAsync(user,model.Password!);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index","Home");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("",error.Description);
                }
            }
			return View(model);
		}


        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Home","Index");
        }
    }
}
