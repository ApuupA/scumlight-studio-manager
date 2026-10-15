using StudioManager.DataLayer.DTO.Read;

namespace StudioManager.ServiceLayer.IService.IUserServices
{
    public interface IUserService
    {
        Task<UserReadDto?> Get(int id);

        /// <summary>
        /// Checks email and password. Returns the user if they are correct, otherwise null.
        /// </summary>
        Task<UserReadDto?> ValidateCredentials(string email, string password);

        /// <summary>
        /// Creates the owner account on the very first start (when no user exists yet).
        /// </summary>
        Task EnsureOwnerExists(string email, string password, string displayName);
    }
}
