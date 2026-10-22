using StudioManager.DataLayer.Models.UserModels;
using System.ComponentModel.DataAnnotations;

namespace StudioManager.DataLayer.DTO.Auth
{
    /// <summary>
    /// A new login, created by the owner in the settings. The new user changes the start password in the profile.
    /// </summary>
    public class UserCreateDto
    {
        [Required, MaxLength(100)]
        public string DisplayName { get; set; } = string.Empty;

        [Required, EmailAddress, MaxLength(200)]
        public string Email { get; set; } = string.Empty;

        [Required, MinLength(8), MaxLength(200)]
        public string Password { get; set; } = string.Empty;

        /// <summary>Owner (everything) or Assistant (no finance, no settings).</summary>
        public UserRole Role { get; set; } = UserRole.Assistant;
    }
}
