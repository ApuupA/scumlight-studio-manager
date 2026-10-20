using StudioManager.DataLayer.Context;
using StudioManager.DataLayer.Models.ClientModels;
using StudioManager.DataLayer.Models.ProjectModels;
using StudioManager.DataLayer.Models.SessionModels;
using StudioManager.RepositoryLayer.Repository;
using StudioManager.ServiceLayer.Service.SessionServices;

namespace StudioManager.Tests.Unit
{
    public class SessionTaskServiceTests
    {
        private readonly AppDbContext _context = TestHelper.CreateDbContext();
        private readonly SessionTaskService _service;

        public SessionTaskServiceTests()
        {
            _service = new SessionTaskService(new Repository<SessionTask>(_context), TestHelper.CreateMapper());
        }

        [Fact]
        public async Task GetOpen_ShouldReturnOpenTasks_OldestSessionFirst_WithoutCancelledSessions()
        {
            var project = new Project { Title = "Debut EP", Client = new Client { Name = "Band" } };
            _context.Sessions.AddRange(
                new Session
                {
                    Title = "Drums", Date = new DateOnly(2026, 11, 6), Project = project,
                    Tasks = [new SessionTask { Text = "Tune toms", SortOrder = 1 }, new SessionTask { Text = "Done already", IsDone = true }]
                },
                new Session
                {
                    Title = "Pre-production", Date = new DateOnly(2026, 10, 1), Project = project,
                    Tasks = [new SessionTask { Text = "Send demos" }]
                },
                new Session
                {
                    Title = "Cancelled", Date = new DateOnly(2026, 10, 15), Project = project, Status = SessionStatus.Cancelled,
                    Tasks = [new SessionTask { Text = "Never mind" }]
                });
            await _context.SaveChangesAsync();

            var open = (await _service.GetOpen()).ToList();

            Assert.Equal(["Send demos", "Tune toms"], open.Select(t => t.Text));
            Assert.Equal("Drums", open[1].SessionTitle);
            Assert.Equal("Debut EP", open[1].ProjectTitle);
        }
    }
}
