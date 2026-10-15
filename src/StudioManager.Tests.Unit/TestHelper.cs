using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using StudioManager.DataLayer.Context;
using StudioManager.ServiceLayer.Mapper;

namespace StudioManager.Tests.Unit
{
    /// <summary>
    /// Shared setup for service tests: an in-memory database (fresh for every test) and the real AutoMapper profile.
    /// </summary>
    internal static class TestHelper
    {
        public static AppDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        public static IMapper CreateMapper()
        {
            var configuration = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>(), NullLoggerFactory.Instance);
            return configuration.CreateMapper();
        }
    }
}
