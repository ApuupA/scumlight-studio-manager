using Moq;
using StudioManager.DataLayer.Context;
using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.Models.ClientModels;
using StudioManager.DataLayer.Models.FinanceModels;
using StudioManager.DataLayer.Models.ProjectModels;
using StudioManager.RepositoryLayer.Repository;
using StudioManager.ServiceLayer.Exceptions;
using StudioManager.ServiceLayer.IService.IFileServices;
using StudioManager.ServiceLayer.Service.ClientServices;
using StudioManager.ServiceLayer.Service.FileServices;
using StudioManager.ServiceLayer.Service.ProjectServices;

namespace StudioManager.Tests.Unit
{
    /// <summary>
    /// Tests for projects and the client / band rules around them.
    /// </summary>
    public class ProjectServiceTests
    {
        private readonly AppDbContext _context = TestHelper.CreateDbContext();
        private readonly Mock<IFileStorageService> _mockFileStorage = new();
        private readonly PhotoService _photos;
        private readonly ProjectService _projectService;
        private readonly ClientService _clientService;
        private readonly Client _label;
        private readonly Band _bandA;
        private readonly Band _bandB;
        private readonly Band _bandOfOtherClient;

        public ProjectServiceTests()
        {
            var mapper = TestHelper.CreateMapper();
            _photos = new PhotoService(_mockFileStorage.Object);
            _projectService = new ProjectService(
                new Repository<Project>(_context), new Repository<Client>(_context), new Repository<Band>(_context), _photos, new Repository<Invoice>(_context), mapper);
            _clientService = new ClientService(new Repository<Client>(_context), new Repository<Project>(_context), _photos, mapper);

            _label = new Client { Name = "Split Label" };
            _bandA = new Band { Name = "Band A", Client = _label };
            _bandB = new Band { Name = "Band B", Client = _label };
            _bandOfOtherClient = new Band { Name = "Foreign Band", Client = new Client { Name = "Other" } };
            _context.AddRange(_bandA, _bandB, _bandOfOtherClient);
            _context.SaveChanges();
        }

        private ProjectCreateDto NewDto(params int[] bandIds) => new()
        {
            Title = "Split EP",
            Type = ProjectType.SplitRelease,
            ClientId = _label.Id,
            BandIds = bandIds.ToList()
        };

        [Fact]
        public async Task Insert_ShouldLinkSeveralBandsOfTheClient()
        {
            var result = await _projectService.Insert(NewDto(_bandA.Id, _bandB.Id));

            Assert.Equal("Split Label", result.ClientName);
            Assert.Equal(new[] { "Band A", "Band B" }, result.Bands.Select(b => b.Name).Order());
        }

        [Fact]
        public async Task Insert_ShouldRejectBandOfAnotherClient()
        {
            var exception = await Assert.ThrowsAsync<BusinessRuleException>(
                () => _projectService.Insert(NewDto(_bandA.Id, _bandOfOtherClient.Id)));

            Assert.Equal("project.bandNotOfClient", exception.ErrorCode);
        }

        [Fact]
        public async Task Update_ShouldReplaceBands()
        {
            var created = await _projectService.Insert(NewDto(_bandA.Id, _bandB.Id));

            var updated = await _projectService.Update(created.Id, NewDto(_bandB.Id));

            Assert.Equal("Band B", Assert.Single(updated.Bands).Name);
        }

        [Fact]
        public async Task GetAll_ShouldCalculateSongProgress()
        {
            var created = await _projectService.Insert(NewDto(_bandA.Id));
            _context.Songs.AddRange(
                new Song { Title = "One", ProjectId = created.Id, Status = SongStatus.Done },
                new Song { Title = "Two", ProjectId = created.Id, Status = SongStatus.Mixing });
            await _context.SaveChangesAsync();

            var project = Assert.Single(await _projectService.GetAll());

            Assert.Equal(2, project.SongCount);
            Assert.Equal(1, project.SongsDoneCount);
        }

        [Fact]
        public async Task DeleteClient_ShouldFail_WhenClientHasProjects()
        {
            await _projectService.Insert(NewDto(_bandA.Id));

            var exception = await Assert.ThrowsAsync<BusinessRuleException>(() => _clientService.Delete(_label.Id));

            Assert.Equal("client.hasProjects", exception.ErrorCode);
        }

        [Fact]
        public async Task ClientPhoto_ShouldBeSaved_AndRemovedWithTheClientAndItsBands()
        {
            var client = new Client { Name = "Photo Label", Bands = [new Band { Name = "Photo Band", PhotoPath = "band-photos/band.jpg" }] };
            _context.Clients.Add(client);
            await _context.SaveChangesAsync();
            _mockFileStorage
                .Setup(x => x.SaveAsync(It.IsAny<Stream>(), ClientService.PhotoFolder, ".png"))
                .ReturnsAsync("client-photos/logo.png");

            var withPhoto = await _clientService.SetPhoto(client.Id, Stream.Null, "Logo.PNG", 100);
            await _clientService.Delete(client.Id);

            Assert.True(withPhoto.HasPhoto);
            _mockFileStorage.Verify(x => x.Delete("client-photos/logo.png"), Times.Once);
            _mockFileStorage.Verify(x => x.Delete("band-photos/band.jpg"), Times.Once);
        }

        [Fact]
        public async Task ProjectCover_ShouldBeKept_WhenTheProjectIsEdited()
        {
            var project = await _projectService.Insert(NewDto(_bandA.Id));
            _mockFileStorage
                .Setup(x => x.SaveAsync(It.IsAny<Stream>(), ProjectService.PhotoFolder, ".jpg"))
                .ReturnsAsync("project-covers/cover.jpg");
            await _projectService.SetPhoto(project.Id, Stream.Null, "cover.jpg", 100);

            // Saving the project form must not remove the cover
            var updated = await _projectService.Update(project.Id, NewDto(_bandA.Id));

            Assert.True(updated.HasPhoto);
            Assert.Equal("project-covers/cover.jpg", _context.Projects.Single().PhotoPath);
        }

        [Fact]
        public async Task Photo_WithWrongFileType_ShouldBeRejected()
        {
            var exception = await Assert.ThrowsAsync<BusinessRuleException>(
                () => _clientService.SetPhoto(_label.Id, Stream.Null, "logo.gif", 100));

            Assert.Equal("photo.invalidType", exception.ErrorCode);
        }
    }
}
