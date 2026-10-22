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

        /// <summary>
        /// Secret of the calendar subscription link (/api/Calendar/{token}.ics). Null = no link yet.
        /// </summary>
        public string? CalendarToken { get; set; }

        /// <summary>The groups of the user - an assistant sees the projects of these groups.</summary>
        public ICollection<Group> Groups { get; set; } = new List<Group>();
    }
}
