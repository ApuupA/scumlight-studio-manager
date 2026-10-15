using AutoMapper;
using Microsoft.AspNetCore.Identity;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.UserModels;
using StudioManager.RepositoryLayer.IRepository;
using StudioManager.ServiceLayer.IService.IUserServices;

namespace StudioManager.ServiceLayer.Service.UserServices
{
    public class UserService : IUserService
    {
        private readonly IRepository<User> _repository;
        private readonly IMapper _mapper;

        /// <summary>
        /// ASP.NET Core's standard password hashing (PBKDF2 with salt). Only the hasher is used, not the full Identity system.
        /// </summary>
        private readonly PasswordHasher<User> _passwordHasher = new();

        public UserService(IRepository<User> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<UserReadDto?> Get(int id)
        {
            var user = await _repository.GetAsync(id);
            return user == null ? null : _mapper.Map<UserReadDto>(user);
        }

        public async Task<UserReadDto?> ValidateCredentials(string email, string password)
        {
            var normalizedEmail = NormalizeEmail(email);
            var user = await _repository.GetAsync(u => u.Email == normalizedEmail);

            if (user == null)
            {
                return null;
            }

            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
            return result == PasswordVerificationResult.Failed ? null : _mapper.Map<UserReadDto>(user);
        }

        public async Task EnsureOwnerExists(string email, string password, string displayName)
        {
            if (await _repository.ExistsAsync(u => true))
            {
                return;
            }

            var owner = new User
            {
                Email = NormalizeEmail(email),
                DisplayName = displayName,
                Role = UserRole.Owner
            };
            owner.PasswordHash = _passwordHasher.HashPassword(owner, password);

            await _repository.AddAsync(owner);
        }

        private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
    }
}
