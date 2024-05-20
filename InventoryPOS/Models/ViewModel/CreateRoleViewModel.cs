using System.ComponentModel.DataAnnotations;

namespace InventoryPOS.Models.ViewModel
{
	public class CreateRoleViewModel
	{
		[Required]
		public string RoleName { get; set;}
	}
}
