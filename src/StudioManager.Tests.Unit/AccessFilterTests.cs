using Microsoft.EntityFrameworkCore;
using Moq;
using StudioManager.DataLayer.Context;
using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.Models.ClientModels;
using StudioManager.DataLayer.Models.FinanceModels;
using StudioManager.DataLayer.Models.GearModels;
using StudioManager.DataLayer.Models.ProjectModels;
using StudioManager.DataLayer.Models.SessionModels;
using StudioManager.DataLayer.Models.SettingsModels;
using StudioManager.DataLayer.Models.UserModels;
using StudioManager.RepositoryLayer.Repository;
using StudioManager.ServiceLayer.IService.IFileServices;
using StudioManager.ServiceLayer.Service.CalendarServices;
using StudioManager.ServiceLayer.Service.FileServices;
using StudioManager.ServiceLayer.Service.ProjectServices;

namespace StudioManager.Tests.Unit
{
    /// <summary>
    /// Tests for "who sees which project" (AppDbContext.Access.cs): the owner sees everything, an assistant only the
    /// projects of their groups, the ones visible to everyone and their own - and everything belonging to them.
    /// </summary>
    public class AccessFilterTests
    {
        private readonly TestProjectAccess _access = new();
        private readonly AppDbContext _context;
        private readonly User _assistant;
        private readonly Project _groupProject;
        private readonly Project _sharedProject;
        private readonly Project _foreignProject;
        private readonly Song _foreignSong;

        public AccessFilterTests()
        {
            _context = TestHelper.CreateDbContext(_access);

            // Everything is created as the owner (sees everything)
            _assistant = new User { Email = "assi@studio.local", DisplayName = "Assi", Role = UserRole.Assistant };
            var team = new Group { Name = "Sacroscum", Users = { _assistant } };
            var otherTeam = new Group { Name = "Other band" };

            _groupProject = new Project { Title = "Group project", Client = new Client { Name = "Sacroscum" }, Groups = { team } };
            _sharedProject = new Project { Title = "Shared project", Client = new Client { Name = "Shared client" }, IsVisibleToAll = true };
            _foreignProject = new Project { Title = "Foreign project", Client = new Client { Name = "Foreign label", Bands = { new Band { Name = "Foreign band" } } }, Groups = { otherTeam } };
            _foreignSong = new Song { Title = "Foreign song", Project = _foreignProject };

            _context.AddRange(_groupProject, _sharedProject, _foreignProject, _foreignSong);
            _context.Add(new Song { Title = "Group song", Project = _groupProject });
            _context.Add(new Session { Title = "Foreign session", Project = _foreignProject, Date = new DateOnly(2026, 11, 1) });
            _context.Add(new Session { Title = "Group session", Project = _groupProject, Date = new DateOnly(2026, 11, 2) });
            _context.SaveChanges();
            _context.ChangeTracker.Clear();
        }

        /// <summary>
        /// Every table that belongs to a project needs a filter - otherwise an assistant would see the rows of all projects.
        /// A new entity either gets a filter in AppDbContext.Access.cs or is added here as "global" (visible for everybody).
        /// </summary>
        [Fact]
        public void EveryEntity_ShouldHaveAnAccessFilter_OrBeOnTheGlobalList()
        {
            Type[] global =
            [
                typeof(User), typeof(Group), typeof(GearCategory), typeof(GearItem), typeof(SetupTemplate), typeof(SetupTemplateItem),
                typeof(InputGroup), typeof(StudioInput), typeof(BusinessSettings), typeof(StandardTask)
            ];

            var missing = _context.Model.GetEntityTypes()
                .Where(t => !t.HasSharedClrType)          // join tables of many-to-many (ProjectBands, ...)
                .Where(t => !global.Contains(t.ClrType))
                .Where(t => t.GetDeclaredQueryFilters().Count == 0)
                .Select(t => t.ClrType.Name)
                .ToList();

            Assert.Empty(missing);
        }

        [Fact]
        public void Owner_ShouldSeeEverything()
        {
            Assert.Equal(3, _context.Projects.Count());
            Assert.Equal(2, _context.Sessions.Count());
            Assert.Equal(3, _context.Clients.Count());
        }

        [Fact]
        public void Assistant_ShouldSeeTheProjectsOfTheirGroups_AndTheSharedOnes()
        {
            _access.AsAssistant(_assistant.Id);

            Assert.Equal(["Group project", "Shared project"], _context.Projects.OrderBy(p => p.Title).Select(p => p.Title));
        }

        [Fact]
        public void Assistant_ShouldNotSeeAnythingOfAForeignProject()
        {
            _access.AsAssistant(_assistant.Id);

            // Also when asking for the id directly (/api/Song/17)
            Assert.Null(_context.Songs.SingleOrDefault(s => s.Id == _foreignSong.Id));
            Assert.Equal(["Group song"], _context.Songs.Select(s => s.Title));
            Assert.Equal(["Group session"], _context.Sessions.Select(s => s.Title));
            Assert.DoesNotContain("Foreign label", _context.Clients.Select(c => c.Name));
            Assert.Empty(_context.Bands);
        }

        [Fact]
        public async Task Assistant_ShouldNotAddDataToAForeignProject()
        {
            _access.AsAssistant(_assistant.Id);

            // The id of the foreign song is guessed - the database would accept it (the song exists)
            _context.SongSections.Add(new SongSection { Name = "Hacked chorus", SongId = _foreignSong.Id });

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _context.SaveChangesAsync());
        }

        [Fact]
        public async Task Assistant_ShouldSeeTheirOwnNewProject_WithTheirGroups_AndTheirOwnClient()
        {
            _access.AsAssistant(_assistant.Id);
            var service = new ProjectService(
                new Repository<Project>(_context), new Repository<Client>(_context), new Repository<Band>(_context),
                new PhotoService(new Mock<IFileStorageService>().Object), new Repository<Invoice>(_context), new Repository<Quote>(_context),
                new Repository<VoiceMemo>(_context), new Mock<IFileStorageService>().Object, new Repository<Group>(_context), _access,
                TestHelper.CreateMapper());

            // A new client of the assistant - visible without any project
            var client = new Client { Name = "New band" };
            _context.Clients.Add(client);
            await _context.SaveChangesAsync();
            Assert.Equal(_assistant.Id, client.CreatedByUserId);

            var project = await service.Insert(new ProjectCreateDto { Title = "Demo EP", ClientId = client.Id });

            Assert.Equal("Sacroscum", Assert.Single(project.Groups).Name);
            Assert.Contains("Demo EP", _context.Projects.Select(p => p.Title));
            Assert.Contains("New band", _context.Clients.Select(c => c.Name));
        }

        [Fact]
        public async Task CalendarFeed_ShouldOnlyContainTheSessionsOfTheTokensUser()
        {
            var assistant = await _context.Users.SingleAsync(u => u.Id == _assistant.Id);
            assistant.CalendarToken = "assistant-token";
            await _context.SaveChangesAsync();
            var service = new CalendarService(new Repository<User>(_context), new Repository<Session>(_context), _access);

            var feed = await service.GetFeed("assistant-token");

            Assert.Contains("Group session", feed);
            Assert.DoesNotContain("Foreign session", feed);
        }
    }
}
