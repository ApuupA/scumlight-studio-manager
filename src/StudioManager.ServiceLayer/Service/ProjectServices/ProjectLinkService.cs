using AutoMapper;
using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.ProjectModels;
using StudioManager.RepositoryLayer.IRepository;
using StudioManager.ServiceLayer.IService.IProjectServices;
using System.Linq.Expressions;

namespace StudioManager.ServiceLayer.Service.ProjectServices
{
    public class ProjectLinkService : BaseService<ProjectLink, ProjectLinkReadDto, ProjectLinkCreateDto>, IProjectLinkService
    {
        public ProjectLinkService(IRepository<ProjectLink> repository, IMapper mapper) : base(repository, mapper)
        {
        }

        // Load the song, so SongTitle can be filled
        protected override Expression<Func<ProjectLink, object>>[] DefaultIncludes => [l => l.Song!];

        public async Task<IEnumerable<ProjectLinkReadDto>> GetForProject(int projectId)
        {
            var links = await GetAllWhere(l => l.ProjectId == projectId);
            return links.OrderBy(l => l.Type).ThenBy(l => l.Title);
        }
    }
}
