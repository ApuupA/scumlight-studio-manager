using StudioManager.DataLayer.Models.UserModels;

namespace StudioManager.DataLayer.DTO.Auth
{
    /// <summary>
    /// Changes the role of another login (settings, only the owner).
    /// </summary>
    public class SetRoleDto
    {
        public UserRole Role { get; set; }
    }
}
