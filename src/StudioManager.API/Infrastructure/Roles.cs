using StudioManager.DataLayer.Models.UserModels;

namespace StudioManager.API.Infrastructure
{
    /// <summary>
    /// Role names for [Authorize(Roles = ...)] - the same text as the enum UserRole (it is the role claim of the login cookie).
    /// </summary>
    public static class Roles
    {
        /// <summary>The studio owner: everything, also finance, settings and logins.</summary>
        public const string Owner = nameof(UserRole.Owner);
    }
}
