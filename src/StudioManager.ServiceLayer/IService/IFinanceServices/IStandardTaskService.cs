using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.FinanceModels;

namespace StudioManager.ServiceLayer.IService.IFinanceServices
{
    /// <summary>
    /// The standard tasks of the studio (settings): what time entries are booked on and what the invoice shows.
    /// </summary>
    public interface IStandardTaskService : IBaseService<StandardTask, StandardTaskReadDto, StandardTaskCreateDto>
    {
    }
}
