using System.ComponentModel.DataAnnotations;


namespace blogsite.ViewModels
{
    public class LoginViewModel
    {
        public string Email { get; set; } = string.Empty; // Varsayılan değer veya nullable
        public string Password { get; set; } = string.Empty;
        public bool RememberMe { get; set; }
    }
}
