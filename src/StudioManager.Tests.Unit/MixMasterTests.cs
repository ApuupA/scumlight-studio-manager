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
    /// Tests for mix versions of a project (with song selection and feedback), the song notes,
    /// the master values and the deliverables checklist.
    /// </summary>
    public class MixMasterTests
    {
        private readonly AppDbContext _context = TestHelper.CreateDbContext();
        private readonly MixVersionService _versionService;
        private readonly MixFeedbackService _feedbackService;
        private readonly SongService _songService;
        private readonly ProjectDeliverableService _deliverableService;
        private readonly Project _project;
        private readonly Song _opener;
        private readonly Song _ballad;

        public MixMasterTests()
        {
            var mapper = TestHelper.CreateMapper();
            _versionService = new MixVersionService(new Repository<MixVersion>(_context), new Repository<Project>(_context), new Repository<Song>(_context), mapper);
            _feedbackService = new MixFeedbackService(new Repository<MixFeedback>(_context), new Repository<MixVersion>(_context), mapper);
            _songService = new SongService(new Repository<Song>(_context), new Repository<Project>(_context), mapper);
            _deliverableService = new ProjectDeliverableService(new Repository<ProjectDeliverable>(_context), new Repository<Project>(_context), mapper);

            _project = new Project { Title = "Mix EP", Client = new Client { Name = "Band" }, IncludedRevisions = 1 };
            _opener = new Song { Title = "Opener", TrackNumber = 1, Project = _project };
            _ballad = new Song { Title = "Ballad", TrackNumber = 2, Project = _project };
            _context.Songs.AddRange(_opener, _ballad);
            _context.SaveChanges();
        }

        private MixVersionCreateDto Version(string? name, params Song[] songs) => new()
        {
            ProjectId = _project.Id,
            Name = name,
            Date = new DateOnly(2026, 10, 21),
            Link = "  ",
            Notes = " Vocals +1 dB ",
            SongIds = songs.Select(s => s.Id).ToList()
        };

        [Fact]
        public async Task Versions_ShouldBeNumbered_AndNamedAutomaticallyIfEmpty()
        {
            var v1 = await _versionService.Insert(Version(null, _opener, _ballad));
            var v11 = await _versionService.Insert(Version("  v1.1 ", _ballad));

            Assert.Equal("v1", v1.Name);
            Assert.True(v1.IsAllSongs);
            Assert.Null(v1.Link);
            Assert.Equal("Vocals +1 dB", v1.Notes);
            Assert.Equal("v1.1", v11.Name);
            Assert.Equal(2, v11.Number);
            Assert.False(v11.IsAllSongs);
            Assert.Equal("Ballad", Assert.Single(v11.Songs).Title);
        }

        [Fact]
        public async Task Revisions_ShouldBeCountedPerSong()
        {
            // 1 revision included: the 2nd version of a song is free, the 3rd is extra
            await _versionService.Insert(Version("v1", _opener, _ballad));
            var v11 = await _versionService.Insert(Version("v1.1", _ballad));
            var v12 = await _versionService.Insert(Version("v1.2", _ballad));
            var v2 = await _versionService.Insert(Version("v2", _opener, _ballad));

            Assert.Empty(v11.ExtraRevisionSongs);
            Assert.Equal("Ballad", Assert.Single(v12.ExtraRevisionSongs).Title);
            // The opener is only in its 2nd version (free), the ballad in its 4th (extra)
            Assert.Equal("Ballad", Assert.Single(v2.ExtraRevisionSongs).Title);

            Assert.Equal(["v2", "v1"], (await _versionService.GetForSong(_opener.Id)).Select(v => v.Name));
            var opener = (await _songService.Get(_opener.Id))!;
            Assert.Equal("v2", opener.LatestMixName);
            Assert.Equal(MixVersionStatus.Sent, opener.LatestMixStatus);
        }

        [Fact]
        public async Task Version_ShouldOnlyContainSongsOfTheProject()
        {
            var foreignSong = new Song { Title = "Foreign", Project = new Project { Title = "Other", Client = new Client { Name = "X" } } };
            _context.Songs.Add(foreignSong);
            await _context.SaveChangesAsync();

            var exception = await Assert.ThrowsAsync<BusinessRuleException>(() => _versionService.Insert(Version(null, _opener, foreignSong)));
            Assert.Equal("mixVersion.invalidSongs", exception.ErrorCode);

            var empty = await Assert.ThrowsAsync<BusinessRuleException>(() => _versionService.Insert(Version(null)));
            Assert.Equal("mixVersion.invalidSongs", empty.ErrorCode);
        }

        [Fact]
        public async Task Update_ShouldReplaceTheSongs()
        {
            var version = await _versionService.Insert(Version("v1", _opener, _ballad));

            var updated = await _versionService.Update(version.Id, Version("v1", _ballad));

            Assert.Equal("Ballad", Assert.Single(updated.Songs).Title);
        }

        [Fact]
        public async Task Feedback_ShouldOnlyBeForSongsOfTheVersion_AndBeDeletedWithIt()
        {
            var version = await _versionService.Insert(Version("v1.1", _ballad));
            await _feedbackService.Insert(new MixFeedbackCreateDto { MixVersionId = version.Id, SongId = _ballad.Id, Timecode = " 1:23 ", Text = " Vocals too quiet " });
            await _feedbackService.Insert(new MixFeedbackCreateDto { MixVersionId = version.Id, Timecode = "", Text = "More low end overall" });

            var exception = await Assert.ThrowsAsync<BusinessRuleException>(() =>
                _feedbackService.Insert(new MixFeedbackCreateDto { MixVersionId = version.Id, SongId = _opener.Id, Text = "Not in this version" }));
            Assert.Equal("mixFeedback.songNotInVersion", exception.ErrorCode);

            var loaded = (await _versionService.Get(version.Id))!;
            Assert.Equal(["1:23", null], loaded.Feedback.Select(f => f.Timecode));
            Assert.Equal("Ballad", loaded.Feedback[0].SongTitle);
            Assert.Equal("Vocals too quiet", loaded.Feedback[0].Text);

            await _versionService.Delete(version.Id);
            Assert.Empty(_context.MixFeedback);
        }

        [Fact]
        public async Task SongNotesAndMasterValues_ShouldBeSavedSeparately_AndKeptWhenTheSongFormIsSaved()
        {
            await _songService.SetMixInfo(_opener.Id, new SongMixInfoDto { MaterialNotes = "  Kick missing  ", MixNotes = "LA-2A on vocals" });
            await _songService.SetMasterInfo(_opener.Id, new SongMasterInfoDto { Isrc = "de-a12-26-00001", MeasuredLufs = -9.2m });

            // Saving the song form (title, status, ...) must not wipe the mix notes or master values
            var saved = await _songService.Update(_opener.Id, new SongCreateDto { Title = "Opener (final)", TrackNumber = 1, ProjectId = _project.Id });

            Assert.Equal("Kick missing", saved.MaterialNotes);
            Assert.Equal("LA-2A on vocals", saved.MixNotes);
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
