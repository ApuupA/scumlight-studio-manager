using AutoMapper;
using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.FinanceModels;
using StudioManager.DataLayer.Models.ProjectModels;
using StudioManager.RepositoryLayer.IRepository;
using StudioManager.ServiceLayer.Exceptions;
using StudioManager.ServiceLayer.IService.IFinanceServices;
using System.Linq.Expressions;
using System.Text.Json;

namespace StudioManager.ServiceLayer.Service.FinanceServices
{
    /// <summary>
    /// Invoices. The lines are generated in German ("Tage", "Std.") - the studio invoices in German.
    /// </summary>
    public class InvoiceService : IInvoiceService
    {
        private const string InvoicePaths = "Lines,Payments,Project,Client,CancelsInvoice";

        private readonly IRepository<Invoice> _invoiceRepository;
        private readonly IRepository<Payment> _paymentRepository;
        private readonly IRepository<Project> _projectRepository;
        private readonly IRepository<TimeEntry> _timeEntryRepository;
        private readonly IRepository<ProjectCostItem> _costItemRepository;
        private readonly IRepository<BusinessSettings> _settingsRepository;
        private readonly IRepository<Quote> _quoteRepository;
        private readonly IMapper _mapper;

        public InvoiceService(
            IRepository<Invoice> invoiceRepository,
            IRepository<Payment> paymentRepository,
            IRepository<Project> projectRepository,
            IRepository<TimeEntry> timeEntryRepository,
            IRepository<ProjectCostItem> costItemRepository,
            IRepository<BusinessSettings> settingsRepository,
            IRepository<Quote> quoteRepository,
            IMapper mapper)
        {
            _quoteRepository = quoteRepository;
            _invoiceRepository = invoiceRepository;
            _paymentRepository = paymentRepository;
            _projectRepository = projectRepository;
            _timeEntryRepository = timeEntryRepository;
            _costItemRepository = costItemRepository;
            _settingsRepository = settingsRepository;
            _mapper = mapper;
        }

        // ---------- Reading ----------

        public async Task<IEnumerable<InvoiceReadDto>> GetAll() => await LoadWhere(i => true);

        public async Task<IEnumerable<InvoiceReadDto>> GetForProject(int projectId) => await LoadWhere(i => i.ProjectId == projectId);

        public async Task<InvoiceReadDto?> Get(int id) => (await LoadWhere(i => i.Id == id)).SingleOrDefault();

        // ---------- Draft ----------

        public async Task<InvoiceReadDto> CreateDraft(int projectId)
        {
            var project = await _projectRepository.GetAsync(p => p.Id == projectId, p => p.Client!)
                ?? throw new EntityNotFoundException(nameof(Project), projectId);
            var settings = await FinanceService.LoadSettings(_settingsRepository);
            var rate = project.HourlyRate ?? settings.DefaultHourlyRate;

            // Everything billable that is not on an invoice yet (running timers have no hours yet)
            var entries = (await _timeEntryRepository.GetAllWithPathsAsync(
                    t => t.ProjectId == projectId && t.IsBillable && t.InvoiceId == null && t.StartedAt == null, "StandardTask"))
                .ToList();
            var costItems = (await _costItemRepository.GetAllAsync(c => c.ProjectId == projectId && c.IsBillable && c.InvoiceId == null))
                .ToList();

            // The flat price is invoiced only once - unless that invoice was cancelled
            var flatPriceInvoiced = await _invoiceRepository.ExistsAsync(i =>
                i.ProjectId == projectId && i.IncludesFlatPrice && i.Status != InvoiceStatus.Cancelled && i.CancelsInvoiceId == null);
            var addFlatPrice = project.PricingMode == PricingMode.FlatPrice && !flatPriceInvoiced;

            var invoice = new Invoice
            {
                ProjectId = project.Id,
                ClientId = project.ClientId,
                InvoiceDate = DateOnly.FromDateTime(DateTime.Now),
                RecipientName = project.Client?.Name ?? string.Empty,
                RecipientAddress = project.Client?.Address,
                ServiceFrom = entries.Select(e => (DateOnly?)e.Date).Concat(costItems.Select(c => (DateOnly?)c.Date)).Min(),
                ServiceTo = entries.Select(e => (DateOnly?)e.Date).Concat(costItems.Select(c => (DateOnly?)c.Date)).Max(),
                IntroText = $"Projekt: {project.Title}",
                IncludesFlatPrice = addFlatPrice,
                Lines = BuildLines(project, rate, entries, costItems, addFlatPrice)
            };
            await _invoiceRepository.AddAsync(invoice);

            // Reserve the entries for this invoice, so they are not invoiced twice
            foreach (var entry in entries)
            {
                entry.InvoiceId = invoice.Id;
            }
            foreach (var item in costItems)
            {
                item.InvoiceId = invoice.Id;
            }
            await _timeEntryRepository.UpdateRangeAsync(entries);
            await _costItemRepository.UpdateRangeAsync(costItems);

            return (await Get(invoice.Id))!;
        }

