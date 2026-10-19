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
    /// Quotes - built like the invoices (see InvoiceService), with their own number range.
    /// </summary>
    public class QuoteService : IQuoteService
    {
        private static readonly string[] QuotePaths = ["Lines", "Project", "Client", "Invoice"];

        private readonly IRepository<Quote> _quoteRepository;
        private readonly IRepository<Project> _projectRepository;
        private readonly IRepository<BusinessSettings> _settingsRepository;
        private readonly IMapper _mapper;

        public QuoteService(
            IRepository<Quote> quoteRepository,
            IRepository<Project> projectRepository,
            IRepository<BusinessSettings> settingsRepository,
            IMapper mapper)
        {
            _quoteRepository = quoteRepository;
            _projectRepository = projectRepository;
            _settingsRepository = settingsRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<QuoteReadDto>> GetAll() => await LoadWhere(q => true);

        public async Task<IEnumerable<QuoteReadDto>> GetForProject(int projectId) => await LoadWhere(q => q.ProjectId == projectId);

        public async Task<QuoteReadDto?> Get(int id) => (await LoadWhere(q => q.Id == id)).SingleOrDefault();

        public async Task<QuoteReadDto> CreateDraft(int projectId)
        {
            var project = await _projectRepository.GetAsync(p => p.Id == projectId, p => p.Client!)
                ?? throw new EntityNotFoundException(nameof(Project), projectId);

            var quote = new Quote
            {
                ProjectId = project.Id,
                ClientId = project.ClientId,
                QuoteDate = DateOnly.FromDateTime(DateTime.Now),
                RecipientName = project.Client?.Name ?? string.Empty,
                RecipientAddress = project.Client?.Address,
                IntroText = $"Projekt: {project.Title}"
            };

            // A flat price project starts with its flat price - otherwise the lines are added from the standard tasks
            if (project.PricingMode == PricingMode.FlatPrice && project.FlatPrice != null)
            {
                quote.Lines.Add(Line($"Pauschale: {project.Title}", 1, "pauschal", project.FlatPrice.Value, 1));
            }

            await _quoteRepository.AddAsync(quote);
            return (await Get(quote.Id))!;
        }

        public async Task<QuoteReadDto> UpdateDraft(int id, QuoteUpdateDto dto)
        {
            var quote = await LoadDraft(id);

            quote.QuoteDate = dto.QuoteDate;
            quote.RecipientName = dto.RecipientName.Trim();
            quote.RecipientAddress = Clean(dto.RecipientAddress);
            quote.IntroText = Clean(dto.IntroText);
            quote.ClosingText = Clean(dto.ClosingText);

            // The lines are replaced (EF deletes the removed ones)
            quote.Lines.Clear();
            for (var i = 0; i < dto.Lines.Count; i++)
            {
                var line = dto.Lines[i];
                quote.Lines.Add(Line(line.Description.Trim(), line.Quantity, Clean(line.Unit), line.UnitPrice, i + 1));
            }

            await _quoteRepository.UpdateAsync(quote);
            return (await Get(id))!;
        }

        public async Task DeleteDraft(int id)
        {
            var quote = await LoadDraft(id);
            await _quoteRepository.DeleteAsync(quote.Id);
        }

        public async Task<QuoteReadDto> Issue(int id, InvoiceDateDto dto)
        {
            var quote = await LoadDraft(id);
            var settings = await FinanceService.LoadSettings(_settingsRepository);

            if (quote.Lines.Count == 0)
            {
                throw new BusinessRuleException("quote.noLines", "The quote has no lines.");
            }

            if (!FinanceService.HasRequiredBusinessData(settings))
            {
                throw new BusinessRuleException("invoice.businessDataMissing",
                    "Fill in the business data first (profile page): name, address and tax number.");
            }

            quote.QuoteDate = dto.Date;
            quote.ValidUntil = dto.Date.AddDays(settings.QuoteValidityDays);
            quote.Number = FinanceService.FormatInvoiceNumber(settings.QuoteNumberPrefix, settings.NextQuoteNumber, settings.InvoiceNumberDigits, dto.Date.Year);
            settings.NextQuoteNumber++;

            // Freeze business data, tax treatment and totals
            quote.SenderJson = JsonSerializer.Serialize(_mapper.Map<BusinessSettingsReadDto>(settings));
            quote.IsSmallBusiness = settings.IsSmallBusiness;
            quote.VatRate = settings.IsSmallBusiness ? 0 : settings.VatRate;
            quote.NetTotal = quote.Lines.Sum(l => l.Total);
            quote.VatAmount = quote.IsSmallBusiness ? 0 : Math.Round(quote.NetTotal * quote.VatRate / 100, 2);
            quote.GrossTotal = quote.NetTotal + quote.VatAmount;
            quote.Status = QuoteStatus.Sent;

            await _settingsRepository.UpdateAsync(settings);
            await _quoteRepository.UpdateAsync(quote);
            return (await Get(id))!;
        }

        public async Task<QuoteReadDto> SetStatus(int id, QuoteStatusDto dto)
        {
            var quote = await _quoteRepository.GetAsync(id) ?? throw new EntityNotFoundException(nameof(Quote), id);

            // Only the answer of the client can be set - drafts are issued, invoices are made with "create invoice"
            var allowed = new[] { QuoteStatus.Sent, QuoteStatus.Accepted, QuoteStatus.Declined };
            if (!allowed.Contains(quote.Status) || !allowed.Contains(dto.Status))
            {
                throw new BusinessRuleException("quote.statusNotPossible", "This status can not be set for this quote.");
            }

            quote.Status = dto.Status;
            await _quoteRepository.UpdateAsync(quote);
            return (await Get(id))!;
        }

        private static QuoteLine Line(string description, decimal quantity, string? unit, decimal unitPrice, int sortOrder) => new()
        {
            Description = description,
            Quantity = quantity,
            Unit = unit,
            UnitPrice = unitPrice,
            Total = Math.Round(quantity * unitPrice, 2),
            SortOrder = sortOrder
        };

        private async Task<Quote> LoadDraft(int id)
        {
            var quote = (await _quoteRepository.GetAllWithPathsAsync(q => q.Id == id, QuotePaths)).SingleOrDefault()
                ?? throw new EntityNotFoundException(nameof(Quote), id);

            if (quote.Status != QuoteStatus.Draft)
            {
                throw new BusinessRuleException("quote.notDraft", "An issued quote can not be changed or deleted.");
            }

            return quote;
        }

        private async Task<List<QuoteReadDto>> LoadWhere(Expression<Func<Quote, bool>> predicate)
        {
            var quotes = (await _quoteRepository.GetAllWithPathsAsync(predicate, QuotePaths)).ToList();
            var settings = await FinanceService.LoadSettings(_settingsRepository);
            var today = DateOnly.FromDateTime(DateTime.Now);
            var currentSender = FinanceService.ToSenderDto(_mapper, settings, today.Year);

            return quotes
                .OrderByDescending(q => q.QuoteDate)
                .ThenByDescending(q => q.Id)
                .Select(q => ToDto(q, currentSender, today))
                .ToList();
        }

        private static QuoteReadDto ToDto(Quote quote, BusinessSettingsReadDto currentSender, DateOnly today)
        {
            var isDraft = quote.Status == QuoteStatus.Draft;

            // A draft is calculated with the current settings, an issued quote shows its frozen values
            var isSmallBusiness = isDraft ? currentSender.IsSmallBusiness : quote.IsSmallBusiness;
            var vatRate = isDraft ? (currentSender.IsSmallBusiness ? 0 : currentSender.VatRate) : quote.VatRate;
            var netTotal = isDraft ? quote.Lines.Sum(l => l.Total) : quote.NetTotal;
            var vatAmount = isDraft ? (isSmallBusiness ? 0 : Math.Round(netTotal * vatRate / 100, 2)) : quote.VatAmount;

            return new QuoteReadDto
            {
                Id = quote.Id,
                Number = quote.Number,
                Status = quote.Status,
                IsExpired = quote.Status == QuoteStatus.Sent && quote.ValidUntil < today,
                QuoteDate = quote.QuoteDate,
                ValidUntil = quote.ValidUntil,
                RecipientName = quote.RecipientName,
                RecipientAddress = quote.RecipientAddress,
                IntroText = quote.IntroText,
                ClosingText = quote.ClosingText,
                IsSmallBusiness = isSmallBusiness,
                VatRate = vatRate,
                NetTotal = netTotal,
                VatAmount = vatAmount,
                GrossTotal = isDraft ? netTotal + vatAmount : quote.GrossTotal,
                ProjectId = quote.ProjectId,
                ProjectTitle = quote.Project?.Title ?? string.Empty,
                ClientId = quote.ClientId,
                ClientName = quote.Client?.Name ?? string.Empty,
                InvoiceId = quote.InvoiceId,
                InvoiceNumber = quote.Invoice?.Number,
                Sender = isDraft || quote.SenderJson == null
                    ? currentSender
                    : JsonSerializer.Deserialize<BusinessSettingsReadDto>(quote.SenderJson)!,
                Lines = quote.Lines.OrderBy(l => l.SortOrder).Select(l => new InvoiceLineDto
                {
                    Description = l.Description,
                    Quantity = l.Quantity,
                    Unit = l.Unit,
                    UnitPrice = l.UnitPrice,
                    Total = l.Total
                }).ToList()
            };
        }

        private static string? Clean(string? text) => string.IsNullOrWhiteSpace(text) ? null : text.Trim();
    }
}
