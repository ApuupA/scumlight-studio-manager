using System.ComponentModel.DataAnnotations;

namespace StudioManager.DataLayer.DTO.Auth
{
    /// <summary>
    /// Changes the password of the logged in user. The current password is required as confirmation.
    /// </summary>
    public class ChangePasswordDto
    {
        [Required]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required, MinLength(8), MaxLength(200)]
        public string NewPassword { get; set; } = string.Empty;
    }
}
