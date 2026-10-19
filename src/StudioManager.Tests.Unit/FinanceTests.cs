using StudioManager.DataLayer.Context;
using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.Models.ClientModels;
using StudioManager.DataLayer.Models.FinanceModels;
using StudioManager.DataLayer.Models.ProjectModels;
using StudioManager.DataLayer.Models.SessionModels;
using StudioManager.DataLayer.Models.UserModels;
using StudioManager.RepositoryLayer.Repository;
using StudioManager.ServiceLayer.Exceptions;
using StudioManager.ServiceLayer.Service.FinanceServices;

namespace StudioManager.Tests.Unit
{
    /// <summary>
    /// Tests for time tracking, extra items and the money view of a project.
    /// </summary>
    public class FinanceTests
    {
        private readonly AppDbContext _context = TestHelper.CreateDbContext();
        private readonly TimeEntryService _timeEntryService;
        private readonly ProjectCostItemService _costItemService;
        private readonly FinanceService _financeService;
        private readonly Project _project;
        private readonly User _user;
        private readonly DateOnly _today = DateOnly.FromDateTime(DateTime.Now);

        public FinanceTests()
        {
            var mapper = TestHelper.CreateMapper();
            _timeEntryService = new TimeEntryService(
                new Repository<TimeEntry>(_context), new Repository<Project>(_context), new Repository<Session>(_context), new Repository<User>(_context), mapper);
            _costItemService = new ProjectCostItemService(new Repository<ProjectCostItem>(_context), mapper);
            _financeService = new FinanceService(
                new Repository<BusinessSettings>(_context), new Repository<Project>(_context), new Repository<TimeEntry>(_context),
                new Repository<ProjectCostItem>(_context), mapper);

            _project = new Project { Title = "Album", Client = new Client { Name = "Label" } };
            _user = new User { Email = "owner@studio.local", DisplayName = "Owner" };
            _context.AddRange(_project, _user);
            _context.SaveChanges();
        }

        private TimeEntryCreateDto Entry(decimal hours, bool billable = true, int? sessionId = null) => new()
        {
            Date = _today, Hours = hours, Description = "Recording", IsBillable = billable, ProjectId = _project.Id, SessionId = sessionId, UserId = _user.Id
        };

        [Fact]
        public async Task HourlyProject_ShouldBeWorthTheBillableHoursTimesTheRate_PlusExtras()
        {
            await _financeService.UpdateSettings(new BusinessSettingsUpdateDto { DefaultHourlyRate = 40 });
            await _timeEntryService.Insert(Entry(6));
            await _timeEntryService.Insert(Entry(2.5m));
            await _timeEntryService.Insert(Entry(1, billable: false)); // own preparation
            await _costItemService.Insert(new ProjectCostItemCreateDto { Date = _today, Description = "Tape", Quantity = 2, UnitPrice = 45, ProjectId = _project.Id });
            await _costItemService.Insert(new ProjectCostItemCreateDto { Date = _today, Description = "Coffee", Quantity = 1, UnitPrice = 10, IsBillable = false, ProjectId = _project.Id });

            var finance = await _financeService.GetProjectFinance(_project.Id);

            Assert.Equal(9.5m, finance.TotalHours);
            Assert.Equal(8.5m, finance.BillableHours);
            Assert.Equal(40, finance.EffectiveHourlyRate);   // default rate
            Assert.Equal(340, finance.LaborValue);           // 8.5 h × 40
            Assert.Equal(90, finance.ExtrasValue);           // only the billable tape
            Assert.Equal(430, finance.TotalValue);
            Assert.Null(finance.ActualHourlyRate);
        }

        [Fact]
        public async Task FlatPriceProject_ShouldShowWhatAnHourActuallyEarns()
        {
            await _timeEntryService.Insert(Entry(8));
            await _timeEntryService.Insert(Entry(2, billable: false));

            var finance = await _financeService.SetPricing(_project.Id, new ProjectPricingDto { PricingMode = PricingMode.FlatPrice, FlatPrice = 500 });

            Assert.Equal(500, finance.LaborValue);
            Assert.Equal(50, finance.ActualHourlyRate); // 500 / all 10 hours
        }

        [Fact]
        public async Task OwnHourlyRate_ShouldWinOverTheDefaultRate()
        {
            await _financeService.UpdateSettings(new BusinessSettingsUpdateDto { DefaultHourlyRate = 40 });
            await _timeEntryService.Insert(Entry(2));

            var finance = await _financeService.SetPricing(_project.Id, new ProjectPricingDto { PricingMode = PricingMode.Hourly, HourlyRate = 55 });

            Assert.Equal(55, finance.EffectiveHourlyRate);
            Assert.Equal(110, finance.LaborValue);
        }

        [Fact]
        public async Task FlatPrice_WithoutAmount_ShouldBeRejected()
        {
            var exception = await Assert.ThrowsAsync<BusinessRuleException>(
                () => _financeService.SetPricing(_project.Id, new ProjectPricingDto { PricingMode = PricingMode.FlatPrice }));

            Assert.Equal("projectPricing.flatPriceRequired", exception.ErrorCode);
        }

        [Fact]
        public async Task TimeEntry_WithSessionOfAnotherProject_ShouldBeRejected()
        {
            var otherSession = new Session { Title = "Other", Date = _today, Project = new Project { Title = "Other", ClientId = _project.ClientId } };
            _context.Sessions.Add(otherSession);
            await _context.SaveChangesAsync();

            var exception = await Assert.ThrowsAsync<BusinessRuleException>(() => _timeEntryService.Insert(Entry(3, sessionId: otherSession.Id)));

            Assert.Equal("timeEntry.sessionNotInProject", exception.ErrorCode);
        }

        [Fact]
        public async Task Timer_ShouldRunOncePerUser_AndBookTheTimeWhenStopped()
        {
            var started = await _timeEntryService.StartTimer(_user.Id, new TimerStartDto { ProjectId = _project.Id, Date = _today });
            var second = await Assert.ThrowsAsync<BusinessRuleException>(
                () => _timeEntryService.StartTimer(_user.Id, new TimerStartDto { ProjectId = _project.Id, Date = _today }));

            // Running timers do not count yet
            Assert.Equal(0, (await _financeService.GetProjectFinance(_project.Id)).TotalHours);

            var stopped = await _timeEntryService.StopTimer(_user.Id, new TimerStopDto { Description = " Editing " });

            Assert.True(started.IsRunning);
            Assert.Equal("timeEntry.timerRunning", second.ErrorCode);
            Assert.False(stopped.IsRunning);
            Assert.Equal(0.25m, stopped.Hours);            // shortest booking: a quarter hour
            Assert.Equal("Editing", stopped.Description);
            Assert.Null(await _timeEntryService.GetRunningTimer(_user.Id));
        }

        [Theory]
        [InlineData(1, 0.25)]
        [InlineData(15, 0.25)]
        [InlineData(16, 0.5)]
        [InlineData(65, 1.25)]
        [InlineData(3000, 24)] // a forgotten timer is cut at one day
        public void Timer_ShouldRoundUpToTheQuarterHour(int minutes, decimal expectedHours)
        {
            Assert.Equal(expectedHours, TimeEntryService.RoundUpToQuarterHour(TimeSpan.FromMinutes(minutes)));
        }
    }
}
