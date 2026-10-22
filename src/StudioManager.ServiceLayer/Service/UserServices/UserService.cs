using AutoMapper;
using Microsoft.AspNetCore.Identity;
using StudioManager.DataLayer.DTO.Auth;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.UserModels;
using StudioManager.RepositoryLayer.IRepository;
using StudioManager.ServiceLayer.Exceptions;
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

        public async Task<IEnumerable<UserReadDto>> GetAll()
        {
            var users = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<UserReadDto>>(users.OrderBy(u => u.DisplayName));
        }

        public async Task<UserReadDto> Create(UserCreateDto dto)
        {
            var normalizedEmail = NormalizeEmail(dto.Email);
            if (await _repository.ExistsAsync(u => u.Email == normalizedEmail))
            {
                throw new BusinessRuleException("user.emailExists", "Another user already uses this email address.");
            }

            CheckRole(dto.Role);
            var user = new User
            {
                Email = normalizedEmail,
                DisplayName = dto.DisplayName.Trim(),
                Role = dto.Role
            };
            user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);

            await _repository.AddAsync(user);
            return _mapper.Map<UserReadDto>(user);
        }

        public async Task<UserReadDto> SetRole(int id, SetRoleDto dto, int currentUserId)
        {
            // So the studio never ends up without an owner: nobody changes their own role
            if (id == currentUserId)
            {
                throw new BusinessRuleException("user.cannotChangeOwnRole", "You can not change your own role.");
            }

            CheckRole(dto.Role);
            var user = await GetEntity(id);
            user.Role = dto.Role;
            await _repository.UpdateAsync(user);
            return _mapper.Map<UserReadDto>(user);
        }

        /// <summary>Logins of the studio are owners or assistants (the role "Client" is not used yet).</summary>
        private static void CheckRole(UserRole role)
        {
            if (role is not (UserRole.Owner or UserRole.Assistant))
            {
                throw new BusinessRuleException("user.invalidRole", "A login is an owner or an assistant.");
            }
        }

        public async Task SetPassword(int id, SetPasswordDto dto)
        {
            var user = await GetEntity(id);
            user.PasswordHash = _passwordHasher.HashPassword(user, dto.NewPassword);
            await _repository.UpdateAsync(user);
        }

        public async Task Delete(int id, int currentUserId)
        {
            if (id == currentUserId)
            {
                throw new BusinessRuleException("user.cannotDeleteSelf", "You can not delete your own login.");
            }

            await GetEntity(id);
            await _repository.DeleteAsync(id);
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

        public async Task<UserReadDto> UpdateProfile(int id, UpdateProfileDto dto)
        {
            var user = await GetEntity(id);
            var normalizedEmail = NormalizeEmail(dto.Email);

            if (await _repository.ExistsAsync(u => u.Email == normalizedEmail && u.Id != id))
            {
                throw new BusinessRuleException("user.emailExists", "Another user already uses this email address.");
            }

            user.DisplayName = dto.DisplayName.Trim();
            user.Email = normalizedEmail;
            await _repository.UpdateAsync(user);

            return _mapper.Map<UserReadDto>(user);
        }

        public async Task ChangePassword(int id, ChangePasswordDto dto)
        {
            var user = await GetEntity(id);

            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, dto.CurrentPassword);
            if (result == PasswordVerificationResult.Failed)
            {
                throw new BusinessRuleException("user.wrongPassword", "The current password is wrong.");
            }

            user.PasswordHash = _passwordHasher.HashPassword(user, dto.NewPassword);
            await _repository.UpdateAsync(user);
        }

        private async Task<User> GetEntity(int id)
        {
            return await _repository.GetAsync(id) ?? throw new EntityNotFoundException(nameof(User), id);
        }

        private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
    }
}
