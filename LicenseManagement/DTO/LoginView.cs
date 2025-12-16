using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace LicenseManagement.DTO
{
    public class LoginView
    {
        

        public List<UserDto> Users { get; set; } = new();
        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }

        [Required(ErrorMessage = "Username is required")]
        [StringLength(100, MinimumLength = 3)]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;
    }
}
