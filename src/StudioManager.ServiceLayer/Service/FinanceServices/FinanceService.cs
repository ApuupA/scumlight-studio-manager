using AutoMapper;
using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.FinanceModels;
using StudioManager.DataLayer.Models.ProjectModels;
using StudioManager.RepositoryLayer.IRepository;
using StudioManager.ServiceLayer.Exceptions;
using StudioManager.ServiceLayer.IService.IFinanceServices;

namespace StudioManager.ServiceLayer.Service.FinanceServices
{
    public class FinanceService : IFinanceService
    {
        private readonly IRepository<BusinessSettings> _settingsRepository;
        private readonly IRepository<Project> _projectRepository;
        private readonly IRepository<TimeEntry> _timeEntryRepository;
        private readonly IRepository<ProjectCostItem> _costItemRepository;
        private readonly IRepository<Invoice> _invoiceRepository;
        private readonly IMapper _mapper;

        public FinanceService(
            IRepository<BusinessSettings> settingsRepository,
            IRepository<Project> projectRepository,
            IRepository<TimeEntry> timeEntryRepository,
            IRepository<ProjectCostItem> costItemRepository,
            IRepository<Invoice> invoiceRepository,
            IMapper mapper)
        {
            _settingsRepository = settingsRepository;
            _projectRepository = projectRepository;
            _timeEntryRepository = timeEntryRepository;
            _costItemRepository = costItemRepository;
            _invoiceRepository = invoiceRepository;
            _mapper = mapper;
        }

        public async Task<BusinessSettingsReadDto> GetSettings()
        {
            return ToReadDto(await LoadSettings());
        }

        public async Task<BusinessSettingsReadDto> UpdateSettings(BusinessSettingsUpdateDto dto)
        {
            var settings = await LoadSettings();

            // Empty texts are stored as null, so "not filled in" is always the same
            static string? Clean(string? text) => string.IsNullOrWhiteSpace(text) ? null : text.Trim();
            dto.CompanyName = Clean(dto.CompanyName);
            dto.OwnerName = Clean(dto.OwnerName);
            dto.Street = Clean(dto.Street);
            dto.PostalCode = Clean(dto.PostalCode);
            dto.City = Clean(dto.City);
            dto.Email = Clean(dto.Email);
            dto.Phone = Clean(dto.Phone);
            dto.Website = Clean(dto.Website);
            dto.TaxNumber = Clean(dto.TaxNumber);
            dto.VatId = Clean(dto.VatId);
            dto.BankName = Clean(dto.BankName);
            dto.Iban = Clean(dto.Iban)?.Replace(" ", "").ToUpperInvariant();
            dto.Bic = Clean(dto.Bic)?.ToUpperInvariant();
            dto.InvoiceNumberPrefix = dto.InvoiceNumberPrefix?.Trim() ?? string.Empty;
            dto.QuoteNumberPrefix = dto.QuoteNumberPrefix?.Trim() ?? string.Empty;

            _mapper.Map(dto, settings);
            await _settingsRepository.UpdateAsync(settings);
            return ToReadDto(settings);
        }

        /// <summary>
        /// An invoice number: prefix ({YYYY} = year) + number with leading zeros, e.g. "{YYYY}-" + 18 → "2026-018".
        /// </summary>
        public static string FormatInvoiceNumber(string prefix, int number, int digits, int year)
        {
            return prefix.Replace("{YYYY}", year.ToString()) + number.ToString().PadLeft(digits, '0');
        }

        private BusinessSettingsReadDto ToReadDto(BusinessSettings settings) => ToSenderDto(_mapper, settings, DateTime.Now.Year);

        /// <summary>
        /// The settings as DTO including how the next invoice / quote number will look in the given year.
        /// Also used by InvoiceService and QuoteService for drafts.
        /// </summary>
        public static BusinessSettingsReadDto ToSenderDto(IMapper mapper, BusinessSettings settings, int year)
        {
            var dto = mapper.Map<BusinessSettingsReadDto>(settings);
            dto.NextInvoiceNumberPreview = FormatInvoiceNumber(settings.InvoiceNumberPrefix, settings.NextInvoiceNumber, settings.InvoiceNumberDigits, year);
            dto.NextQuoteNumberPreview = FormatInvoiceNumber(settings.QuoteNumberPrefix, settings.NextQuoteNumber, settings.InvoiceNumberDigits, year);
            return dto;
        }

