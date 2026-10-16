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
    public class ProjectInstrumentService : BaseService<ProjectInstrument, ProjectInstrumentReadDto, ProjectInstrumentCreateDto>, IProjectInstrumentService
    {
        /// <summary>
        /// Everything shown with an instrument: band, setup entries with their studio gear (+ category) and band gear.
        /// Text paths, because they go "through" the list of setup items.
        /// </summary>
        private static readonly string[] IncludePaths =
            ["Band", "SetupItems.GearItem.GearCategory", "SetupItems.ExternalGear"];

        private readonly IRepository<Project> _projectRepository;
        private readonly IRepository<SetupTemplate> _templateRepository;

        public ProjectInstrumentService(
            IRepository<ProjectInstrument> repository,
            IRepository<Project> projectRepository,
            IRepository<SetupTemplate> templateRepository,
            IMapper mapper) : base(repository, mapper)
        {
            _projectRepository = projectRepository;
            _templateRepository = templateRepository;
        }

        public override async Task<IEnumerable<ProjectInstrumentReadDto>> GetAll()
        {
            return await LoadWhere(i => true);
        }

        public async Task<IEnumerable<ProjectInstrumentReadDto>> GetForProject(int projectId)
        {
            return await LoadWhere(i => i.ProjectId == projectId);
        }

        public override async Task<ProjectInstrumentReadDto?> Get(int id)
        {
            return (await LoadWhere(i => i.Id == id)).SingleOrDefault();
        }

        public override async Task<ProjectInstrumentReadDto> Insert(ProjectInstrumentCreateDto createDto)
        {
            await EnsureBandIsInProject(createDto.ProjectId, createDto.BandId);

            if (createDto.SortOrder == 0)
            {
                createDto.SortOrder = await NextSortOrder(createDto.ProjectId);
            }

            return await base.Insert(createDto);
        }

        public override async Task<ProjectInstrumentReadDto> Update(int id, ProjectInstrumentCreateDto updateDto)
        {
            await EnsureBandIsInProject(updateDto.ProjectId, updateDto.BandId);
            return await base.Update(id, updateDto);
        }

        public async Task<ProjectInstrumentReadDto> ApplyTemplate(ApplySetupTemplateDto dto)
        {
            var template = (await _templateRepository.GetAllWithPathsAsync(t => t.Id == dto.SetupTemplateId, "Items")).SingleOrDefault()
                ?? throw new EntityNotFoundException(nameof(SetupTemplate), dto.SetupTemplateId);

            await EnsureBandIsInProject(dto.ProjectId, dto.BandId);

            var instrument = new ProjectInstrument
            {
                ProjectId = dto.ProjectId,
                BandId = dto.BandId,
                Name = string.IsNullOrWhiteSpace(dto.InstrumentName) ? template.Name : dto.InstrumentName.Trim(),
                IsChain = template.IsChain,
                Notes = template.Notes,
                SortOrder = await NextSortOrder(dto.ProjectId),
                SetupItems = template.Items.Select(item => new InstrumentSetupItem
                {
                    GearItemId = item.GearItemId,
                    CustomName = item.CustomName,
                    UsedFor = item.UsedFor,
                    Quantity = item.Quantity,
                    NeedsInput = item.NeedsInput,
                    SortOrder = item.SortOrder,
                    Notes = item.Notes
                }).ToList()
            };

            await _repository.AddAsync(instrument);
            return (await Get(instrument.Id))!;
        }

        private async Task<List<ProjectInstrumentReadDto>> LoadWhere(Expression<Func<ProjectInstrument, bool>> predicate)
        {
            var instruments = await _repository.GetAllWithPathsAsync(predicate, IncludePaths);
            return _mapper.Map<IEnumerable<ProjectInstrumentReadDto>>(instruments)
                .OrderBy(i => i.SortOrder)
                .ThenBy(i => i.Id)
                .ToList();
        }

        private async Task<int> NextSortOrder(int projectId)
        {
            var instruments = await _repository.GetAllAsync(i => i.ProjectId == projectId);
            return instruments.Select(i => i.SortOrder).DefaultIfEmpty(0).Max() + 1;
        }

        /// <summary>
        /// The project must exist and the band (if set) must take part in it.
        /// </summary>
        private async Task EnsureBandIsInProject(int projectId, int? bandId)
        {
            var project = await _projectRepository.GetAsync(p => p.Id == projectId, p => p.Bands)
                ?? throw new BusinessRuleException("instrument.projectNotFound", $"Project with id {projectId} does not exist.");

            if (bandId != null && project.Bands.All(b => b.Id != bandId))
            {
                throw new BusinessRuleException("instrument.bandNotInProject", "The band does not take part in this project.");
            }
        }
    }
}
