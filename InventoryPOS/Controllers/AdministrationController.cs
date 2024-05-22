using InventoryPOS.Models;
using InventoryPOS.Models.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;

namespace InventoryPOS.Controllers
{
	//[Authorize(Roles = "Admin")]
	// single role authorization
	//[Authorize(Roles = "Admin,User")]
	//we can assign authorization to two roles as well

	//[Authorize(Roles = "Admin")]
	//[Authorize(Roles = "User")]
	// if we mention like this then the user need to have both roles as admin and user
	//having admin wont give access to this page

	[Authorize(Policy = ("AdminRolePolicy"))]
	public class AdministrationController : Controller
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
		public IActionResult ListUsers()
		{
			var users = userManager.Users;
			return View(users);
		}

		[HttpGet]
		public async Task<IActionResult> EditUser(string id)
		{
			var user = await userManager.FindByIdAsync(id);
			if (user == null)
			{
				ViewBag.ErrorMessage = $"User with Id={id} cannot be found";
				return View("NotFound");
			}

			var userClaims = await userManager.GetClaimsAsync(user);
			var userRoles = await userManager.GetRolesAsync(user);

			var model = new EditUserViewModel
			{
				Id = user.Id,
				Email = user.Email,
				UserName = user.UserName,
				Address = user.Address,
				Claims = userClaims.Select(c => c.Value).ToList(),
				Roles = userRoles
			};
			return View(model);
		}


		[HttpPost]
		public async Task<IActionResult> EditUser(EditUserViewModel model)
		{
			var user = await userManager.FindByIdAsync(model.Id);
			if (user == null)
			{
				ViewBag.ErrorMessage = $"User with Id={model.Id} cannot be found";
				return View("NotFound");
			}
			else
			{
				user.Email = model.Email;
				user.UserName = model.UserName;
				user.Address = model.Address;
				var result = await userManager.UpdateAsync(user);
				if (result.Succeeded)
				{
					return RedirectToAction("ListUsers");
				}
				foreach (var error in result.Errors)
				{
					ModelState.AddModelError("", error.Description);
				}
			}
			return View(model);
		}

		public async Task<IActionResult> DeleteUser(string id)
		{
			var user = await userManager.FindByIdAsync(id);
			if (user == null)
			{
				ViewBag.ErrorMessage = $"User with Id={id} cannot be found";
				return View("NotFound");
			}
			else
			{
				var result = await userManager.DeleteAsync(user);

				if (result.Succeeded)
				{
					return RedirectToAction("ListUsers");
				}
				foreach (var error in result.Errors)
				{
					ModelState.AddModelError("", error.Description);
				}
				return View("ListUsers");
			}
		}

		[HttpGet]
		[Authorize(Policy = "EditRolePolicy")]
		public async Task<IActionResult> ManageUserRoles(string userId)
		{
			ViewBag.userId = userId;
			var user = await userManager.FindByIdAsync(userId);
			if (user == null)
			{
				ViewBag.ErrorMessage = $"User with Id={userId} cannot be found";
				return View("NotFound");
			}

			//var model = new List<UserRolesViewModel>();

			//foreach (var role in roleManager.Roles)
			//{


			//	var userRolesViewModel = new UserRolesViewModel()
			//	{
			//		RoleId = role.Id,
			//		RoleName = role.Name,
			//	};
			//	var userSelect = await userManager.GetUsersInRoleAsync(role.Name);
			//	if (userSelect.Contains(user))
			//	{
			//		userRolesViewModel.IsSelected = true;
			//	}
			//	else
			//	{
			//		userRolesViewModel.IsSelected = false;
			//	}

			//	model.Add(userRolesViewModel);
			//}
			var roles = await roleManager.Roles.ToListAsync();

			var model = roles.Select(role => new UserRolesViewModel
			{
				RoleId = role.Id,
				RoleName = role.Name,
				IsSelected = userManager.IsInRoleAsync(user, role.Name).Result // Blocking call, but it's safe here
			}).ToList();

			return View(model);
		}
		// like roles where we can add or remove user from role we can manage which
		// role to add or remove for user


		[HttpPost]
		[Authorize(Policy = "EditRolePolicy")]
		public async Task<IActionResult> ManageUserRoles(List<UserRolesViewModel> model,string userId)
		{
			var user = await userManager.FindByIdAsync(userId);
			if (user == null)
			{
				ViewBag.ErrorMessage = $"User with Id={userId} cannot be found";
				return View("NotFound");
			}

			var roles = await userManager.GetRolesAsync(user);
			var result = await userManager.RemoveFromRolesAsync(user, roles);

			if (!result.Succeeded)
			{
				ModelState.AddModelError("", "Cannot remoce user existing roles");
				return View(model);

			}

			result = await userManager.AddToRolesAsync(user,
				model.Where(x=>x.IsSelected).Select(y=> y.RoleName));
			
			if (!result.Succeeded)
			{
				ModelState.AddModelError("", "Cannot add selected roles to user");
				return View(model);
			}
			return RedirectToAction("EditUser", new { Id = userId });
		}


		[HttpGet]
		public async Task<IActionResult> ManageUserClaims(string userId)
		{
			var user = await userManager.FindByIdAsync(userId);
			if (user == null)
			{
				ViewBag.ErrorMessage = $"User with Id={userId} cannot be found";
				return View("NotFound");
			}

			var existingUserClaims = await userManager.GetClaimsAsync(user);

			var model = new UserClaimsViewModel
			{
				UserId = userId
			};

			foreach(Claim claim in ClaimsStore.AllClaims)
			{
				UserClaim userClaim = new UserClaim
				{
					ClaimType = claim.Type
				};
				if (existingUserClaims.Any(c => c.Type == claim.Type))
				{
					userClaim.IsSelected = true;
				}
				model.Claims.Add(userClaim);
			}
			return View(model);
		}