        public async Task<InvoiceReadDto> CreateDraftFromQuote(int quoteId)
        {
            var quote = (await _quoteRepository.GetAllWithPathsAsync(q => q.Id == quoteId, "Lines", "Project")).SingleOrDefault()
                ?? throw new EntityNotFoundException(nameof(Quote), quoteId);

            if (quote.Status != QuoteStatus.Sent && quote.Status != QuoteStatus.Accepted)
            {
                throw new BusinessRuleException("quote.cannotBeInvoiced", "Only a sent or accepted quote can become an invoice.");
            }

            var project = quote.Project!;

            // The quote's price covers the project's work - its open hours and extra items count as invoiced
            var entries = (await _timeEntryRepository.GetAllAsync(t => t.ProjectId == project.Id && t.IsBillable && t.InvoiceId == null && t.StartedAt == null)).ToList();
            var costItems = (await _costItemRepository.GetAllAsync(c => c.ProjectId == project.Id && c.IsBillable && c.InvoiceId == null)).ToList();

            var invoice = new Invoice
            {
                ProjectId = quote.ProjectId,
                ClientId = quote.ClientId,
                InvoiceDate = DateOnly.FromDateTime(DateTime.Now),
                RecipientName = quote.RecipientName,
                RecipientAddress = quote.RecipientAddress,
                ServiceFrom = entries.Select(e => (DateOnly?)e.Date).Min(),
                ServiceTo = entries.Select(e => (DateOnly?)e.Date).Max(),
                IntroText = $"Gemäß Angebot {quote.Number} vom {quote.QuoteDate:dd.MM.yyyy}",
                IncludesFlatPrice = project.PricingMode == PricingMode.FlatPrice,
                Lines = quote.Lines
                    .OrderBy(l => l.SortOrder)
                    .Select(l => Line(l.Description, l.Quantity, l.Unit, l.UnitPrice, l.SortOrder))
                    .ToList()
            };
            await _invoiceRepository.AddAsync(invoice);

            foreach (var entry in entries)
            {
                entry.InvoiceId = invoice.Id;
            }
            foreach (var item in costItems)
            {
                item.InvoiceId = invoice.Id;
            }
            await _timeEntryRepository.UpdateRangeAsync(entries);
            await _costItemRepository.UpdateRangeAsync(costItems);

            quote.Status = QuoteStatus.Invoiced;
            quote.InvoiceId = invoice.Id;
            await _quoteRepository.UpdateAsync(quote);

            return (await Get(invoice.Id))!;
        }

