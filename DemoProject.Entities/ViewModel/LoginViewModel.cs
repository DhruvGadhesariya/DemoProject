using System.ComponentModel.DataAnnotations;

namespace DemoProject.Entities.ViewModel
{
    public class LoginViewModel
    {
        [Required]
        [DataType(DataType.EmailAddress)]
        [RegularExpression("^[a-z0-9_\\+-]+(\\.[a-z0-9_\\+-]+)*@[a-z0-9-]+(\\.[a-z0-9]+)*\\.([a-z]{2,4})$", ErrorMessage = "Invalid email format.")]
        public string Email { get; set; } = null!;

        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Password is required")]
        //[RegularExpression(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*[!@#$%^&*()_+])[A-Za-z\d!@#$%^&*()_+]{8,100}$", ErrorMessage = "Password not In-format!!")]
        public string Password { get; set; } = null!;
    }
}
