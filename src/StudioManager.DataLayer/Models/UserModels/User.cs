namespace StudioManager.DataLayer.Models.UserModels
{
    public class User : BaseEntity
    {
        public string Email { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// Hash created by ASP.NET Core's PasswordHasher - the plain password is never stored.
        /// </summary>
        public string PasswordHash { get; set; } = string.Empty;

        public UserRole Role { get; set; } = UserRole.Owner;
    }
}
