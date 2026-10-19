using StudioManager.DataLayer.Context;
using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.ClientModels;
using StudioManager.DataLayer.Models.FinanceModels;
using StudioManager.DataLayer.Models.ProjectModels;
using StudioManager.RepositoryLayer.Repository;
using StudioManager.ServiceLayer.Exceptions;
using StudioManager.ServiceLayer.Service.FinanceServices;

namespace StudioManager.Tests.Unit
{
    /// <summary>
    /// Tests for quotes: draft, issue with own number range, answer, invoice from quote.
    /// </summary>
    public class QuoteTests
    {
        private readonly AppDbContext _context = TestHelper.CreateDbContext();
        private readonly QuoteService _quoteService;
        private readonly InvoiceService _invoiceService;
        private readonly Project _project;
        private readonly DateOnly _today = new(2026, 10, 19);

        public QuoteTests()
        {
            var mapper = TestHelper.CreateMapper();
            _quoteService = new QuoteService(new Repository<Quote>(_context), new Repository<Project>(_context), new Repository<BusinessSettings>(_context), mapper);
            _invoiceService = new InvoiceService(
                new Repository<Invoice>(_context), new Repository<Payment>(_context), new Repository<Project>(_context),
                new Repository<TimeEntry>(_context), new Repository<ProjectCostItem>(_context), new Repository<BusinessSettings>(_context),
                new Repository<Quote>(_context), mapper);

            _project = new Project { Title = "Album", Client = new Client { Name = "Label GmbH", Address = "Musterstr. 1" } };
            _context.Projects.Add(_project);
            _context.BusinessSettings.Add(new BusinessSettings
            {
                DefaultHourlyRate = 37.5m, CompanyName = "Scumlight Studio", Street = "Studioweg 1", PostalCode = "54321", City = "Rockstadt",
                TaxNumber = "12/345/67890", NextInvoiceNumber = 18
            });
            _context.SaveChanges();
        }

        private async Task<QuoteReadDto> IssuedQuote()
        {
            var draft = await _quoteService.CreateDraft(_project.Id);
            await _quoteService.UpdateDraft(draft.Id, new QuoteUpdateDto
            {
                QuoteDate = _today,
                RecipientName = draft.RecipientName,
                Lines = [new() { Description = "Recording / Tag", Quantity = 5, Unit = "Tage", UnitPrice = 300 }]
            });
            return await _quoteService.Issue(draft.Id, new InvoiceDateDto { Date = _today });
        }

        [Fact]
        public async Task FlatPriceProject_ShouldStartWithTheFlatPrice()
        {
            _project.PricingMode = PricingMode.FlatPrice;
            _project.FlatPrice = 1500;
            await _context.SaveChangesAsync();

            var draft = await _quoteService.CreateDraft(_project.Id);

            Assert.Equal("Pauschale: Album", Assert.Single(draft.Lines).Description);
            Assert.Equal(1500m, draft.GrossTotal);
            Assert.Equal("AN-2026-001", draft.Sender.NextQuoteNumberPreview.Replace(DateTime.Now.Year.ToString(), "2026"));
        }

        [Fact]
        public async Task Issue_ShouldUseTheQuoteNumberRange_AndTheValidity()
        {
            var quote = await IssuedQuote();

            Assert.Equal("AN-2026-001", quote.Number);
            Assert.Equal(QuoteStatus.Sent, quote.Status);
            Assert.Equal(new DateOnly(2026, 11, 18), quote.ValidUntil); // 30 days
            Assert.Equal(1500m, quote.GrossTotal);
            var settings = _context.BusinessSettings.Single();
            Assert.Equal(2, settings.NextQuoteNumber);
            Assert.Equal(18, settings.NextInvoiceNumber); // the invoice numbers are not touched
        }

        [Fact]
        public async Task EmptyDraft_ShouldNotBeIssued()
        {
            var draft = await _quoteService.CreateDraft(_project.Id);

            var exception = await Assert.ThrowsAsync<BusinessRuleException>(() => _quoteService.Issue(draft.Id, new InvoiceDateDto { Date = _today }));

            Assert.Equal("quote.noLines", exception.ErrorCode);
        }

        [Fact]
        public async Task Answer_CanOnlyBeSetForAnIssuedQuote()
        {
            var draft = await _quoteService.CreateDraft(_project.Id);
            var quote = await IssuedQuote();

            var onDraft = await Assert.ThrowsAsync<BusinessRuleException>(
                () => _quoteService.SetStatus(draft.Id, new QuoteStatusDto { Status = QuoteStatus.Accepted }));
            var accepted = await _quoteService.SetStatus(quote.Id, new QuoteStatusDto { Status = QuoteStatus.Accepted });

            Assert.Equal("quote.statusNotPossible", onDraft.ErrorCode);
            Assert.Equal(QuoteStatus.Accepted, accepted.Status);
        }

        [Fact]
        public async Task InvoiceFromQuote_ShouldCopyTheLines_AndCoverTheOpenHours()
        {
            _context.TimeEntries.Add(new TimeEntry { Date = _today, Hours = 8, Description = "Drums", IsBillable = true, ProjectId = _project.Id });
            await _context.SaveChangesAsync();
            var quote = await IssuedQuote();

            var invoice = await _invoiceService.CreateDraftFromQuote(quote.Id);
            var invoicedQuote = (await _quoteService.Get(quote.Id))!;

            Assert.Equal("Recording / Tag", Assert.Single(invoice.Lines).Description);
            Assert.Equal(1500m, invoice.GrossTotal);
            Assert.Equal("Gemäß Angebot AN-2026-001 vom 19.10.2026", invoice.IntroText);
            Assert.Equal(invoice.Id, _context.TimeEntries.Single().InvoiceId); // the hours count as invoiced
            Assert.Equal(QuoteStatus.Invoiced, invoicedQuote.Status);

            // Deleting the invoice draft: the quote can be invoiced again
            await _invoiceService.DeleteDraft(invoice.Id);
            Assert.Equal(QuoteStatus.Accepted, (await _quoteService.Get(quote.Id))!.Status);
            Assert.Null(_context.TimeEntries.Single().InvoiceId);
        }

        [Fact]
        public async Task SentQuote_AfterValidUntil_ShouldBeExpired()
        {
            var draft = await _quoteService.CreateDraft(_project.Id);
            await _quoteService.UpdateDraft(draft.Id, new QuoteUpdateDto
            {
                QuoteDate = _today,
                RecipientName = "Label GmbH",
                Lines = [new() { Description = "Mixing", Quantity = 10, Unit = "Std.", UnitPrice = 37.5m }]
            });

            var quote = await _quoteService.Issue(draft.Id, new InvoiceDateDto { Date = DateOnly.FromDateTime(DateTime.Now).AddDays(-40) });

            Assert.True(quote.IsExpired);
        }
    }
}
