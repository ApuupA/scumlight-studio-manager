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
    /// Tests for the song page: arrangement, recording checklist and lyrics.
    /// </summary>
    public class SongPageTests
    {
        private readonly AppDbContext _context = TestHelper.CreateDbContext();
        private readonly SongService _songService;
        private readonly SongSectionService _sectionService;
        private readonly SongTrackService _trackService;
        private readonly Project _project;
        private readonly Song _song;

        public SongPageTests()
        {
            var mapper = TestHelper.CreateMapper();
            _songService = new SongService(new Repository<Song>(_context), new Repository<Project>(_context), mapper);
            _sectionService = new SongSectionService(new Repository<SongSection>(_context), new Repository<Song>(_context), mapper);
            _trackService = new SongTrackService(
                new Repository<SongTrack>(_context), new Repository<Song>(_context), new Repository<ProjectInstrument>(_context), mapper);

            _project = new Project { Title = "Album", Client = new Client { Name = "Label" } };
            _song = new Song { Title = "Intro", TrackNumber = 1, Project = _project };
            _context.Songs.Add(_song);
            _context.ProjectInstruments.AddRange(
                new ProjectInstrument { Name = "Drums", SortOrder = 1, Project = _project },
                new ProjectInstrument { Name = "Guitar", SortOrder = 2, Project = _project });
            _context.SaveChanges();
        }

        private SongSectionCreateDto Section(string name, int? fromBar = null, int? toBar = null) =>
            new() { SongId = _song.Id, Name = name, FromBar = fromBar, ToBar = toBar };

        [Fact]
        public async Task Sections_ShouldBeAppendedAndMovable()
        {
            await _sectionService.Insert(Section("Intro", 1, 8));
            var verse = await _sectionService.Insert(Section(" Verse 1 ", 9, 24));
            await _sectionService.Insert(Section("Chorus 1", 25, 40));

            await _sectionService.Move(verse.Id, new MoveDto { Direction = 1 });

            var sections = (await _sectionService.GetForSong(_song.Id)).ToList();
            Assert.Equal(new[] { "Intro", "Chorus 1", "Verse 1" }, sections.Select(s => s.Name)); // trimmed and moved
            Assert.Equal(new[] { 1, 2, 3 }, sections.Select(s => s.SortOrder));
        }

        [Fact]
        public async Task Section_WithLastBarBeforeFirstBar_ShouldBeRejected()
        {
            var exception = await Assert.ThrowsAsync<BusinessRuleException>(() => _sectionService.Insert(Section("Bridge", 40, 32)));

            Assert.Equal("songSection.barsInvalid", exception.ErrorCode);
        }

        [Fact]
        public async Task AddFromInstruments_ShouldSkipNamesAlreadyOnTheChecklist()
        {
            await _trackService.Insert(new SongTrackCreateDto { SongId = _song.Id, Name = "drums", Status = SongTrackStatus.Recorded });

            var tracks = (await _trackService.AddFromInstruments(_song.Id)).ToList();

            Assert.Equal(new[] { "drums", "Guitar" }, tracks.Select(t => t.Name)); // "Drums" is already there
            Assert.Equal(SongTrackStatus.Open, tracks[1].Status);
        }

        [Fact]
        public async Task CopyFromSong_ShouldCopyTheNamesAsOpenTracks()
        {
            var otherSong = new Song { Title = "Outro", TrackNumber = 2, Project = _project };
            _context.Songs.Add(otherSong);
            await _context.SaveChangesAsync();
            await _trackService.Insert(new SongTrackCreateDto { SongId = otherSong.Id, Name = "Guitar L", Status = SongTrackStatus.Edited });
            await _trackService.Insert(new SongTrackCreateDto { SongId = otherSong.Id, Name = "Guitar R", Status = SongTrackStatus.Edited });

            var tracks = await _trackService.CopyFromSong(new SongTrackCopyDto { FromSongId = otherSong.Id, ToSongId = _song.Id });

            Assert.Equal(new[] { "Guitar L", "Guitar R" }, tracks.Select(t => t.Name));
            Assert.All(tracks, t => Assert.Equal(SongTrackStatus.Open, t.Status));
        }

        [Fact]
        public async Task CopyFromSong_OfAnotherProject_ShouldBeRejected()
        {
            var foreignSong = new Song { Title = "Foreign", Project = new Project { Title = "Other", ClientId = _project.ClientId } };
            _context.Songs.Add(foreignSong);
            await _context.SaveChangesAsync();

            var exception = await Assert.ThrowsAsync<BusinessRuleException>(
                () => _trackService.CopyFromSong(new SongTrackCopyDto { FromSongId = foreignSong.Id, ToSongId = _song.Id }));

            Assert.Equal("songTrack.otherProject", exception.ErrorCode);
        }

        [Fact]
        public async Task Song_ShouldShowTheRecordingProgress()
        {
            await _trackService.Insert(new SongTrackCreateDto { SongId = _song.Id, Name = "Drums", Status = SongTrackStatus.Edited });
            await _trackService.Insert(new SongTrackCreateDto { SongId = _song.Id, Name = "Bass", Status = SongTrackStatus.Recorded });
            await _trackService.Insert(new SongTrackCreateDto { SongId = _song.Id, Name = "Vocals" });

            var song = (await _songService.Get(_song.Id))!;

            Assert.Equal(3, song.TrackCount);
            Assert.Equal(2, song.TracksRecordedCount);
        }

        [Fact]
        public async Task Lyrics_ShouldBeSaved_AndKeptWhenTheSongIsEdited()
        {
            await _songService.SetLyrics(_song.Id, new SongLyricsDto { Lyrics = "First line\nSecond line\n\n" });

            // Editing the song in the song form must not delete the lyrics
            var updated = await _songService.Update(_song.Id, new SongCreateDto { Title = "Intro (new)", ProjectId = _project.Id });

            Assert.Equal("First line\nSecond line", updated.Lyrics);
            Assert.Equal("Intro (new)", updated.Title);
        }
    }
}
