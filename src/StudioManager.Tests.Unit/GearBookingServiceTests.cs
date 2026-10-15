using StudioManager.DataLayer.Context;
using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.Models.ClientModels;
using StudioManager.DataLayer.Models.GearModels;
using StudioManager.DataLayer.Models.ProjectModels;
using StudioManager.DataLayer.Models.SessionModels;
using StudioManager.RepositoryLayer.Repository;
using StudioManager.ServiceLayer.Service.SessionServices;

namespace StudioManager.Tests.Unit
{
    public class GearBookingServiceTests
    {
        private static readonly DateOnly Day = new(2026, 11, 6);

        private readonly AppDbContext _context = TestHelper.CreateDbContext();
        private readonly GearBookingService _service;
        private readonly Project _project;
        private readonly GearItem _u87;
        private readonly GearItem _xlrCables;

        public GearBookingServiceTests()
        {
            _service = new GearBookingService(
                new Repository<GearBooking>(_context),
                new Repository<Session>(_context),
                new Repository<GearItem>(_context),
                TestHelper.CreateMapper());

            var category = new GearCategory { Name = "Microphones" };
            _u87 = new GearItem { Name = "U87", Quantity = 1, GearCategory = category };
            _xlrCables = new GearItem { Name = "XLR cable 5m", Quantity = 20, GearCategory = category };
            _project = new Project { Title = "Debut EP", Client = new Client { Name = "Scumlight Records" } };
            _context.AddRange(_u87, _xlrCables, _project);
            _context.SaveChanges();
        }

        private Session AddSession(string title, string? start = null, string? end = null, SessionStatus status = SessionStatus.Planned)
        {
            var session = new Session
            {
                Title = title,
                Date = Day,
                StartTime = start == null ? null : TimeOnly.Parse(start),
                EndTime = end == null ? null : TimeOnly.Parse(end),
                Status = status,
                Project = _project
            };
            _context.Sessions.Add(session);
            _context.SaveChanges();
            return session;
        }

        private Task<DataLayer.DTO.Read.GearBookingReadDto> Book(Session session, GearItem item, int quantity = 1) =>
            _service.Insert(new GearBookingCreateDto { SessionId = session.Id, GearItemId = item.Id, Quantity = quantity });

        [Theory]
        [InlineData(null, null, "10:00", "12:00", true)]       // all-day overlaps everything
        [InlineData("10:00", "14:00", "13:00", "18:00", true)] // partly overlapping
        [InlineData("10:00", "14:00", "14:00", "18:00", false)] // back to back is fine
        [InlineData("18:00", "22:00", "10:00", "17:00", false)]
        public void SessionsOverlap_ShouldCompareTimesOnTheSameDay(string? startA, string? endA, string? startB, string? endB, bool expected)
        {
            var a = new Session { Date = Day, StartTime = Parse(startA), EndTime = Parse(endA) };
            var b = new Session { Date = Day, StartTime = Parse(startB), EndTime = Parse(endB) };

            Assert.Equal(expected, GearBookingService.SessionsOverlap(a, b));
        }

        [Fact]
        public void SessionsOverlap_ShouldBeFalse_OnDifferentDays()
        {
            var a = new Session { Date = Day };
            var b = new Session { Date = Day.AddDays(1) };

            Assert.False(GearBookingService.SessionsOverlap(a, b));
        }

        [Fact]
        public async Task Insert_ShouldWarn_WhenSingleItemIsBookedTwiceAtTheSameTime()
        {
            var drums = AddSession("Drums", "10:00", "18:00");
            var vocals = AddSession("Vocals", "14:00", "20:00");
            await Book(drums, _u87);

            var result = await Book(vocals, _u87);

            Assert.True(result.IsOverbooked);
            Assert.Equal(2, result.TotalBookedQuantity);
            Assert.Equal("Drums", Assert.Single(result.ConflictingSessions).Title);
        }

        [Fact]
        public async Task Insert_ShouldNotWarn_WhenSessionsDoNotOverlap()
        {
            var recording = AddSession("Recording", "10:00", "17:00");
            var mixing = AddSession("Mixing", "18:00", "22:00");
            await Book(recording, _u87);

            var result = await Book(mixing, _u87);

            Assert.False(result.IsOverbooked);
            Assert.Empty(result.ConflictingSessions);
        }

        [Fact]
        public async Task Insert_ShouldCountPieces_ForBulkItems()
        {
            var drums = AddSession("Drums");
            var guitars = AddSession("Guitars");
            await Book(drums, _xlrCables, 12);

            var withinStock = await Book(guitars, _xlrCables, 8);
            Assert.False(withinStock.IsOverbooked);
            Assert.Equal(20, withinStock.TotalBookedQuantity);

            var tooMany = await Book(guitars, _xlrCables, 1); // added to the existing booking -> 9 + 12 = 21
            Assert.True(tooMany.IsOverbooked);
            Assert.Equal(21, tooMany.TotalBookedQuantity);
        }

        [Fact]
        public async Task Insert_ShouldIgnoreCancelledSessions()
        {
            var cancelled = AddSession("Cancelled", status: SessionStatus.Cancelled);
            var drums = AddSession("Drums");
            await Book(cancelled, _u87);

            var result = await Book(drums, _u87);

            Assert.False(result.IsOverbooked);
        }

        [Fact]
        public async Task Insert_ShouldAddQuantity_WhenGearIsAlreadyBookedInTheSession()
        {
            var drums = AddSession("Drums");
            await Book(drums, _xlrCables, 5);

            var result = await Book(drums, _xlrCables, 3);

            Assert.Equal(8, result.Quantity);
            Assert.Single(_context.GearBookings);
        }

        [Fact]
        public async Task Insert_ShouldStillSave_WhenOverbooked()
        {
            var a = AddSession("A");
            var b = AddSession("B");
            await Book(a, _u87);

            await Book(b, _u87);

            Assert.Equal(2, _context.GearBookings.Count()); // warning only, never blocked
        }

        private static TimeOnly? Parse(string? time) => time == null ? null : TimeOnly.Parse(time);
    }
}
