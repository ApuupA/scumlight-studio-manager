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

        private SongCreateDto SongWithTempos(params (decimal Bpm, int? FromBar)[] tempos) => new()
        {
            Title = "Intro",
            ProjectId = _project.Id,
            Tempos = tempos.Select(t => new SongTempoCreateDto { Bpm = t.Bpm, FromBar = t.FromBar }).ToList()
        };

        [Fact]
        public async Task Insert_ShouldKeepTheTemposInSongOrder()
        {
            var song = await _service.Insert(SongWithTempos((190, null), (120, 121), (190, 141)));

            Assert.Equal(new[] { 190m, 120m, 190m }, song.Tempos.Select(t => t.Bpm));
            Assert.Equal(new int?[] { null, 121, 141 }, song.Tempos.Select(t => t.FromBar));
        }

        [Fact]
        public async Task Update_ShouldReplaceTheTempos()
        {
            var song = await _service.Insert(SongWithTempos((190, null), (120, 121)));

            var updated = await _service.Update(song.Id, SongWithTempos((140, null)));

            Assert.Equal(140m, Assert.Single(updated.Tempos).Bpm);
            Assert.Single(_context.SongTempos); // the old ones are deleted
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