        public async Task<InvoiceReadDto> UpdateDraft(int id, InvoiceUpdateDto dto)
        {
            var invoice = await LoadDraft(id);

            invoice.InvoiceDate = dto.InvoiceDate;
            invoice.ServiceFrom = dto.ServiceFrom;
            invoice.ServiceTo = dto.ServiceTo;
            invoice.RecipientName = dto.RecipientName.Trim();
            invoice.RecipientAddress = Clean(dto.RecipientAddress);
            invoice.IntroText = Clean(dto.IntroText);
            invoice.ClosingText = Clean(dto.ClosingText);

            // The lines are replaced (EF deletes the removed ones)
            invoice.Lines.Clear();
            for (var i = 0; i < dto.Lines.Count; i++)
            {
                var line = dto.Lines[i];
                invoice.Lines.Add(Line(line.Description.Trim(), line.Quantity, Clean(line.Unit), line.UnitPrice, i + 1));
            }

            await _invoiceRepository.UpdateAsync(invoice);
            return (await Get(id))!;
        }

        public async Task DeleteDraft(int id)
        {
            var invoice = await LoadDraft(id);

            await ReleaseEntries(invoice.Id);
            await _invoiceRepository.DeleteAsync(invoice.Id);
        }

        // ---------- Issue, cancel ----------

        public async Task<InvoiceReadDto> Issue(int id, InvoiceDateDto dto)
        {
            var invoice = await LoadDraft(id);
            var settings = await FinanceService.LoadSettings(_settingsRepository);

            if (invoice.Lines.Count == 0)
            {
                throw new BusinessRuleException("invoice.noLines", "The invoice has no lines.");
            }

            if (string.IsNullOrWhiteSpace(invoice.RecipientName))
            {
                throw new BusinessRuleException("invoice.recipientMissing", "The invoice needs a recipient.");
            }

            if (!FinanceService.HasRequiredBusinessData(settings))
            {
                throw new BusinessRuleException("invoice.businessDataMissing",
                    "Fill in the business data first (profile page): name, address and tax number.");
            }

            invoice.InvoiceDate = dto.Date;
            invoice.DueDate = dto.Date.AddDays(settings.PaymentTermDays);
            invoice.Number = TakeNextNumber(settings, dto.Date.Year);
            Freeze(invoice, settings);
            invoice.Status = InvoiceStatus.Issued;

            await _settingsRepository.UpdateAsync(settings);
            await _invoiceRepository.UpdateAsync(invoice);

            return (await Get(id))!;
        }

        public async Task<InvoiceReadDto> Cancel(int id, InvoiceDateDto dto)
        {
            var invoice = await LoadInvoice(id);
            if (invoice.Status != InvoiceStatus.Issued || invoice.CancelsInvoiceId != null)
            {
                throw new BusinessRuleException("invoice.cannotBeCancelled", "Only issued invoices can be cancelled.");
            }

            var settings = await FinanceService.LoadSettings(_settingsRepository);

            // The cancellation invoice: the same lines with negative quantities, own number
            var cancellation = new Invoice
            {
                ProjectId = invoice.ProjectId,
                ClientId = invoice.ClientId,
                InvoiceDate = dto.Date,
                ServiceFrom = invoice.ServiceFrom,
                ServiceTo = invoice.ServiceTo,
                RecipientName = invoice.RecipientName,
                RecipientAddress = invoice.RecipientAddress,
                IntroText = $"Stornorechnung zu Rechnung {invoice.Number} vom {invoice.InvoiceDate:dd.MM.yyyy}",
                CancelsInvoiceId = invoice.Id,
                Status = InvoiceStatus.Issued,
                Number = TakeNextNumber(settings, dto.Date.Year),
                Lines = invoice.Lines
                    .OrderBy(l => l.SortOrder)
                    .Select(l => Line(l.Description, -l.Quantity, l.Unit, l.UnitPrice, l.SortOrder))
                    .ToList()
            };
            Freeze(cancellation, settings);
            // The cancellation keeps the tax treatment of the original invoice
            cancellation.IsSmallBusiness = invoice.IsSmallBusiness;
            cancellation.VatRate = invoice.VatRate;
            SetTotals(cancellation);

            invoice.Status = InvoiceStatus.Cancelled;

            await _invoiceRepository.AddAsync(cancellation);
            await _settingsRepository.UpdateAsync(settings);
            await _invoiceRepository.UpdateAsync(invoice);

            // The hours and extra items can be invoiced again
            await ReleaseEntries(invoice.Id);

            return (await Get(cancellation.Id))!;
        }

