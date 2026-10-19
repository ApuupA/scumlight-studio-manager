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
    /// Tests for invoices: draft from open hours, issue, lock, cancellation, payments.
    /// </summary>
    public class InvoiceTests
    {
        private readonly AppDbContext _context = TestHelper.CreateDbContext();
        private readonly InvoiceService _invoiceService;
        private readonly TimeEntryService _timeEntryService;
        private readonly FinanceService _financeService;
        private readonly Project _project;
        private readonly StandardTask _recordingDay;
        private readonly StandardTask _mixingHour;
        private readonly DateOnly _today = new(2026, 10, 19);

        public InvoiceTests()
        {
            var mapper = TestHelper.CreateMapper();
            _invoiceService = new InvoiceService(
                new Repository<Invoice>(_context), new Repository<Payment>(_context), new Repository<Project>(_context),
                new Repository<TimeEntry>(_context), new Repository<ProjectCostItem>(_context), new Repository<BusinessSettings>(_context), mapper);
            _timeEntryService = new TimeEntryService(
                new Repository<TimeEntry>(_context), new Repository<Project>(_context), new Repository<Session>(_context),
                new Repository<User>(_context), new Repository<StandardTask>(_context), mapper);
            _financeService = new FinanceService(
                new Repository<BusinessSettings>(_context), new Repository<Project>(_context), new Repository<TimeEntry>(_context),
                new Repository<ProjectCostItem>(_context), new Repository<Invoice>(_context), mapper);

            _project = new Project { Title = "Album", Client = new Client { Name = "Label GmbH", Address = "Musterstr. 1\n12345 Musterstadt" } };
            _recordingDay = new StandardTask { Name = "Recording / Tag", Unit = TaskUnit.Day, HoursPerUnit = 8, SortOrder = 1 };
            _mixingHour = new StandardTask { Name = "Mixing / Stunde", Unit = TaskUnit.Hour, HoursPerUnit = 1, SortOrder = 2 };
            _context.AddRange(_project, _recordingDay, _mixingHour);
            _context.BusinessSettings.Add(new BusinessSettings
            {
                DefaultHourlyRate = 37.5m,
                CompanyName = "Scumlight Studio",
                Street = "Studioweg 1",
                PostalCode = "54321",
                City = "Rockstadt",
                TaxNumber = "12/345/67890",
                NextInvoiceNumber = 18
            });
            _context.SaveChanges();
        }

        private void AddTime(decimal hours, StandardTask? task, bool billable = true)
        {
            _context.TimeEntries.Add(new TimeEntry
            {
                Date = _today, Hours = hours, Description = "note", IsBillable = billable, ProjectId = _project.Id, StandardTaskId = task?.Id
            });
            _context.SaveChanges();
        }

        private async Task<int> IssuedInvoice()
        {
            AddTime(8, _recordingDay);
            var draft = await _invoiceService.CreateDraft(_project.Id);
            return (await _invoiceService.Issue(draft.Id, new InvoiceDateDto { Date = _today })).Id;
        }

        [Fact]
        public async Task Draft_ShouldSumTheOpenHoursPerStandardTask_PlusExtras()
        {
            AddTime(8, _recordingDay);
            AddTime(8, _recordingDay);
            AddTime(3, _mixingHour);
            AddTime(1, null);                     // without task
            AddTime(2, _mixingHour, billable: false);
            _context.ProjectCostItems.Add(new ProjectCostItem { Date = _today, Description = "Tonband", Quantity = 2, UnitPrice = 45, ProjectId = _project.Id });
            await _context.SaveChangesAsync();

            var draft = await _invoiceService.CreateDraft(_project.Id);

            Assert.Equal(new[] { "Recording / Tag", "Mixing / Stunde", "Studioarbeit", "Tonband" }, draft.Lines.Select(l => l.Description));
            Assert.Equal((2m, "Tage", 300m, 600m), (draft.Lines[0].Quantity, draft.Lines[0].Unit, draft.Lines[0].UnitPrice, draft.Lines[0].Total));
            Assert.Equal(112.5m, draft.Lines[1].Total);  // 3 h × 37.50
            Assert.Equal(840m, draft.NetTotal);
            Assert.Equal(840m, draft.GrossTotal);       // small business: no VAT
            Assert.Equal("Label GmbH", draft.RecipientName);
            Assert.Equal(InvoicePaymentStatus.Draft, draft.PaymentStatus);
            Assert.Equal($"{DateTime.Now.Year}-018", draft.Sender.NextInvoiceNumberPreview); // the number it will get

            // Everything is reserved - a second draft finds nothing (only the non-billable time stays open)
            var second = await _invoiceService.CreateDraft(_project.Id);
            Assert.Empty(second.Lines);
            Assert.Single(_context.TimeEntries.Where(t => t.InvoiceId == null));
        }

        [Fact]
        public async Task Issue_ShouldTakeTheNextNumber_AndFreezeTheBusinessData()
        {
            var id = await IssuedInvoice();

            await _financeService.UpdateSettings(new BusinessSettingsUpdateDto { CompanyName = "New Name", InvoiceNumberPrefix = "{YYYY}-", NextInvoiceNumber = 19, InvoiceNumberDigits = 3 });
            var invoice = (await _invoiceService.Get(id))!;

            Assert.Equal("2026-018", invoice.Number);
            Assert.Equal(new DateOnly(2026, 11, 2), invoice.DueDate); // 14 days
            Assert.Equal("Scumlight Studio", invoice.Sender.CompanyName); // frozen
            Assert.Equal(19, _context.BusinessSettings.Single().NextInvoiceNumber);
            Assert.Equal(InvoicePaymentStatus.Open, invoice.PaymentStatus);
        }

        [Fact]
        public async Task Issue_WithoutBusinessData_ShouldBeRejected()
        {
            var settings = _context.BusinessSettings.Single();
            settings.TaxNumber = null;
            await _context.SaveChangesAsync();
            AddTime(8, _recordingDay);
            var draft = await _invoiceService.CreateDraft(_project.Id);

            var exception = await Assert.ThrowsAsync<BusinessRuleException>(() => _invoiceService.Issue(draft.Id, new InvoiceDateDto { Date = _today }));

            Assert.Equal("invoice.businessDataMissing", exception.ErrorCode);
        }

        [Fact]
        public async Task IssuedInvoice_AndItsTime_ShouldBeLocked()
        {
            var id = await IssuedInvoice();
            var entryId = _context.TimeEntries.Single().Id;

            var update = await Assert.ThrowsAsync<BusinessRuleException>(() => _invoiceService.UpdateDraft(id, new InvoiceUpdateDto { RecipientName = "X" }));
            var delete = await Assert.ThrowsAsync<BusinessRuleException>(() => _invoiceService.DeleteDraft(id));
            var deleteTime = await Assert.ThrowsAsync<BusinessRuleException>(() => _timeEntryService.Delete(entryId));

            Assert.Equal("invoice.notDraft", update.ErrorCode);
            Assert.Equal("invoice.notDraft", delete.ErrorCode);
            Assert.Equal("timeEntry.invoiced", deleteTime.ErrorCode);
        }

        [Fact]
        public async Task DeletingADraft_ShouldReleaseItsTime()
        {
            AddTime(8, _recordingDay);
            var draft = await _invoiceService.CreateDraft(_project.Id);

            await _invoiceService.DeleteDraft(draft.Id);

            Assert.Null(_context.TimeEntries.Single().InvoiceId);
            Assert.Empty(_context.Invoices);
        }

        [Fact]
        public async Task Cancel_ShouldCreateANegativeInvoice_AndReleaseTheTime()
        {
            var id = await IssuedInvoice();

            var cancellation = await _invoiceService.Cancel(id, new InvoiceDateDto { Date = _today });
            var original = (await _invoiceService.Get(id))!;

            Assert.Equal("2026-019", cancellation.Number);
            Assert.Equal(-300m, cancellation.GrossTotal);
            Assert.Equal(InvoicePaymentStatus.Cancellation, cancellation.PaymentStatus);
            Assert.Equal("2026-018", cancellation.CancelsInvoiceNumber);
            Assert.Equal(InvoicePaymentStatus.Cancelled, original.PaymentStatus);
            Assert.Equal("2026-019", original.CancelledByNumber);
            Assert.Null(_context.TimeEntries.Single().InvoiceId); // can be invoiced again
            Assert.Equal(0, (await _financeService.GetProjectFinance(_project.Id)).InvoicedTotal); // 300 - 300
        }

        [Fact]
        public async Task Payments_ShouldSetTheStatus()
        {
            var id = await IssuedInvoice();

            var partly = await _invoiceService.AddPayment(id, new PaymentCreateDto { Date = _today, Amount = 100 });
            var paid = await _invoiceService.AddPayment(id, new PaymentCreateDto { Date = _today, Amount = 200 });

            Assert.Equal(InvoicePaymentStatus.PartiallyPaid, partly.PaymentStatus);
            Assert.Equal(200m, partly.OpenAmount);
            Assert.Equal(InvoicePaymentStatus.Paid, paid.PaymentStatus);
            Assert.Equal(0m, paid.OpenAmount);
        }

        [Fact]
        public async Task UnpaidInvoice_AfterTheDueDate_ShouldBeOverdue()
        {
            AddTime(8, _recordingDay);
            var draft = await _invoiceService.CreateDraft(_project.Id);

            var invoice = await _invoiceService.Issue(draft.Id, new InvoiceDateDto { Date = DateOnly.FromDateTime(DateTime.Now).AddDays(-30) });

            Assert.Equal(InvoicePaymentStatus.Overdue, invoice.PaymentStatus);
        }

        [Fact]
        public async Task FlatPrice_ShouldBeInvoicedOnce_AndAgainAfterACancellation()
        {
            _project.PricingMode = PricingMode.FlatPrice;
            _project.FlatPrice = 1500;
            await _context.SaveChangesAsync();
            AddTime(20, _recordingDay); // hours are reserved, but not charged

            var first = await _invoiceService.CreateDraft(_project.Id);
            var issued = await _invoiceService.Issue(first.Id, new InvoiceDateDto { Date = _today });
            var second = await _invoiceService.CreateDraft(_project.Id);
            await _invoiceService.DeleteDraft(second.Id);
            await _invoiceService.Cancel(issued.Id, new InvoiceDateDto { Date = _today });
            var third = await _invoiceService.CreateDraft(_project.Id);

            Assert.Equal("Pauschale: Album", Assert.Single(first.Lines).Description);
            Assert.Equal(1500m, first.GrossTotal);
            Assert.Empty(second.Lines);
            Assert.Equal(1500m, Assert.Single(third.Lines).Total);
        }

        [Fact]
        public async Task WithVat_ShouldAddTheVat()
        {
            var settings = _context.BusinessSettings.Single();
            settings.IsSmallBusiness = false;
            settings.VatRate = 19;
            await _context.SaveChangesAsync();
            AddTime(8, _recordingDay);
            var draft = await _invoiceService.CreateDraft(_project.Id);

            var invoice = await _invoiceService.Issue(draft.Id, new InvoiceDateDto { Date = _today });

            Assert.Equal(300m, invoice.NetTotal);
            Assert.Equal(57m, invoice.VatAmount);
            Assert.Equal(357m, invoice.GrossTotal);
        }

        [Fact]
        public async Task UpdateDraft_ShouldReplaceTheLines()
        {
            AddTime(8, _recordingDay);
            var draft = await _invoiceService.CreateDraft(_project.Id);

            var updated = await _invoiceService.UpdateDraft(draft.Id, new InvoiceUpdateDto
            {
                InvoiceDate = _today,
                RecipientName = " Label GmbH ",
                Lines =
                [
                    new() { Description = "Recording / Tag", Quantity = 1, Unit = "Tage", UnitPrice = 300 },
                    new() { Description = "Rabatt", Quantity = 1, Unit = "pauschal", UnitPrice = -50 }
                ]
            });

            Assert.Equal(new[] { 300m, -50m }, updated.Lines.Select(l => l.Total));
            Assert.Equal(250m, updated.GrossTotal);
            Assert.Equal("Label GmbH", updated.RecipientName);
        }
    }
}
