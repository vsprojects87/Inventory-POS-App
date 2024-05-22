using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace InventoryPOS.Security
{
	public class CanEditOnlyOtherAdminRolesAndClaimsHandler : 
		AuthorizationHandler<ManageAdminRolesAndClaimsRequirememt>
	{
		protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
			ManageAdminRolesAndClaimsRequirememt requirement)
		{
			var authFilterContext = context.Resource as AuthorizationFilterContext;
			if(authFilterContext != null)
			{
				return Task.CompletedTask;
			}

			string loggedinAdminId =
				context.User.Claims.FirstOrDefault(c=>c.Type == ClaimTypes.NameIdentifier).Value;

			string adminIdBeingEdited = authFilterContext.HttpContext.Request.Query["UserId"];

			if(context.User.IsInRole("Admin") && 
				context.User.HasClaim(claim=> claim.Type == "Edit Role" && claim.Value == "true")
				&& adminIdBeingEdited.ToLower() != loggedinAdminId.ToLower()) 
			{
				context.Succeed(requirement);
			}
			return Task.CompletedTask;

		}
	}
}
