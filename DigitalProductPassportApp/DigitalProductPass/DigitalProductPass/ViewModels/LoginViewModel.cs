using System.ComponentModel.DataAnnotations;

namespace DigitalProductPass.ViewModels;

public class LoginViewModel
{
    [Required, StringLength(64)]
    public string Username { get; set; } = "";

    [Required, StringLength(128), DataType(DataType.Password)]
    public string Password { get; set; } = "";

    public bool RememberMe { get; set; }
}
