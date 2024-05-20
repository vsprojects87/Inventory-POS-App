﻿using InventoryPOS.Models;
using InventoryPOS.Models.ViewModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;

namespace InventoryPOS.Controllers
{
	public class AdministrationController :Controller 
	{
		private readonly RoleManager<IdentityRole> roleManager;
		private readonly UserManager<AppUser> userManager;

		public AdministrationController(RoleManager<IdentityRole> roleManager,
										UserManager<AppUser> userManager)
        {
			this.roleManager = roleManager;
			this.userManager = userManager;
		}

		[HttpGet]
		public IActionResult CreateRole()
		{
			return View();
		}

		[HttpPost]
		public async  Task<IActionResult> CreateRole(CreateRoleViewModel model)
		{
			if(ModelState.IsValid)
			{
				IdentityRole identityRole = new IdentityRole()
				{
					Name = model.RoleName
				};
				IdentityResult result = await roleManager.CreateAsync(identityRole);
				if (result.Succeeded)
				{
					return RedirectToAction("index","home");
				}
				foreach(IdentityError error in result.Errors)
				{
					ModelState.AddModelError("", error.Description);
				}
			}
			return View(model);
		}

		[HttpGet]
		public IActionResult ListRoles()
		{
			var roles = roleManager.Roles;
			return View(roles);
		}

		[HttpGet]
		public async Task<IActionResult> EditRole(string id)
		{
			var role = await roleManager.FindByIdAsync(id);
			if (role == null)
			{
				ViewBag.ErrorMessage = $"Role with Id={id} cannot be found";
				return View($"NotFound");
			}
			var model = new EditRoleViewModel
			{
				Id = role.Id,
				RoleName = role.Name
			};

			var usersInRole = await userManager.GetUsersInRoleAsync(role.Name);

			foreach (var user in usersInRole)
			{
				model.Users.Add(user.UserName);
			}
			return View(model);
		}

		[HttpPost]
		public async Task<IActionResult> EditRole(EditRoleViewModel model)
		{
			var role = await roleManager.FindByIdAsync(model.Id);
			if (role == null)
			{
				ViewBag.ErrorMessage = $"Role with Id={model.Id} cannot be found";
				return View($"NotFound");
			}
			else
			{
				role.Name = model.RoleName;
				var result = await roleManager.UpdateAsync(role);
				if (result.Succeeded)
				{
					return RedirectToAction("ListRoles");
				}

				foreach(var error in result.Errors)
				{
					ModelState.AddModelError("",error.Description);
				}
			}
			return View(model);
		}

		[HttpGet]
		public async Task<IActionResult> EditUsersInRole(string roleId)
		{
			ViewBag.roleId = roleId;

			var role = await roleManager.FindByIdAsync(roleId);
			if(role == null)
			{
				ViewBag.ErrorMessage = $"Role with Id={roleId} cannot be found";
				return View($"NotFound");
			}

			var model = new List<UserRoleViewModel>();

			foreach (var user in userManager.Users)
			{
				var userRoleViewModel = new UserRoleViewModel
				{
					UserId = user.Id,
					UserName = user.UserName
				};

				if(await userManager.IsInRoleAsync(user,role.Name))
				{
					userRoleViewModel.IsSelected = true;
				}
				else
				{
					userRoleViewModel.IsSelected = false;
				}
				model.Add(userRoleViewModel);
			}

			return View(model);

		}

	}
}