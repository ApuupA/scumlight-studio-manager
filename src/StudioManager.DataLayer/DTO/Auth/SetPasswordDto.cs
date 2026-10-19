using System.ComponentModel.DataAnnotations;

namespace StudioManager.DataLayer.DTO.Auth
{
    /// <summary>
    /// Sets a new password for another user, e.g. when it was forgotten. No current password needed.
    /// </summary>
    public class SetPasswordDto
    {
        [Required, MinLength(8), MaxLength(200)]
        public string NewPassword { get; set; } = string.Empty;
    }
}
