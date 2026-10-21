using Moq;
using StudioManager.DataLayer.Context;
using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.Models.ClientModels;
using StudioManager.DataLayer.Models.ProjectModels;
using StudioManager.RepositoryLayer.Repository;
using StudioManager.ServiceLayer.Exceptions;
using StudioManager.ServiceLayer.IService.IFileServices;
using StudioManager.ServiceLayer.Service.ProjectServices;

namespace StudioManager.Tests.Unit
{
    /// <summary>
    /// Tests for the voice memos of songwriting: upload, sorting to songs and parts, deleting the file.
    /// </summary>
    public class VoiceMemoTests
    {
        private readonly AppDbContext _context = TestHelper.CreateDbContext();
        private readonly Mock<IFileStorageService> _mockFileStorage = new();
        private readonly VoiceMemoService _service;
        private readonly Project _project;
        private readonly Song _song;
        private readonly Song _otherSong;
        private readonly SongSection _chorus;

        public VoiceMemoTests()
        {
            _service = new VoiceMemoService(
                new Repository<VoiceMemo>(_context), new Repository<Project>(_context), new Repository<Song>(_context),
                new Repository<SongSection>(_context), _mockFileStorage.Object, TestHelper.CreateMapper());

            _project = new Project { Title = "Neues Album", Client = new Client { Name = "Eigene Band" } };
            _song = new Song { Title = "Riffmonster", Project = _project };
            _otherSong = new Song { Title = "Ballade", Project = _project };
            _chorus = new SongSection { Name = "Chorus 1", Song = _song };
            _context.AddRange(_song, _otherSong, _chorus);
            _context.SaveChanges();

            _mockFileStorage
                .Setup(x => x.SaveAsync(It.IsAny<Stream>(), VoiceMemoService.MemoFolder, It.IsAny<string>()))
                .ReturnsAsync("voice-memos/abc.m4a");
        }

        private VoiceMemoCreateDto UpdateDto(int? songId, int? sectionId) => new()
        {
            ProjectId = _project.Id,
            Title = "  Riff Strophe  ",
            Notes = " ",
            SongId = songId,
            SongSectionId = sectionId
        };

        [Fact]
        public async Task Upload_ShouldUseTheFileNameAsTitle_AndLandInTheInbox()
        {
            var memo = await _service.Upload(_project.Id, Stream.Null, "Neue Aufnahme 37.M4A", 1000, new DateOnly(2025, 5, 1), null);

            Assert.Equal("Neue Aufnahme 37", memo.Title);
            Assert.Equal("Neue Aufnahme 37.M4A", memo.FileName);
            Assert.Equal(new DateOnly(2025, 5, 1), memo.RecordedOn);
            Assert.Null(memo.SongId);
        }

        [Fact]
        public async Task Upload_ShouldRejectOtherFilesAndTooLargeFiles()
        {
            var wrongType = await Assert.ThrowsAsync<BusinessRuleException>(() =>
                _service.Upload(_project.Id, Stream.Null, "Notizen.pdf", 1000, null, null));
            var tooLarge = await Assert.ThrowsAsync<BusinessRuleException>(() =>
                _service.Upload(_project.Id, Stream.Null, "Probe.wav", VoiceMemoService.MaxMemoSizeBytes + 1, null, null));

            Assert.Equal("voiceMemo.invalidType", wrongType.ErrorCode);
            Assert.Equal("voiceMemo.tooLarge", tooLarge.ErrorCode);
            Assert.Empty(_context.VoiceMemos);
        }

        [Fact]
        public async Task Update_WithAPart_ShouldSetTheSongOfThePart()
        {
            var memo = await _service.Upload(_project.Id, Stream.Null, "Idee.m4a", 1000, null, null);

            var result = await _service.Update(memo.Id, UpdateDto(null, _chorus.Id));

            Assert.Equal(_song.Id, result.SongId);
            Assert.Equal("Riffmonster", result.SongTitle);
            Assert.Equal("Chorus 1", result.SongSectionName);
            Assert.Equal("Riff Strophe", result.Title);
            Assert.Null(result.Notes);
        }

        [Fact]
        public async Task Update_WithAPartOfAnotherSong_ShouldBeRejected()
        {
            var memo = await _service.Upload(_project.Id, Stream.Null, "Idee.m4a", 1000, null, null);

            var exception = await Assert.ThrowsAsync<BusinessRuleException>(() => _service.Update(memo.Id, UpdateDto(_otherSong.Id, _chorus.Id)));

            Assert.Equal("voiceMemo.sectionOfOtherSong", exception.ErrorCode);
        }

        [Fact]
        public async Task Delete_ShouldDeleteTheAudioFile()
        {
            var memo = await _service.Upload(_project.Id, Stream.Null, "Idee.m4a", 1000, null, _song.Id);

            await _service.Delete(memo.Id);

            _mockFileStorage.Verify(x => x.Delete("voice-memos/abc.m4a"), Times.Once);
            Assert.Empty(_context.VoiceMemos);
        }
    }
}