        /// <summary>
        /// Required content of an invoice / quote: full name and address of the sender, tax number or VAT ID.
        /// </summary>
        public static bool HasRequiredBusinessData(BusinessSettings settings) =>
            !string.IsNullOrWhiteSpace(settings.CompanyName ?? settings.OwnerName)
            && !string.IsNullOrWhiteSpace(settings.Street)
            && !string.IsNullOrWhiteSpace(settings.PostalCode)
            && !string.IsNullOrWhiteSpace(settings.City)
            && !string.IsNullOrWhiteSpace(settings.TaxNumber ?? settings.VatId);

        public async Task<ProjectFinanceDto> GetProjectFinance(int projectId)
        {
            var project = await GetProject(projectId);
            var settings = await LoadSettings();

            // Running timers have no hours yet - they count once they are stopped
            var entries = (await _timeEntryRepository.GetAllAsync(t => t.ProjectId == projectId && t.StartedAt == null)).ToList();
            var costItems = await _costItemRepository.GetAllAsync(c => c.ProjectId == projectId && c.IsBillable);

            var totalHours = entries.Sum(t => t.Hours);
            var billableHours = entries.Where(t => t.IsBillable).Sum(t => t.Hours);
            var hourlyRate = project.HourlyRate ?? settings.DefaultHourlyRate;
            var laborValue = project.PricingMode == PricingMode.FlatPrice
                ? project.FlatPrice ?? 0
                : billableHours * hourlyRate;
            var extrasValue = costItems.Sum(c => c.Quantity * c.UnitPrice);

            // Issued invoices (cancellations have negative totals, so a cancelled invoice counts zero) and payments
            var invoices = (await _invoiceRepository.GetAllWithPathsAsync(i => i.ProjectId == projectId && i.Number != null, "Payments")).ToList();
            var invoicedTotal = invoices.Sum(i => i.GrossTotal);
            var paidTotal = invoices.SelectMany(i => i.Payments).Sum(p => p.Amount);

            return new ProjectFinanceDto
            {
                PricingMode = project.PricingMode,
                HourlyRate = project.HourlyRate,
                EffectiveHourlyRate = hourlyRate,
                FlatPrice = project.FlatPrice,
                TotalHours = totalHours,
                BillableHours = billableHours,
                LaborValue = Math.Round(laborValue, 2),
                ExtrasValue = Math.Round(extrasValue, 2),
                TotalValue = Math.Round(laborValue + extrasValue, 2),
                // With a flat price: what an hour of work really earns
                ActualHourlyRate = project.PricingMode == PricingMode.FlatPrice && totalHours > 0
                    ? Math.Round((project.FlatPrice ?? 0) / totalHours, 2)
                    : null,
                InvoicedTotal = invoicedTotal,
                PaidTotal = paidTotal,
                Currency = settings.Currency
            };
        }

        public async Task<ProjectFinanceDto> SetPricing(int projectId, ProjectPricingDto dto)
        {
            if (dto.PricingMode == PricingMode.FlatPrice && dto.FlatPrice == null)
            {
                throw new BusinessRuleException("projectPricing.flatPriceRequired", "Enter the flat price.");
            }

            var project = await GetProject(projectId);
            project.PricingMode = dto.PricingMode;
            project.HourlyRate = dto.HourlyRate;
            project.FlatPrice = dto.FlatPrice;
            await _projectRepository.UpdateAsync(project);

            return await GetProjectFinance(projectId);
        }

        private Task<BusinessSettings> LoadSettings() => LoadSettings(_settingsRepository);

        /// <summary>
        /// The one settings row - created with defaults the first time. Also used by InvoiceService.
        /// </summary>
        public static async Task<BusinessSettings> LoadSettings(IRepository<BusinessSettings> settingsRepository)
        {
            var settings = (await settingsRepository.GetAllAsync()).OrderBy(s => s.Id).FirstOrDefault();
            if (settings == null)
            {
                settings = new BusinessSettings();
                await settingsRepository.AddAsync(settings);
            }

            return settings;
        }

        private async Task<Project> GetProject(int projectId)
        {
            return await _projectRepository.GetAsync(projectId) ?? throw new EntityNotFoundException(nameof(Project), projectId);
        }
    }
}
