using StudioManager.DataLayer.Models.UserModels;

namespace StudioManager.DataLayer.DTO.Read
{
    public class UserReadDto
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public UserRole Role { get; set; }
    }
}