        // ---------- Payments ----------

        public async Task<InvoiceReadDto> AddPayment(int invoiceId, PaymentCreateDto dto)
        {
            var invoice = await LoadInvoice(invoiceId);
            if (invoice.Status != InvoiceStatus.Issued || invoice.CancelsInvoiceId != null)
            {
                throw new BusinessRuleException("invoice.noPaymentPossible", "Payments can only be recorded for issued invoices.");
            }

            await _paymentRepository.AddAsync(new Payment
            {
                InvoiceId = invoiceId,
                Date = dto.Date,
                Amount = dto.Amount,
                Note = Clean(dto.Note)
            });

            return (await Get(invoiceId))!;
        }

        public async Task<InvoiceReadDto> DeletePayment(int invoiceId, int paymentId)
        {
            var payment = await _paymentRepository.GetAsync(p => p.Id == paymentId && p.InvoiceId == invoiceId)
                ?? throw new EntityNotFoundException(nameof(Payment), paymentId);

            await _paymentRepository.DeleteAsync(payment.Id);
            return (await Get(invoiceId))!;
        }

        // ---------- Helpers ----------

        /// <summary>
        /// The lines of a new draft: hours per standard task (a day task in days), the flat price, the extra items.
        /// </summary>
        private static List<InvoiceLine> BuildLines(Project project, decimal rate, List<TimeEntry> entries, List<ProjectCostItem> costItems, bool addFlatPrice)
        {
            var lines = new List<InvoiceLine>();

            if (project.PricingMode == PricingMode.FlatPrice)
            {
                // With a flat price the hours are not charged - they are only reserved, so they count as invoiced
                if (addFlatPrice)
                {
                    lines.Add(Line($"Pauschale: {project.Title}", 1, "pauschal", project.FlatPrice ?? 0, 0));
                }
            }
            else
            {
                var groups = entries
                    .GroupBy(e => e.StandardTaskId)
                    .OrderBy(g => g.First().StandardTask?.SortOrder ?? int.MaxValue);

                foreach (var group in groups)
                {
                    var hours = group.Sum(e => e.Hours);
                    var task = group.First().StandardTask;

                    if (task == null)
                    {
                        lines.Add(Line("Studioarbeit", hours, "Std.", rate, 0));
                    }
                    else if (task.Unit == TaskUnit.Day)
                    {
                        lines.Add(Line(task.Name, Math.Round(hours / task.HoursPerUnit, 2), "Tage", Math.Round(task.HoursPerUnit * rate, 2), 0));
                    }
                    else
                    {
                        lines.Add(Line(task.Name, hours, "Std.", rate, 0));
                    }
                }
            }

            foreach (var item in costItems.OrderBy(c => c.Date).ThenBy(c => c.Id))
            {
                lines.Add(Line(item.Description, item.Quantity, "Stk.", item.UnitPrice, 0));
            }

            for (var i = 0; i < lines.Count; i++)
            {
                lines[i].SortOrder = i + 1;
            }

            return lines;
        }

        private static InvoiceLine Line(string description, decimal quantity, string? unit, decimal unitPrice, int sortOrder) => new()
        {
            Description = description,
            Quantity = quantity,
            Unit = unit,
            UnitPrice = unitPrice,
            Total = Math.Round(quantity * unitPrice, 2),
            SortOrder = sortOrder
        };

        /// <summary>
        /// Takes the next invoice number of the number range and counts it up.
        /// </summary>
        private static string TakeNextNumber(BusinessSettings settings, int year)
        {
            var number = FinanceService.FormatInvoiceNumber(settings.InvoiceNumberPrefix, settings.NextInvoiceNumber, settings.InvoiceNumberDigits, year);
            settings.NextInvoiceNumber++;
            return number;
        }

