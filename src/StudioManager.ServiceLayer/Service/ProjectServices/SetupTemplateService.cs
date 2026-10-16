using AutoMapper;
using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.ProjectModels;
using StudioManager.RepositoryLayer.IRepository;
using StudioManager.ServiceLayer.Exceptions;
using StudioManager.ServiceLayer.IService.IProjectServices;

namespace StudioManager.ServiceLayer.Service.ProjectServices
{
    public class SetupTemplateService : ISetupTemplateService
    {
        private readonly IRepository<SetupTemplate> _repository;
        private readonly IRepository<ProjectInstrument> _instrumentRepository;
        private readonly IMapper _mapper;

        public SetupTemplateService(IRepository<SetupTemplate> repository, IRepository<ProjectInstrument> instrumentRepository, IMapper mapper)
        {
            _repository = repository;
            _instrumentRepository = instrumentRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<SetupTemplateReadDto>> GetAll()
        {
            var templates = await _repository.GetAllWithPathsAsync(t => true, "Items.GearItem");
            return _mapper.Map<IEnumerable<SetupTemplateReadDto>>(templates).OrderBy(t => t.Name);
        }

        public async Task<SetupTemplateReadDto> CreateFromInstrument(SetupTemplateFromInstrumentDto dto)
        {
            var instrument = (await _instrumentRepository.GetAllWithPathsAsync(i => i.Id == dto.ProjectInstrumentId, "SetupItems")).SingleOrDefault()
                ?? throw new EntityNotFoundException(nameof(ProjectInstrument), dto.ProjectInstrumentId);

            var template = new SetupTemplate
            {
                Name = dto.Name.Trim(),
                IsChain = instrument.IsChain,
                Notes = instrument.Notes,
                // Band gear belongs to one project only - it is not part of a template
                Items = instrument.SetupItems
                    .Where(item => item.ExternalGearId == null)
                    .Select(item => new SetupTemplateItem
                    {
                        GearItemId = item.GearItemId,
                        CustomName = item.CustomName,
                        UsedFor = item.UsedFor,
                        Quantity = item.Quantity,
                        NeedsInput = item.NeedsInput,
                        SortOrder = item.SortOrder,
                        Notes = item.Notes
                    })
                    .ToList()
            };

            await _repository.AddAsync(template);

            var created = (await _repository.GetAllWithPathsAsync(t => t.Id == template.Id, "Items.GearItem")).Single();
            return _mapper.Map<SetupTemplateReadDto>(created);
        }

        public async Task Delete(int id)
        {
            if (!await _repository.ExistsAsync(t => t.Id == id))
            {
                throw new EntityNotFoundException(nameof(SetupTemplate), id);
            }

            await _repository.DeleteAsync(id);
        }
    }
}
