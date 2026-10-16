using StudioManager.DataLayer.Context;
using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.Models.ClientModels;
using StudioManager.DataLayer.Models.ProjectModels;
using StudioManager.DataLayer.Models.SessionModels;
using StudioManager.RepositoryLayer.Repository;
using StudioManager.ServiceLayer.Exceptions;
using StudioManager.ServiceLayer.Service.SessionServices;

namespace StudioManager.Tests.Unit
{
    public class SessionServiceTests
    {
        private readonly AppDbContext _context = TestHelper.CreateDbContext();
        private readonly SessionService _service;
        private readonly Project _project;
        private readonly Song _song1;
        private readonly Song _song2;
        private readonly Song _otherProjectSong;

        public SessionServiceTests()
        {
            _service = new SessionService(
                new Repository<Session>(_context),
                new Repository<Project>(_context),
                new Repository<Song>(_context),
                new Repository<ProjectInstrument>(_context),
                TestHelper.CreateMapper());

            var client = new Client { Name = "Scumlight Records" };
            _project = new Project { Title = "Debut EP", Client = client };
            var otherProject = new Project { Title = "Other", Client = client };
            _song1 = new Song { Title = "Song 1", Project = _project };
            _song2 = new Song { Title = "Song 2", Project = _project };
            _otherProjectSong = new Song { Title = "Foreign", Project = otherProject };
            _context.AddRange(_song1, _song2, _otherProjectSong);
            _context.SaveChanges();
        }

        private SessionCreateDto NewDto(string? start = "18:00", string? end = "22:00") => new()
        {
            Title = "Mixing",
            Type = SessionType.Mixing,
            Date = new DateOnly(2026, 11, 6),
            StartTime = start == null ? null : TimeOnly.Parse(start),
            EndTime = end == null ? null : TimeOnly.Parse(end),
            ProjectId = _project.Id
        };

        [Fact]
        public async Task Insert_ShouldReturnProjectAndClientName()
        {
            var result = await _service.Insert(NewDto());

            Assert.Equal("Debut EP", result.ProjectTitle);
            Assert.Equal("Scumlight Records", result.ClientName);
            Assert.False(result.IsAllDay);
        }

        [Fact]
        public async Task Insert_WithoutTimes_ShouldBeAllDay()
        {
            var result = await _service.Insert(NewDto(start: null, end: null));

            Assert.True(result.IsAllDay);
        }

        [Theory]
        [InlineData("18:00", null)]
        [InlineData(null, "22:00")]
        [InlineData("22:00", "18:00")]
        [InlineData("18:00", "18:00")]
        public async Task Insert_ShouldRejectInvalidTimes(string? start, string? end)
        {
            var exception = await Assert.ThrowsAsync<BusinessRuleException>(() => _service.Insert(NewDto(start, end)));

            Assert.Equal("session.invalidTimes", exception.ErrorCode);
        }

        [Fact]
        public async Task Insert_ShouldRejectEndDateBeforeStartDate()
        {
            var dto = NewDto();
            dto.EndDate = dto.Date.AddDays(-1);

            var exception = await Assert.ThrowsAsync<BusinessRuleException>(() => _service.Insert(dto));

            Assert.Equal("session.invalidEndDate", exception.ErrorCode);
        }

        [Fact]
        public async Task Insert_WithEndDateOnTheSameDay_ShouldBeSingleDay()
        {
            var dto = NewDto();
            dto.EndDate = dto.Date;

            var result = await _service.Insert(dto);

            Assert.Null(result.EndDate);
        }

        [Fact]
        public async Task SetStatus_ShouldOnlyChangeTheStatus()
        {
            var dto = NewDto();
            dto.SongIds = [_song1.Id];
            var created = await _service.Insert(dto);

            var result = await _service.SetStatus(created.Id, new SessionStatusUpdateDto { Status = SessionStatus.Completed });

            Assert.Equal(SessionStatus.Completed, result.Status);
            Assert.Equal("Mixing", result.Title);
            Assert.Equal([_song1.Id], result.SongIds);
        }

        [Fact]
        public async Task Duplicate_ShouldCopyPlanButNotResults()
        {
            var dto = NewDto(start: null, end: null);
            dto.EndDate = dto.Date.AddDays(1); // two-day session
            dto.Summary = "Drums song 1 done";
            dto.SongIds = [_song1.Id];
            var source = await _service.Insert(dto);
            _context.SessionTasks.Add(new SessionTask { SessionId = source.Id, Text = "Mic check", IsDone = true });
            await _context.SaveChangesAsync();

            var newDates = new[] { new DateOnly(2026, 12, 1), new DateOnly(2026, 12, 3) };
            var copies = (await _service.Duplicate(source.Id, new SessionDuplicateDto { Dates = newDates.ToList() })).ToList();

            Assert.Equal(2, copies.Count);
            Assert.Equal(new DateOnly(2026, 12, 2), copies[0].EndDate); // keeps the length of two days
            Assert.All(copies, copy =>
            {
                Assert.Equal("Mixing", copy.Title);
                Assert.Equal(SessionStatus.Planned, copy.Status);
                Assert.Null(copy.Summary);
                Assert.Equal([_song1.Id], copy.SongIds);
                Assert.Equal(1, copy.TaskCount);
                Assert.Equal(0, copy.TasksDoneCount); // to-dos start open again
            });
        }

        [Fact]
        public async Task Insert_ShouldLinkInstrumentsOfTheProjectOnly()
        {
            var drums = new ProjectInstrument { Name = "Drums", Project = _project };
            var foreign = new ProjectInstrument { Name = "Foreign", Project = _otherProjectSong.Project };
            _context.ProjectInstruments.AddRange(drums, foreign);
            await _context.SaveChangesAsync();

            var dto = NewDto();
            dto.InstrumentIds = [drums.Id];
            var created = await _service.Insert(dto);
            var copy = Assert.Single(await _service.Duplicate(created.Id, new SessionDuplicateDto { Dates = [new DateOnly(2026, 12, 1)] }));

            Assert.Equal("Drums", Assert.Single(created.Instruments).Name);
            Assert.Equal("Drums", Assert.Single(copy.Instruments).Name);

            dto.InstrumentIds = [foreign.Id];
            var exception = await Assert.ThrowsAsync<BusinessRuleException>(() => _service.Insert(dto));
            Assert.Equal("session.instrumentNotInProject", exception.ErrorCode);
        }

        [Fact]
        public async Task Insert_ShouldRejectSongsOfAnotherProject()
        {
            var dto = NewDto();
            dto.SongIds = [_song1.Id, _otherProjectSong.Id];

            var exception = await Assert.ThrowsAsync<BusinessRuleException>(() => _service.Insert(dto));

            Assert.Equal("session.songNotInProject", exception.ErrorCode);
        }

        [Fact]
        public async Task Update_ShouldReplaceSongs()
        {
            var dto = NewDto();
            dto.SongIds = [_song1.Id];
            var created = await _service.Insert(dto);

            dto.SongIds = [_song2.Id];
            var updated = await _service.Update(created.Id, dto);

            Assert.Equal([_song2.Id], updated.SongIds);
        }
    }
}
