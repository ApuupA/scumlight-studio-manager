using AutoMapper;
using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.ProjectModels;
using StudioManager.RepositoryLayer.IRepository;
using StudioManager.ServiceLayer.Exceptions;
using StudioManager.ServiceLayer.IService.IProjectServices;
using System.Linq.Expressions;

namespace StudioManager.ServiceLayer.Service.ProjectServices
{
    public class InstrumentSetupItemService : BaseService<InstrumentSetupItem, InstrumentSetupItemReadDto, InstrumentSetupItemCreateDto>, IInstrumentSetupItemService
    {
        private readonly IRepository<ProjectInstrument> _instrumentRepository;
        private readonly IRepository<ExternalGear> _externalGearRepository;

        public InstrumentSetupItemService(
            IRepository<InstrumentSetupItem> repository,
            IRepository<ProjectInstrument> instrumentRepository,
            IRepository<ExternalGear> externalGearRepository,
            IMapper mapper) : base(repository, mapper)
        {
            _instrumentRepository = instrumentRepository;
            _externalGearRepository = externalGearRepository;
        }

        // Load the gear (+ category) and band gear, so name, category and status can be shown
        protected override Expression<Func<InstrumentSetupItem, object>>[] DefaultIncludes =>
            [s => s.GearItem!.GearCategory!, s => s.ExternalGear!];

        public override async Task<InstrumentSetupItemReadDto> Insert(InstrumentSetupItemCreateDto createDto)
        {
            await Validate(createDto);

            if (createDto.SortOrder == 0)
            {
                var siblings = await _repository.GetAllAsync(s => s.ProjectInstrumentId == createDto.ProjectInstrumentId);
                createDto.SortOrder = siblings.Select(s => s.SortOrder).DefaultIfEmpty(0).Max() + 1;
            }

            return await base.Insert(createDto);
        }

        public override async Task<InstrumentSetupItemReadDto> Update(int id, InstrumentSetupItemCreateDto updateDto)
        {
            await Validate(updateDto);
            return await base.Update(id, updateDto);
        }

        public async Task Move(int id, SetupItemMoveDto dto)
        {
            var item = await _repository.GetAsync(s => s.Id == id, s => s.ProjectInstrument!)
                ?? throw new EntityNotFoundException(nameof(InstrumentSetupItem), id);

            // Number all entries of the instrument 1, 2, 3, ...
            var siblings = (await _repository.GetAllAsync(s => s.ProjectInstrumentId == item.ProjectInstrumentId))
                .OrderBy(s => s.SortOrder)
                .ThenBy(s => s.Id)
                .ToList();
            for (var i = 0; i < siblings.Count; i++)
            {
                siblings[i].SortOrder = i + 1;
            }

            // A list is shown in two groups (mics / DIs and other gear): swap with the neighbour in the same group.
            // A chain is shown in one row: swap with the direct neighbour.
            var group = item.ProjectInstrument!.IsChain
                ? siblings
                : siblings.Where(s => s.NeedsInput == item.NeedsInput).ToList();

            var index = group.FindIndex(s => s.Id == id);
            var neighbourIndex = index + dto.Direction;
            if (neighbourIndex >= 0 && neighbourIndex < group.Count)
            {
                (group[index].SortOrder, group[neighbourIndex].SortOrder) =
                    (group[neighbourIndex].SortOrder, group[index].SortOrder);
            }

            await _repository.UpdateRangeAsync(siblings);
        }

        public async Task<IEnumerable<GearUsageDto>> GetUsagesOfGearItem(int gearItemId)
        {
            var items = await _repository.GetAllWithPathsAsync(s => s.GearItemId == gearItemId, "ProjectInstrument.Project");
            return _mapper.Map<IEnumerable<GearUsageDto>>(items)
                .OrderBy(u => u.ProjectTitle)
                .ThenBy(u => u.InstrumentName);
        }

        /// <summary>
        /// Exactly one source must be set, and band gear must belong to the instrument's project.
        /// </summary>
        private async Task Validate(InstrumentSetupItemCreateDto dto)
        {
            dto.CustomName = string.IsNullOrWhiteSpace(dto.CustomName) ? null : dto.CustomName.Trim();
            dto.UsedFor = string.IsNullOrWhiteSpace(dto.UsedFor) ? null : dto.UsedFor.Trim();

            var sourceCount = (dto.GearItemId != null ? 1 : 0) + (dto.ExternalGearId != null ? 1 : 0) + (dto.CustomName != null ? 1 : 0);
            if (sourceCount != 1)
            {
                throw new BusinessRuleException("setupItem.oneSourceRequired",
                    "Choose studio gear, band gear or enter a name - exactly one of them.");
            }

            var instrument = await _instrumentRepository.GetAsync(dto.ProjectInstrumentId)
                ?? throw new BusinessRuleException("setupItem.instrumentNotFound", $"Instrument with id {dto.ProjectInstrumentId} does not exist.");

            if (dto.ExternalGearId != null
                && !await _externalGearRepository.ExistsAsync(g => g.Id == dto.ExternalGearId && g.ProjectId == instrument.ProjectId))
            {
                throw new BusinessRuleException("setupItem.bandGearNotInProject", "The band gear does not belong to this project.");
            }
        }
    }
}
