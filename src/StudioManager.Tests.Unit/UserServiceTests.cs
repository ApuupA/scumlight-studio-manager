using StudioManager.DataLayer.Context;
using StudioManager.DataLayer.Models.UserModels;
using StudioManager.RepositoryLayer.Repository;
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
            await _service.EnsureOwnerExists("owner@studio.local", "secret", "Owner");

            var result = await _service.ValidateCredentials(email, password);

            Assert.Equal(expectedValid, result != null);
        }
    }
}
