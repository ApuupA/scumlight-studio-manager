using AutoMapper;
using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.ClientModels;
using StudioManager.DataLayer.Models.ProjectModels;
using StudioManager.RepositoryLayer.IRepository;
using StudioManager.ServiceLayer.Exceptions;
using StudioManager.ServiceLayer.IService.IProjectServices;
using System.Linq.Expressions;

namespace StudioManager.ServiceLayer.Service.ProjectServices
{
    public class ProjectService : BaseService<Project, ProjectReadDto, ProjectCreateDto>, IProjectService
    {
        private readonly IRepository<Client> _clientRepository;
        private readonly IRepository<Band> _bandRepository;

        public ProjectService(
            IRepository<Project> repository,
            IRepository<Client> clientRepository,
            IRepository<Band> bandRepository,
            IMapper mapper) : base(repository, mapper)
        {
            _clientRepository = clientRepository;
            _bandRepository = bandRepository;
        }

        // Everything needed for the list: client name, bands and the progress numbers
        protected override Expression<Func<Project, object>>[] DefaultIncludes =>
            [p => p.Client!, p => p.Bands, p => p.Songs, p => p.Sessions];

        public override async Task<IEnumerable<ProjectReadDto>> GetAll()
        {
            var projects = await base.GetAll();
            return projects.OrderByDescending(p => p.DateCreated);
        }

        public async Task<IEnumerable<ProjectReadDto>> GetForClient(int clientId)
        {
            var projects = await GetAllWhere(p => p.ClientId == clientId);
            return projects.OrderByDescending(p => p.DateCreated);
        }

        public override async Task<ProjectReadDto> Insert(ProjectCreateDto createDto)
        {
            await EnsureClientExists(createDto.ClientId);
            var bands = await LoadBandsOfClient(createDto);

            var project = _mapper.Map<Project>(createDto);
            foreach (var band in bands)
            {
                project.Bands.Add(band);
            }

            await _repository.AddAsync(project);
            return (await Get(project.Id))!;
        }

        public override async Task<ProjectReadDto> Update(int id, ProjectCreateDto updateDto)
        {
            // Load the current bands too, so EF knows which band links to remove
            var project = await _repository.GetAsync(p => p.Id == id, p => p.Bands)
                ?? throw new EntityNotFoundException(nameof(Project), id);

            await EnsureClientExists(updateDto.ClientId);
            var bands = await LoadBandsOfClient(updateDto);

            _mapper.Map(updateDto, project);
            project.Bands.Clear();
            foreach (var band in bands)
            {
                project.Bands.Add(band);
            }

            await _repository.UpdateAsync(project);
            return (await Get(id))!;
        }

        private async Task EnsureClientExists(int clientId)
        {
            if (!await _clientRepository.ExistsAsync(c => c.Id == clientId))
            {
                throw new BusinessRuleException("project.clientNotFound", $"Client with id {clientId} does not exist.");
            }
        }

        /// <summary>
        /// Loads the bands of the DTO and makes sure all of them belong to the project's client.
        /// </summary>
        private async Task<List<Band>> LoadBandsOfClient(ProjectCreateDto dto)
        {
            var bandIds = dto.BandIds.Distinct().ToList();
            var bands = (await _bandRepository.GetAllAsync(b => bandIds.Contains(b.Id))).ToList();

            if (bands.Count != bandIds.Count || bands.Any(b => b.ClientId != dto.ClientId))
            {
                throw new BusinessRuleException("project.bandNotOfClient",
                    "All bands of a project must belong to the project's client.");
            }

            return bands;
        }
    }
}
