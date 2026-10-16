using StudioManager.DataLayer.Context;
using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.Models.ClientModels;
using StudioManager.DataLayer.Models.ProjectModels;
using StudioManager.RepositoryLayer.Repository;
using StudioManager.ServiceLayer.Service.ProjectServices;

namespace StudioManager.Tests.Unit
{
    public class SongServiceTests
    {
        private readonly AppDbContext _context = TestHelper.CreateDbContext();
        private readonly SongService _service;
        private readonly Project _project;

        public SongServiceTests()
        {
            _service = new SongService(new Repository<Song>(_context), new Repository<Project>(_context), TestHelper.CreateMapper());
            _project = new Project { Title = "EP", Client = new Client { Name = "Label" } };
            _context.Projects.Add(_project);
            _context.SaveChanges();
        }

        [Fact]
        public async Task SetStatus_ShouldChangeOnlyTheSelectedSongs()
        {
            var songs = Enumerable.Range(1, 3)
                .Select(number => new Song { Title = $"Song {number}", TrackNumber = number, Project = _project })
                .ToList();
            _context.Songs.AddRange(songs);
            await _context.SaveChangesAsync();

            var result = await _service.SetStatus(new SongStatusUpdateDto
            {
                SongIds = [songs[0].Id, songs[2].Id],
                Status = SongStatus.Mixing
            });

            Assert.Equal(new[] { "Song 1", "Song 3" }, result.Select(s => s.Title));
            Assert.Equal(new[] { SongStatus.Mixing, SongStatus.Planned, SongStatus.Mixing }, _context.Songs.OrderBy(s => s.TrackNumber).Select(s => s.Status));
        }
    }
}
