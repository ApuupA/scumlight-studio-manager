using StudioManager.DataLayer.Context;
using StudioManager.DataLayer.DTO.Auth;
using StudioManager.DataLayer.Models.UserModels;
using StudioManager.RepositoryLayer.Repository;
using StudioManager.ServiceLayer.Exceptions;
using StudioManager.ServiceLayer.Service.UserServices;

namespace StudioManager.Tests.Unit
{
    public class UserServiceTests
    {
        private readonly AppDbContext _context = TestHelper.CreateDbContext();
        private readonly UserService _service;

        public UserServiceTests()
        {
            _service = new UserService(new Repository<User>(_context), TestHelper.CreateMapper());
        }

        private async Task<User> CreateOwner()
        {
            await _service.EnsureOwnerExists("owner@studio.local", "secret", "Owner");
            return _context.Users.Single();
        }

        [Fact]
        public async Task EnsureOwnerExists_ShouldCreateOwnerOnlyOnce()
        {
            await _service.EnsureOwnerExists("Owner@Studio.local", "secret", "Owner");
            await _service.EnsureOwnerExists("other@studio.local", "secret", "Other");

            var user = Assert.Single(_context.Users);
            Assert.Equal("owner@studio.local", user.Email);
            Assert.Equal(UserRole.Owner, user.Role);
            Assert.NotEqual("secret", user.PasswordHash);
        }

        [Theory]
        [InlineData("owner@studio.local", "secret", true)]
        [InlineData("  OWNER@studio.local ", "secret", true)]
        [InlineData("owner@studio.local", "wrong", false)]
        [InlineData("unknown@studio.local", "secret", false)]
        public async Task ValidateCredentials_ShouldOnlyAcceptCorrectPassword(string email, string password, bool expectedValid)
        {
            await CreateOwner();

            var result = await _service.ValidateCredentials(email, password);

            Assert.Equal(expectedValid, result != null);
        }

        [Fact]
        public async Task ChangePassword_ShouldReplacePassword()
        {
            var owner = await CreateOwner();

            await _service.ChangePassword(owner.Id, new ChangePasswordDto { CurrentPassword = "secret", NewPassword = "new-secret-123" });

            Assert.Null(await _service.ValidateCredentials("owner@studio.local", "secret"));
            Assert.NotNull(await _service.ValidateCredentials("owner@studio.local", "new-secret-123"));
        }

        [Fact]
        public async Task ChangePassword_ShouldFail_WhenCurrentPasswordIsWrong()
        {
            var owner = await CreateOwner();

            var exception = await Assert.ThrowsAsync<BusinessRuleException>(() =>
                _service.ChangePassword(owner.Id, new ChangePasswordDto { CurrentPassword = "wrong", NewPassword = "new-secret-123" }));

            Assert.Equal("user.wrongPassword", exception.ErrorCode);
            Assert.NotNull(await _service.ValidateCredentials("owner@studio.local", "secret"));
        }

        [Fact]
        public async Task UpdateProfile_ShouldNormalizeEmailAndKeepPassword()
        {
            var owner = await CreateOwner();

            var result = await _service.UpdateProfile(owner.Id, new UpdateProfileDto { DisplayName = " Scumlight ", Email = " Me@Scumlight.de " });

            Assert.Equal("Scumlight", result.DisplayName);
            Assert.Equal("me@scumlight.de", result.Email);
            Assert.NotNull(await _service.ValidateCredentials("me@scumlight.de", "secret"));
        }

        [Fact]
        public async Task UpdateProfile_ShouldFail_WhenEmailBelongsToAnotherUser()
        {
            var owner = await CreateOwner();
            _context.Users.Add(new User { Email = "band@example.com", DisplayName = "Band", Role = UserRole.Client });
            await _context.SaveChangesAsync();

            var exception = await Assert.ThrowsAsync<BusinessRuleException>(() =>
                _service.UpdateProfile(owner.Id, new UpdateProfileDto { DisplayName = "Owner", Email = "BAND@example.com" }));

            Assert.Equal("user.emailExists", exception.ErrorCode);
        }
    }
}
