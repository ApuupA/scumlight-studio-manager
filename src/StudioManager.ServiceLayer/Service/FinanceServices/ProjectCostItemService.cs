using AutoMapper;
using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.FinanceModels;
using StudioManager.RepositoryLayer.IRepository;
using StudioManager.ServiceLayer.IService.IFinanceServices;

namespace StudioManager.ServiceLayer.Service.FinanceServices
{
    public class ProjectCostItemService : BaseService<ProjectCostItem, ProjectCostItemReadDto, ProjectCostItemCreateDto>, IProjectCostItemService
    {
        public ProjectCostItemService(IRepository<ProjectCostItem> repository, IMapper mapper) : base(repository, mapper)
        {
        }

        public async Task<IEnumerable<ProjectCostItemReadDto>> GetForProject(int projectId)
        {
            var items = await GetAllWhere(c => c.ProjectId == projectId);
            return items.OrderByDescending(c => c.Date).ThenByDescending(c => c.Id);
        }

        public override async Task<ProjectCostItemReadDto> Insert(ProjectCostItemCreateDto createDto)
        {
            createDto.Description = createDto.Description.Trim();
            return await base.Insert(createDto);
        }

        public override async Task<ProjectCostItemReadDto> Update(int id, ProjectCostItemCreateDto updateDto)
        {
            updateDto.Description = updateDto.Description.Trim();
            return await base.Update(id, updateDto);
        }
    }
}
