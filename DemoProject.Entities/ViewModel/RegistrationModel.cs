using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DemoProject.Entities.ViewModel
{
    public class RegistrationModel
    {
        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        [RegularExpression("^[a-z0-9_\\+-]+(\\.[a-z0-9_\\+-]+)*@[a-z0-9-]+(\\.[a-z0-9]+)*\\.([a-z]{2,4})$", ErrorMessage = "Invalid email format.")]
        public string Email { get; set; } = null!;

        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, ErrorMessage = "Strong Password is needed!!", MinimumLength = 6)]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*[!@#$%^&*()_+])[A-Za-z\d!@#$%^&*()_+]{8,100}$", ErrorMessage = "Strong Password is needed!!")]
        public string Password { get; set; } = null!;

        [NotMapped]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "The Password and ConfirmPassword fields do not match.")]
        public string? ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Phone number is required.")]
        [RegularExpression(@"^\+?[0-9]{1,3}-?[0-9]{3,4}-?[0-9]{6,8}$", ErrorMessage = "Invalid phone number.")]
        public long PhoneNumber { get; set; }

        public long? CountryId { get; set; }

        public long? CityId { get; set; }

        public long Otp { get; set; }

        //public string CityName { get; set; } = null!;

        //public string CountryName { get; set; } = null!;

    }
}
