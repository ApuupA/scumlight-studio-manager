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
        private readonly IMapper _mapper;

        public FinanceService(
            IRepository<BusinessSettings> settingsRepository,
            IRepository<Project> projectRepository,
            IRepository<TimeEntry> timeEntryRepository,
            IRepository<ProjectCostItem> costItemRepository,
            IMapper mapper)
        {
            _settingsRepository = settingsRepository;
            _projectRepository = projectRepository;
            _timeEntryRepository = timeEntryRepository;
            _costItemRepository = costItemRepository;
            _mapper = mapper;
        }

        public async Task<BusinessSettingsReadDto> GetSettings()
        {
            return _mapper.Map<BusinessSettingsReadDto>(await LoadSettings());
        }

        public async Task<BusinessSettingsReadDto> UpdateSettings(BusinessSettingsUpdateDto dto)
        {
            var settings = await LoadSettings();
            _mapper.Map(dto, settings);
            await _settingsRepository.UpdateAsync(settings);
            return _mapper.Map<BusinessSettingsReadDto>(settings);
        }

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

        /// <summary>
        /// The one settings row - created with defaults the first time.
        /// </summary>
        private async Task<BusinessSettings> LoadSettings()
        {
            var settings = (await _settingsRepository.GetAllAsync()).OrderBy(s => s.Id).FirstOrDefault();
            if (settings == null)
            {
                settings = new BusinessSettings();
                await _settingsRepository.AddAsync(settings);
            }

            return settings;
        }

        private async Task<Project> GetProject(int projectId)
        {
            return await _projectRepository.GetAsync(projectId) ?? throw new EntityNotFoundException(nameof(Project), projectId);
        }
    }
}
