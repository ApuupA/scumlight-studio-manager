using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.FinanceModels;

namespace StudioManager.ServiceLayer.IService.IFinanceServices
{
    /// <summary>
    /// Extra items of a project besides the working time (tape, rental, external mastering, ...).
    /// </summary>
    public interface IProjectCostItemService : IBaseService<ProjectCostItem, ProjectCostItemReadDto, ProjectCostItemCreateDto>
    {
        /// <summary>The items of a project, newest first.</summary>
        Task<IEnumerable<ProjectCostItemReadDto>> GetForProject(int projectId);
    }
}
