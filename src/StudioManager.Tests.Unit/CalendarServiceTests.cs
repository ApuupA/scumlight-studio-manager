using StudioManager.DataLayer.Context;
using StudioManager.DataLayer.Models.ClientModels;
using StudioManager.DataLayer.Models.ProjectModels;
using StudioManager.DataLayer.Models.SessionModels;
using StudioManager.DataLayer.Models.UserModels;
using StudioManager.RepositoryLayer.Repository;
using StudioManager.ServiceLayer.Service.CalendarServices;

namespace StudioManager.Tests.Unit
{
    /// <summary>
    /// Tests for the calendar subscription (iCalendar feed).
    /// </summary>
    public class CalendarServiceTests
    {
        private readonly AppDbContext _context = TestHelper.CreateDbContext();
        private readonly CalendarService _service;
        private readonly User _user;
        private readonly Project _project;
        private readonly DateOnly _nextWeek = DateOnly.FromDateTime(DateTime.Now).AddDays(7);

        public CalendarServiceTests()
        {
            _service = new CalendarService(new Repository<User>(_context), new Repository<Session>(_context));

            _user = new User { Email = "owner@studio.local", DisplayName = "Owner" };
            _project = new Project { Title = "Album", Client = new Client { Name = "Label" } };
            _context.Users.Add(_user);
            _context.Projects.Add(_project);
            _context.SaveChanges();
        }

        private async Task<string> Feed()
        {
            var token = (await _service.CreateToken(_user.Id)).Token!;
            return (await _service.GetFeed(token))!;
        }

        private void AddSession(Session session)
        {
            session.Project = _project;
            _context.Sessions.Add(session);
            _context.SaveChanges();
        }

        [Fact]
        public async Task Feed_ShouldContainATimedSession_WithItsTimes()
        {
            AddSession(new Session { Title = "Drums", Date = _nextWeek, StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(17, 30), Status = SessionStatus.Confirmed });

            var feed = await Feed();

            Assert.StartsWith("BEGIN:VCALENDAR\r\n", feed);
            Assert.Contains("SUMMARY:Drums · Album\r\n", feed);
            Assert.Contains($"DTSTART:{_nextWeek:yyyyMMdd}T090000\r\n", feed);
            Assert.Contains($"DTEND:{_nextWeek:yyyyMMdd}T173000\r\n", feed);
            Assert.Contains("STATUS:CONFIRMED", feed);
            Assert.DoesNotContain("RRULE", feed);
        }

        [Fact]
        public async Task Feed_ShouldRepeatATimedMultiDaySession_EveryDay()
        {
            AddSession(new Session { Title = "Album", Date = _nextWeek, EndDate = _nextWeek.AddDays(2), StartTime = new TimeOnly(10, 0), EndTime = new TimeOnly(18, 0) });

            var feed = await Feed();

            Assert.Contains("RRULE:FREQ=DAILY;COUNT=3", feed);
            Assert.Contains("STATUS:TENTATIVE", feed); // planned
        }

        [Fact]
        public async Task Feed_AllDaySession_ShouldEndTheDayAfterTheLastDay()
        {
            AddSession(new Session { Title = "Mix", Date = _nextWeek, EndDate = _nextWeek.AddDays(1), Status = SessionStatus.Cancelled });

            var feed = await Feed();

            Assert.Contains($"DTSTART;VALUE=DATE:{_nextWeek:yyyyMMdd}", feed);
            Assert.Contains($"DTEND;VALUE=DATE:{_nextWeek.AddDays(2):yyyyMMdd}", feed);
            Assert.Contains("STATUS:CANCELLED", feed);
        }

        [Fact]
        public async Task Feed_ShouldEscapeTexts_AndFoldLongLines()
        {
            AddSession(new Session
            {
                Title = "Vocals; take 2, final",
                Date = _nextWeek,
                Notes = "Bring the U87\nand a pop filter - a long note that is certainly longer than seventy-five bytes"
            });

            var feed = await Feed();

            Assert.Contains("SUMMARY:Vocals\\; take 2\\, final · Album", feed);
            Assert.Contains("Bring the U87\\nand a pop filter", feed.Replace("\r\n ", ""));
            Assert.All(feed.Split("\r\n"), line => Assert.True(System.Text.Encoding.UTF8.GetByteCount(line) <= 75, line));
        }

        [Fact]
        public async Task Feed_ShouldLeaveOutOldSessions()
        {
            AddSession(new Session { Title = "Long ago", Date = DateOnly.FromDateTime(DateTime.Now).AddDays(-CalendarService.DaysInThePast - 10) });

            var feed = await Feed();

            Assert.DoesNotContain("Long ago", feed);
        }

        [Fact]
        public async Task NewToken_ShouldMakeTheOldLinkInvalid()
        {
            var oldToken = (await _service.CreateToken(_user.Id)).Token!;
            var newToken = (await _service.CreateToken(_user.Id)).Token!;

            Assert.NotEqual(oldToken, newToken);
            Assert.Null(await _service.GetFeed(oldToken));
            Assert.NotNull(await _service.GetFeed(newToken));
            Assert.Null(await _service.GetFeed(""));
        }
    }
}
