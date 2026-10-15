using System.ComponentModel.DataAnnotations;

namespace StudioManager.DataLayer.DTO.Auth
{
    /// <summary>
    /// Changes name and email of the logged in user.
    /// </summary>
    public class UpdateProfileDto
    {
        [Required, MaxLength(100)]
        public string DisplayName { get; set; } = string.Empty;

        [Required, EmailAddress, MaxLength(200)]
        public string Email { get; set; } = string.Empty;
    }
}
