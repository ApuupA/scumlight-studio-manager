using Moq;
using StudioManager.DataLayer.Context;
using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.Models.GearModels;
using StudioManager.DataLayer.Models.ProjectModels;
using StudioManager.DataLayer.Models.SessionModels;
using StudioManager.RepositoryLayer.Repository;
using StudioManager.ServiceLayer.Exceptions;
using StudioManager.ServiceLayer.IService.IFileServices;
using StudioManager.ServiceLayer.Service.GearServices;

namespace StudioManager.Tests.Unit
{
    public class GearItemServiceTests
    {
        private readonly AppDbContext _context = TestHelper.CreateDbContext();
        private readonly Mock<IFileStorageService> _mockFileStorage = new();
        private readonly GearItemService _service;
        private readonly GearCategory _category;

        public GearItemServiceTests()
        {
            _service = new GearItemService(
                new Repository<GearItem>(_context),
                new Repository<GearCategory>(_context),
                new Repository<Session>(_context),
                new Repository<InstrumentSetupItem>(_context),
                _mockFileStorage.Object,
                TestHelper.CreateMapper());

            _category = new GearCategory { Name = "Microphones" };
            _context.GearCategories.Add(_category);
            _context.SaveChanges();
        }

        private GearItemCreateDto NewDto(string name = "SM57") => new()
        {
            Name = name,
            Manufacturer = "Shure",
            GearCategoryId = _category.Id
        };

        [Fact]
        public async Task Insert_ShouldReturnItemWithCategoryName()
        {
            var result = await _service.Insert(NewDto());

            Assert.True(result.Id > 0);
            Assert.Equal("SM57", result.Name);
            Assert.Equal("Microphones", result.GearCategoryName);
            Assert.Equal(GearStatus.Available, result.Status);
            Assert.False(result.HasPhoto);
        }

        [Fact]
        public async Task Insert_ShouldFail_ForUnknownCategory()
        {
            var dto = NewDto();
            dto.GearCategoryId = 999;

            var exception = await Assert.ThrowsAsync<BusinessRuleException>(() => _service.Insert(dto));

            Assert.Equal("gearItem.categoryNotFound", exception.ErrorCode);
        }

        [Fact]
        public async Task Update_ShouldOverwriteValues()
        {
            var created = await _service.Insert(NewDto());
            var dto = NewDto("SM57 (old one)");
            dto.Status = GearStatus.InRepair;

            var result = await _service.Update(created.Id, dto);

            Assert.Equal("SM57 (old one)", result.Name);
            Assert.Equal(GearStatus.InRepair, result.Status);
        }

        [Fact]
        public async Task Update_ShouldThrowNotFound_ForUnknownId()
        {
            await Assert.ThrowsAsync<EntityNotFoundException>(() => _service.Update(999, NewDto()));
        }

        [Theory]
        [InlineData("photo.gif")]
        [InlineData("photo.pdf")]
        [InlineData("photo")]
        public async Task SetPhoto_ShouldRejectInvalidFileTypes(string fileName)
        {
            var created = await _service.Insert(NewDto());

            var exception = await Assert.ThrowsAsync<BusinessRuleException>(
                () => _service.SetPhoto(created.Id, Stream.Null, fileName, 100));

            Assert.Equal("photo.invalidType", exception.ErrorCode);
        }

        [Fact]
        public async Task SetPhoto_ShouldRejectTooLargeFiles()
        {
            var created = await _service.Insert(NewDto());

            var exception = await Assert.ThrowsAsync<BusinessRuleException>(
                () => _service.SetPhoto(created.Id, Stream.Null, "photo.jpg", GearItemService.MaxPhotoSizeBytes + 1));

            Assert.Equal("photo.tooLarge", exception.ErrorCode);
        }

        [Fact]
        public async Task SetPhoto_ShouldReplaceAndDeleteOldPhoto()
        {
            var created = await _service.Insert(NewDto());
            _mockFileStorage
                .SetupSequence(x => x.SaveAsync(It.IsAny<Stream>(), GearItemService.PhotoFolder, ".jpg"))
                .ReturnsAsync("gear-photos/first.jpg")
                .ReturnsAsync("gear-photos/second.jpg");

            await _service.SetPhoto(created.Id, Stream.Null, "first.JPG", 100);
            var result = await _service.SetPhoto(created.Id, Stream.Null, "second.jpg", 100);

            Assert.True(result.HasPhoto);
            Assert.Equal("gear-photos/second.jpg", _context.GearItems.Single().PhotoPath);
            _mockFileStorage.Verify(x => x.Delete("gear-photos/first.jpg"), Times.Once);
        }

        [Fact]
        public async Task Delete_ShouldFail_WhenGearIsUsedInASetup()
        {
            var created = await _service.Insert(NewDto());
            var project = new Project { Title = "Album", Client = new DataLayer.Models.ClientModels.Client { Name = "Label" } };
            _context.ProjectInstruments.Add(new ProjectInstrument
            {
                Name = "Drums",
                Project = project,
                SetupItems = [new InstrumentSetupItem { GearItemId = created.Id, UsedFor = "Snare Top" }]
            });
            await _context.SaveChangesAsync();

            var exception = await Assert.ThrowsAsync<BusinessRuleException>(() => _service.Delete(created.Id));

            Assert.Equal("gearItem.usedInSetups", exception.ErrorCode);
        }

        [Fact]
        public async Task GetAll_ShouldCountGearOfTodaysSessionsInstruments()
        {
            var created = await _service.Insert(NewDto());
            var today = DateOnly.FromDateTime(DateTime.Now);
            var project = new Project { Title = "Album", Client = new DataLayer.Models.ClientModels.Client { Name = "Label" } };
            var drums = new ProjectInstrument
            {
                Name = "Drums",
                Project = project,
                SetupItems = [new InstrumentSetupItem { GearItemId = created.Id, Quantity = 2, UsedFor = "Toms" }]
            };
            _context.Sessions.AddRange(
                new Session { Title = "Today", Date = today, Project = project, Instruments = [drums] },
                new Session { Title = "Tomorrow", Date = today.AddDays(1), Project = project, Instruments = [drums] });
            await _context.SaveChangesAsync();

            var item = Assert.Single(await _service.GetAll());

            Assert.Equal(2, item.InSessionTodayQuantity);
        }

        [Fact]
        public async Task Delete_ShouldAlsoDeleteThePhoto()
        {
            var created = await _service.Insert(NewDto());
            _mockFileStorage
                .Setup(x => x.SaveAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync("gear-photos/photo.png");
            await _service.SetPhoto(created.Id, Stream.Null, "photo.png", 100);

            await _service.Delete(created.Id);

            Assert.Empty(_context.GearItems);
            _mockFileStorage.Verify(x => x.Delete("gear-photos/photo.png"), Times.Once);
        }
    }
}
