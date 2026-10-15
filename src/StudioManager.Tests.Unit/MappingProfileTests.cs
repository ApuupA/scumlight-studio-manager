using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;
using StudioManager.ServiceLayer.Mapper;

namespace StudioManager.Tests.Unit
{
    public class MappingProfileTests
    {
        [Fact]
        public void MappingProfile_ShouldBeValid()
        {
            // Fails if a DTO / entity property is neither mapped nor explicitly ignored
            var configuration = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>(), NullLoggerFactory.Instance);

            configuration.AssertConfigurationIsValid();
        }
    }
}
