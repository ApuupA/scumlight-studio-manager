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
                new Repository<TimeEntry>(_context), new Repository<Project>(_context), new Repository<Session>(_context), new Repository<User>(_context),
                new Repository<StandardTask>(_context), mapper);
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

        [Fact]
        public async Task StandardTasks_ShouldBeAppended_AndAnHourIsAlwaysOneHour()
        {
            var service = new StandardTaskService(new Repository<StandardTask>(_context), new Repository<TimeEntry>(_context), TestHelper.CreateMapper());

            var day = await service.Insert(new StandardTaskCreateDto { Name = " Recording / Tag ", Unit = TaskUnit.Day, HoursPerUnit = 8 });
            var hour = await service.Insert(new StandardTaskCreateDto { Name = "Mixing / Stunde", Unit = TaskUnit.Hour, HoursPerUnit = 5 });

            Assert.Equal("Recording / Tag", day.Name);
            Assert.Equal(8, day.HoursPerUnit);
            Assert.Equal(1, hour.HoursPerUnit);
            Assert.Equal(new[] { 1, 2 }, (await service.GetAll()).Select(t => t.SortOrder));
        }

        [Fact]
        public async Task UsedStandardTask_ShouldNotBeDeleted_OnlyArchived()
        {
            var service = new StandardTaskService(new Repository<StandardTask>(_context), new Repository<TimeEntry>(_context), TestHelper.CreateMapper());
            var task = await service.Insert(new StandardTaskCreateDto { Name = "Recording / Tag", Unit = TaskUnit.Day, HoursPerUnit = 8 });
            var entry = Entry(8);
            entry.StandardTaskId = task.Id;
            var booked = await _timeEntryService.Insert(entry);

            var exception = await Assert.ThrowsAsync<BusinessRuleException>(() => service.Delete(task.Id));
            var archived = await service.Update(task.Id, new StandardTaskCreateDto { Name = task.Name, Unit = TaskUnit.Day, HoursPerUnit = 8, IsArchived = true });

            Assert.Equal("standardTask.inUse", exception.ErrorCode);
            Assert.True(archived.IsArchived);
            Assert.Equal("Recording / Tag", booked.StandardTaskName);
        }

        [Fact]
        public async Task TimeEntry_WithUnknownStandardTask_ShouldBeRejected()
        {
            var entry = Entry(2);
            entry.StandardTaskId = 999;

            var exception = await Assert.ThrowsAsync<BusinessRuleException>(() => _timeEntryService.Insert(entry));

            Assert.Equal("timeEntry.standardTaskNotFound", exception.ErrorCode);
        }

        [Theory]
        [InlineData("{YYYY}-", 18, 3, 2026, "2026-018")]
        [InlineData("RE-", 7, 4, 2026, "RE-0007")]
        [InlineData("", 1234, 3, 2027, "1234")]
        public void InvoiceNumber_ShouldUseYearPrefixAndDigits(string prefix, int number, int digits, int year, string expected)
        {
            Assert.Equal(expected, FinanceService.FormatInvoiceNumber(prefix, number, digits, year));
        }

        [Fact]
        public async Task BusinessSettings_ShouldDefaultToSmallBusiness_AndCleanTheInput()
        {
            var defaults = await _financeService.GetSettings();

            var saved = await _financeService.UpdateSettings(new BusinessSettingsUpdateDto
            {
                CompanyName = " Scumlight Studio ",
                Street = "  ",
                Iban = "de89 3704 0044 0532 0130 00",
                InvoiceNumberPrefix = "{YYYY}-",
                NextInvoiceNumber = 18,
                InvoiceNumberDigits = 3,
                IsSmallBusiness = true,
                PaymentTermDays = 14
            });

            Assert.True(defaults.IsSmallBusiness);
            Assert.Equal(14, defaults.PaymentTermDays);
            Assert.Equal("Scumlight Studio", saved.CompanyName);
            Assert.Null(saved.Street);
            Assert.Equal("DE89370400440532013000", saved.Iban);
            Assert.Equal($"{DateTime.Now.Year}-018", saved.NextInvoiceNumberPreview);
        }
    }
}
