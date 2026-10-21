using StudioManager.DataLayer.Context;
using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.Models.ClientModels;
using StudioManager.DataLayer.Models.ProjectModels;
using StudioManager.RepositoryLayer.Repository;
using StudioManager.ServiceLayer.Exceptions;
using StudioManager.ServiceLayer.Service.ProjectServices;

namespace StudioManager.Tests.Unit
{
    /// <summary>
    /// Tests for mix versions with feedback, the mix / master details of a song and the deliverables checklist.
    /// </summary>
    public class MixMasterTests
    {
        private readonly AppDbContext _context = TestHelper.CreateDbContext();
        private readonly MixVersionService _versionService;
        private readonly MixFeedbackService _feedbackService;
        private readonly SongService _songService;
        private readonly ProjectDeliverableService _deliverableService;
        private readonly Project _project;
        private readonly Song _song;

        public MixMasterTests()
        {
            var mapper = TestHelper.CreateMapper();
            _versionService = new MixVersionService(new Repository<MixVersion>(_context), new Repository<Song>(_context), mapper);
            _feedbackService = new MixFeedbackService(new Repository<MixFeedback>(_context), mapper);
            _songService = new SongService(new Repository<Song>(_context), new Repository<Project>(_context), mapper);
            _deliverableService = new ProjectDeliverableService(new Repository<ProjectDeliverable>(_context), new Repository<Project>(_context), mapper);

            _project = new Project { Title = "Mix EP", Client = new Client { Name = "Band" }, IncludedRevisions = 2 };
            _song = new Song { Title = "Opener", TrackNumber = 1, Project = _project };
            _context.Songs.Add(_song);
            _context.SaveChanges();
        }

        private MixVersionCreateDto Version() => new() { SongId = _song.Id, Date = new DateOnly(2026, 10, 21), Link = "  ", Notes = " Vocals +1 dB " };

        [Fact]
        public async Task Versions_ShouldBeNumbered_AndRevisionsBeyondTheIncludedOnesAreExtra()
        {
            var v1 = await _versionService.Insert(Version());
            var v2 = await _versionService.Insert(Version());
            var v3 = await _versionService.Insert(Version());
            var v4 = await _versionService.Insert(Version());

            Assert.Equal([1, 2, 3, 4], new[] { v1.Number, v2.Number, v3.Number, v4.Number });
            Assert.Null(v1.Link);
            Assert.Equal("Vocals +1 dB", v1.Notes);
            // 2 revisions included: v2 and v3 are free, v4 is the 3rd revision
            Assert.False(v3.IsExtraRevision);
            Assert.True(v4.IsExtraRevision);
            Assert.Equal(4, (await _versionService.GetForSong(_song.Id)).First().Number);

            var song = (await _songService.Get(_song.Id))!;
            Assert.Equal(4, song.LatestMixNumber);
            Assert.Equal(MixVersionStatus.Sent, song.LatestMixStatus);
        }

        [Fact]
        public async Task Feedback_ShouldBeListedWithTheVersion_AndDeletedWithIt()
        {
            var version = await _versionService.Insert(Version());
            await _feedbackService.Insert(new MixFeedbackCreateDto { MixVersionId = version.Id, Timecode = " 1:23 ", Text = " Vocals too quiet " });
            await _feedbackService.Insert(new MixFeedbackCreateDto { MixVersionId = version.Id, Timecode = "", Text = "More low end overall" });

            var loaded = (await _versionService.Get(version.Id))!;
            Assert.Equal(["1:23", null], loaded.Feedback.Select(f => f.Timecode));
            Assert.Equal("Vocals too quiet", loaded.Feedback[0].Text);

            await _versionService.Delete(version.Id);
            Assert.Empty(_context.MixFeedback);
        }

        [Fact]
        public async Task Update_ShouldNotMoveAVersionToAnotherSong()
        {
            var version = await _versionService.Insert(Version());
            var other = new Song { Title = "Other", Project = _project };
            _context.Songs.Add(other);
            await _context.SaveChangesAsync();

            var dto = Version();
            dto.SongId = other.Id;

            var exception = await Assert.ThrowsAsync<BusinessRuleException>(() => _versionService.Update(version.Id, dto));
            Assert.Equal("mixVersion.otherSong", exception.ErrorCode);
        }

        [Fact]
        public async Task MixInfo_ShouldBeSaved_AndKeptWhenTheSongFormIsSaved()
        {
            await _songService.SetMixInfo(_song.Id, new SongMixInfoDto
            {
                MaterialTrackCount = 48,
                MaterialNotes = "  Kick missing  ",
                Isrc = "de-a12-26-00001",
                MeasuredLufs = -9.2m
            });

            // Saving the song form (title, status, ...) must not wipe the mix details
            var saved = await _songService.Update(_song.Id, new SongCreateDto { Title = "Opener (final)", TrackNumber = 1, ProjectId = _project.Id });

            Assert.Equal(48, saved.MaterialTrackCount);
            Assert.Equal("Kick missing", saved.MaterialNotes);
            Assert.Equal("DEA122600001", saved.Isrc);
            Assert.Equal(-9.2m, saved.MeasuredLufs);
        }

        [Fact]
        public async Task AddDefaults_ShouldNotAddTheSameDeliverableTwice()
        {
            await _deliverableService.Insert(new ProjectDeliverableCreateDto { ProjectId = _project.Id, Name = "MP3 320 kbps", SortOrder = 1 });

            await _deliverableService.AddDefaults(_project.Id);
            var list = (await _deliverableService.AddDefaults(_project.Id)).ToList();

            Assert.Equal(ProjectDeliverableService.DefaultNames.Length, list.Count);
            Assert.Equal("MP3 320 kbps", list[0].Name);
        }
    }
}