        /// <summary>
        /// Freezes the business data, tax treatment and totals - later changes in the profile do not change this invoice.
        /// </summary>
        private void Freeze(Invoice invoice, BusinessSettings settings)
        {
            invoice.SenderJson = JsonSerializer.Serialize(_mapper.Map<BusinessSettingsReadDto>(settings));
            invoice.IsSmallBusiness = settings.IsSmallBusiness;
            invoice.VatRate = settings.IsSmallBusiness ? 0 : settings.VatRate;
            SetTotals(invoice);
        }

        private static void SetTotals(Invoice invoice)
        {
            invoice.NetTotal = invoice.Lines.Sum(l => l.Total);
            invoice.VatAmount = invoice.IsSmallBusiness ? 0 : Math.Round(invoice.NetTotal * invoice.VatRate / 100, 2);
            invoice.GrossTotal = invoice.NetTotal + invoice.VatAmount;
        }

        /// <summary>
        /// The hours and extra items of an invoice are open again (draft deleted or invoice cancelled),
        /// a quote it was made from can be invoiced again.
        /// </summary>
        private async Task ReleaseEntries(int invoiceId)
        {
            var entries = (await _timeEntryRepository.GetAllAsync(t => t.InvoiceId == invoiceId)).ToList();
            var costItems = (await _costItemRepository.GetAllAsync(c => c.InvoiceId == invoiceId)).ToList();

            foreach (var entry in entries)
            {
                entry.InvoiceId = null;
            }
            foreach (var item in costItems)
            {
                item.InvoiceId = null;
            }

            await _timeEntryRepository.UpdateRangeAsync(entries);
            await _costItemRepository.UpdateRangeAsync(costItems);

            // A quote that became this invoice can be invoiced again
            var quotes = (await _quoteRepository.GetAllAsync(q => q.InvoiceId == invoiceId)).ToList();
            foreach (var quote in quotes)
            {
                quote.InvoiceId = null;
                quote.Status = QuoteStatus.Accepted;
            }
            await _quoteRepository.UpdateRangeAsync(quotes);
        }

        private async Task<Invoice> LoadInvoice(int id)
        {
            return (await _invoiceRepository.GetAllWithPathsAsync(i => i.Id == id, InvoicePaths.Split(','))).SingleOrDefault()
                ?? throw new EntityNotFoundException(nameof(Invoice), id);
        }

        private async Task<Invoice> LoadDraft(int id)
        {
            var invoice = await LoadInvoice(id);
            if (invoice.Status != InvoiceStatus.Draft)
            {
                throw new BusinessRuleException("invoice.notDraft", "An issued invoice can not be changed or deleted - cancel it instead.");
            }

            return invoice;
        }

        private async Task<List<InvoiceReadDto>> LoadWhere(Expression<Func<Invoice, bool>> predicate)
        {
            var invoices = (await _invoiceRepository.GetAllWithPathsAsync(predicate, InvoicePaths.Split(','))).ToList();
            if (invoices.Count == 0)
            {
                return new List<InvoiceReadDto>();
            }

            // Which invoices were cancelled by which cancellation invoice
            var cancelledIds = invoices.Select(i => (int?)i.Id).ToList();
            var cancellations = (await _invoiceRepository.GetAllAsync(i => cancelledIds.Contains(i.CancelsInvoiceId)))
                .ToDictionary(i => i.CancelsInvoiceId!.Value, i => i.Number);

            var settings = await FinanceService.LoadSettings(_settingsRepository);
            var today = DateOnly.FromDateTime(DateTime.Now);
            // A draft shows which number it will get
            var currentSender = FinanceService.ToSenderDto(_mapper, settings, today.Year);

            return invoices
                .OrderByDescending(i => i.InvoiceDate)
                .ThenByDescending(i => i.Id)
                .Select(i => ToDto(i, cancellations.GetValueOrDefault(i.Id), currentSender, today))
                .ToList();
        }

