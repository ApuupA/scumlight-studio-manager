using StudioManager.DataLayer.DTO.Auth;
using StudioManager.DataLayer.DTO.Read;

namespace StudioManager.ServiceLayer.IService.IUserServices
{
    public interface IUserService
    {
        Task<UserReadDto?> Get(int id);

        /// <summary>
        /// All logins, sorted by name.
        /// </summary>
        Task<IEnumerable<UserReadDto>> GetAll();

        /// <summary>
        /// Creates another login with full access. The email address must not be used yet.
        /// </summary>
        Task<UserReadDto> Create(UserCreateDto dto);

        /// <summary>
        /// Sets a new password for a user (no current password needed - only owners may do this).
        /// </summary>
        Task SetPassword(int id, SetPasswordDto dto);

        /// <summary>
        /// Deletes a login. The logged in user can not delete themselves, so there is always one login left.
        /// </summary>
        Task Delete(int id, int currentUserId);

        /// <summary>
        /// Checks email and password. Returns the user if they are correct, otherwise null.
        /// </summary>
        Task<UserReadDto?> ValidateCredentials(string email, string password);

        /// <summary>
        /// Creates the owner account on the very first start (when no user exists yet).
        /// </summary>
        Task EnsureOwnerExists(string email, string password, string displayName);

        /// <summary>
        /// Changes name and email. Throws BusinessRuleException if another user already has the email.
        /// </summary>
        Task<UserReadDto> UpdateProfile(int id, UpdateProfileDto dto);

        /// <summary>
        /// Sets a new password. Throws BusinessRuleException if the current password is wrong.
        /// </summary>
        Task ChangePassword(int id, ChangePasswordDto dto);
    }
}
