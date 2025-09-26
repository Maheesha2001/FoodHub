using System.ComponentModel.DataAnnotations;

namespace FoodHub.ViewModels
{
    public class RegisterVM
    { [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    [Required]
    [Compare("Password", ErrorMessage = "Passwords do not match")]
    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; }

    [Required]
    public string FullName { get; set; }
    }
}