        private static InvoiceReadDto ToDto(Invoice invoice, string? cancelledByNumber, BusinessSettingsReadDto currentSender, DateOnly today)
        {
            var isDraft = invoice.Status == InvoiceStatus.Draft;

            // A draft is calculated with the current settings, an issued invoice shows its frozen values
            var isSmallBusiness = isDraft ? currentSender.IsSmallBusiness : invoice.IsSmallBusiness;
            var vatRate = isDraft ? (currentSender.IsSmallBusiness ? 0 : currentSender.VatRate) : invoice.VatRate;
            var netTotal = isDraft ? invoice.Lines.Sum(l => l.Total) : invoice.NetTotal;
            var vatAmount = isDraft ? (isSmallBusiness ? 0 : Math.Round(netTotal * vatRate / 100, 2)) : invoice.VatAmount;
            var grossTotal = isDraft ? netTotal + vatAmount : invoice.GrossTotal;

            var paid = invoice.Payments.Sum(p => p.Amount);
            var open = invoice.CancelsInvoiceId == null && invoice.Status == InvoiceStatus.Issued ? grossTotal - paid : 0;

            return new InvoiceReadDto
            {
                Id = invoice.Id,
                Number = invoice.Number,
                Status = invoice.Status,
                PaymentStatus = PaymentStatusOf(invoice, paid, open, today),
                InvoiceDate = invoice.InvoiceDate,
                ServiceFrom = invoice.ServiceFrom,
                ServiceTo = invoice.ServiceTo,
                DueDate = invoice.DueDate,
                RecipientName = invoice.RecipientName,
                RecipientAddress = invoice.RecipientAddress,
                IntroText = invoice.IntroText,
                ClosingText = invoice.ClosingText,
                IsSmallBusiness = isSmallBusiness,
                VatRate = vatRate,
                NetTotal = netTotal,
                VatAmount = vatAmount,
                GrossTotal = grossTotal,
                PaidAmount = paid,
                OpenAmount = open,
                CancelsInvoiceId = invoice.CancelsInvoiceId,
                CancelsInvoiceNumber = invoice.CancelsInvoice?.Number,
                CancelledByNumber = cancelledByNumber,
                ProjectId = invoice.ProjectId,
                ProjectTitle = invoice.Project?.Title ?? string.Empty,
                ClientId = invoice.ClientId,
                ClientName = invoice.Client?.Name ?? string.Empty,
                Sender = isDraft || invoice.SenderJson == null
                    ? currentSender
                    : JsonSerializer.Deserialize<BusinessSettingsReadDto>(invoice.SenderJson)!,
                Lines = invoice.Lines.OrderBy(l => l.SortOrder).Select(l => new InvoiceLineDto
                {
                    Description = l.Description,
                    Quantity = l.Quantity,
                    Unit = l.Unit,
                    UnitPrice = l.UnitPrice,
                    Total = l.Total
                }).ToList(),
                Payments = invoice.Payments.OrderBy(p => p.Date).Select(p => new PaymentReadDto
                {
                    Id = p.Id,
                    Date = p.Date,
                    Amount = p.Amount,
                    Note = p.Note
                }).ToList()
            };
        }

        private static InvoicePaymentStatus PaymentStatusOf(Invoice invoice, decimal paid, decimal open, DateOnly today)
        {
            if (invoice.Status == InvoiceStatus.Draft) return InvoicePaymentStatus.Draft;
            if (invoice.Status == InvoiceStatus.Cancelled) return InvoicePaymentStatus.Cancelled;
            if (invoice.CancelsInvoiceId != null) return InvoicePaymentStatus.Cancellation;
            if (open <= 0) return InvoicePaymentStatus.Paid;
            if (invoice.DueDate != null && invoice.DueDate < today) return InvoicePaymentStatus.Overdue;
            return paid > 0 ? InvoicePaymentStatus.PartiallyPaid : InvoicePaymentStatus.Open;
        }

        private static string? Clean(string? text) => string.IsNullOrWhiteSpace(text) ? null : text.Trim();
    }
}