		[HttpPost]
		public async Task<IActionResult> ManageUserClaims(UserClaimsViewModel model, string userId)
		{
			var user = await userManager.FindByIdAsync(userId);
			if (user == null)
			{
				ViewBag.ErrorMessage = $"User with Id={userId} cannot be found";
				return View("NotFound");
			}

			var claims = await userManager.GetClaimsAsync(user);
			var result = await userManager.RemoveClaimsAsync(user, claims);

			if (!result.Succeeded)
			{
				ModelState.AddModelError("","Cannot remove user existing claims");
				return View(model);
			}
			result = await userManager.AddClaimsAsync(user,
				model.Claims.Where(c => c.IsSelected).Select(c => new Claim(c.ClaimType, c.ClaimType)));

			if (!result.Succeeded)
			{
				ModelState.AddModelError("", "Cannot add selected claims to user");
				return View(model);
			}

			return RedirectToAction("EditUser", new {Id = model.UserId });
		}



		// >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
		// for the role
		[HttpGet]
		public IActionResult CreateRole()
		{
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> CreateRole(CreateRoleViewModel model)
		{
			if (ModelState.IsValid)
			{
				IdentityRole identityRole = new IdentityRole()
				{
					Name = model.RoleName
				};
				IdentityResult result = await roleManager.CreateAsync(identityRole);
				if (result.Succeeded)
				{
					return RedirectToAction("index", "home");
				}
				foreach (IdentityError error in result.Errors)
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

		[Authorize(Policy = "DeleteRolePolicy")]
		// we have registered policy in program.cs
		public async Task<IActionResult> DeleteRole(string id)
		{
			var role = await roleManager.FindByIdAsync(id);
			if (role == null)
			{
				ViewBag.ErrorMessage = $"Role with Id={id} cannot be found";
				return View("NotFound");
			}
			else
			{
				var result = await roleManager.DeleteAsync(role);

				if (result.Succeeded)
				{
					return RedirectToAction("ListRoles");
				}
				foreach (var error in result.Errors)
				{
					ModelState.AddModelError("", error.Description);
				}
				return View("ListRoles");
			}
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

				foreach (var error in result.Errors)
				{
					ModelState.AddModelError("", error.Description);
				}
			}
			return View(model);
		}

		[HttpGet]
		//[Authorize(Roles = "Admin")]
		// we can set up role for specific action, to give access or restrict it
		//[AllowAnonymous] this will give access to anyone
		public async Task<IActionResult> EditUsersInRole(string roleId)
		{
			ViewBag.roleId = roleId;

			var role = await roleManager.FindByIdAsync(roleId);
			if (role == null)
			{
				ViewBag.ErrorMessage = $"Role with Id={roleId} cannot be found";
				return View($"NotFound");
			}

			//var model = new List<UserRoleViewModel>();
			//foreach (var user in userManager.Users)
			//{
			//	var userRoleViewModel = new UserRoleViewModel
			//	{
			//		UserId = user.Id,
			//		UserName = user.UserName
			//	};
			//	if (await userManager.IsInRoleAsync(user, role.Name))
			//	{
			//		userRoleViewModel.IsSelected = true;
			//	}
			//	else
			//	{
			//		userRoleViewModel.IsSelected = false;
			//	}
			//	model.Add(userRoleViewModel);
			//}

			var usersInRole = await userManager.GetUsersInRoleAsync(role.Name);

			var model = new List<UserRoleViewModel>();

			foreach (var user in userManager.Users)
			{
				var userRoleViewModel = new UserRoleViewModel
				{
					UserId = user.Id,
					UserName = user.UserName,
					IsSelected = usersInRole.Contains(user)
				};

				model.Add(userRoleViewModel);
			}

			return View(model);
		}



		[HttpPost]
		public async Task<IActionResult> EditUsersInRole(List<UserRoleViewModel> model, string roleId)
		{
			ViewBag.roleId = roleId;

			var role = await roleManager.FindByIdAsync(roleId);
			if (role == null)
			{
				ViewBag.ErrorMessage = $"Role with Id={roleId} cannot be found";
				return View($"NotFound");
			}

			for (int i = 0; i < model.Count; i++)
			{
				var user = await userManager.FindByIdAsync(model[i].UserId);

				IdentityResult result = null;
				if (model[i].IsSelected && !(await userManager.IsInRoleAsync(user, role.Name)))
				{
					result = await userManager.AddToRoleAsync(user, role.Name);
				}
				else if (!model[i].IsSelected && await userManager.IsInRoleAsync(user, role.Name))
				{
					result = await userManager.RemoveFromRoleAsync(user, role.Name);
				}
				else
				{
					continue;
				}
				// we are checking if user is checked and already doesnt exist in roles then we
				// assign roles otherwise if uncheck then we remove roles
				// if not both then we just skip the current user and check next user in list

				if (result.Succeeded)
				{
					if (i < (model.Count - 1))
					{
						continue;
						// here if i is less than total number of model count then we still have
						// records which is nothing but more users so we will continue
					}
					else
					{
						return RedirectToAction("EditRole", new { Id = roleId });
						// if we have already reach the end of loop then we will exit and
						// we will go back to editrole page we began and will send back the id
						// of that role
					}
				}
			}

			return RedirectToAction("EditRole", new { Id = roleId });
			//in case if we dont have any users then we will redirect to editrole
		}


	}
}
