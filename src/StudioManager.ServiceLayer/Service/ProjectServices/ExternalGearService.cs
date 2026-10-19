using AutoMapper;
using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.ProjectModels;
using StudioManager.RepositoryLayer.IRepository;
using StudioManager.ServiceLayer.Exceptions;
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

            // New suggestions of the band first, so they are not overlooked
            return gear.OrderByDescending(g => g.IsSuggestion).ThenBy(g => g.Owner).ThenBy(g => g.Name);
        }

        public async Task<ExternalGearReadDto> Accept(int id)
        {
            var gear = await _repository.GetAsync(id) ?? throw new EntityNotFoundException(nameof(ExternalGear), id);

            gear.IsSuggestion = false;
            await _repository.UpdateAsync(gear);

            return (await Get(id))!;
        }
    }
}
