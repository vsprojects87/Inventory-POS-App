using System.ComponentModel.DataAnnotations;

namespace InventoryPOS.Models.ViewModel
{
    public class RegisterVM
    {
        [Required(ErrorMessage = "Username is required")]
        public string Name { get; set; }

        public string Email { get; set; }

        [Required(ErrorMessage = "Passowrd is Required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Compare("Password",ErrorMessage ="Password dont match")]
        public string ConfirmPassword { get; set; }

        public string Address { get; set; }
    }
}
