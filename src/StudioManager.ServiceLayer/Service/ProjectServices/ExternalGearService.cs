using AutoMapper;
using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.ProjectModels;
using StudioManager.RepositoryLayer.IRepository;
using StudioManager.ServiceLayer.IService.IProjectServices;

namespace StudioManager.ServiceLayer.Service.ProjectServices
{
    public class ExternalGearService : BaseService<ExternalGear, ExternalGearReadDto, ExternalGearCreateDto>, IExternalGearService
    {
        public ExternalGearService(IRepository<ExternalGear> repository, IMapper mapper) : base(repository, mapper)
        {
        }

        public async Task<IEnumerable<ExternalGearReadDto>> GetForProject(int projectId)
        {
            var gear = await GetAllWhere(g => g.ProjectId == projectId);
            return gear.OrderBy(g => g.Owner).ThenBy(g => g.Name);
        }
    }
}
