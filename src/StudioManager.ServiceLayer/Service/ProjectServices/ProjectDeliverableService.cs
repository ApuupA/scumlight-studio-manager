using AutoMapper;
using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.ProjectModels;
using StudioManager.RepositoryLayer.IRepository;
using StudioManager.ServiceLayer.Exceptions;
using StudioManager.ServiceLayer.IService.IProjectServices;

namespace StudioManager.ServiceLayer.Service.ProjectServices
{
    public class ProjectDeliverableService : BaseService<ProjectDeliverable, ProjectDeliverableReadDto, ProjectDeliverableCreateDto>, IProjectDeliverableService
    {
        /// <summary>The usual deliverables of a release - added with one click.</summary>
        public static readonly string[] DefaultNames =
        [
            "WAV 24 Bit / 44,1 kHz (Streaming)",
            "MP3 320 kbps",
            "Instrumentals",
            "DDP (CD-Presswerk)",
            "Vinyl-Master"
        ];

        private readonly IRepository<Project> _projectRepository;

        public ProjectDeliverableService(IRepository<ProjectDeliverable> repository, IRepository<Project> projectRepository, IMapper mapper)
            : base(repository, mapper)
        {
            _projectRepository = projectRepository;
        }

        public async Task<IEnumerable<ProjectDeliverableReadDto>> GetForProject(int projectId)
        {
            var deliverables = await GetAllWhere(d => d.ProjectId == projectId);
            return deliverables.OrderBy(d => d.SortOrder).ThenBy(d => d.Id);
        }

        public async Task<IEnumerable<ProjectDeliverableReadDto>> AddDefaults(int projectId)
        {
            if (!await _projectRepository.ExistsAsync(p => p.Id == projectId))
            {
                throw new EntityNotFoundException(nameof(Project), projectId);
            }

            var existing = (await _repository.GetAllAsync(d => d.ProjectId == projectId)).ToList();
            var nextSortOrder = existing.Select(d => d.SortOrder).DefaultIfEmpty(0).Max() + 1;

            foreach (var name in DefaultNames.Where(name => !existing.Any(d => d.Name == name)))
            {
                await _repository.AddAsync(new ProjectDeliverable { ProjectId = projectId, Name = name, SortOrder = nextSortOrder++ });
            }

            return await GetForProject(projectId);
        }

        public override async Task<ProjectDeliverableReadDto> Insert(ProjectDeliverableCreateDto createDto)
        {
            createDto.Name = createDto.Name.Trim();
            createDto.Notes = string.IsNullOrWhiteSpace(createDto.Notes) ? null : createDto.Notes.Trim();
            return await base.Insert(createDto);
        }

        public override async Task<ProjectDeliverableReadDto> Update(int id, ProjectDeliverableCreateDto updateDto)
        {
            updateDto.Name = updateDto.Name.Trim();
            updateDto.Notes = string.IsNullOrWhiteSpace(updateDto.Notes) ? null : updateDto.Notes.Trim();
            return await base.Update(id, updateDto);
        }
    }
}
