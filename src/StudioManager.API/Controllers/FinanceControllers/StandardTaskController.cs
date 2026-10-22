using Microsoft.AspNetCore.Authorization;
using StudioManager.API.Infrastructure;
using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.FinanceModels;
using StudioManager.ServiceLayer.IService.IFinanceServices;

namespace StudioManager.API.Controllers.FinanceControllers
{
    /// <summary>
    /// The standard tasks of the studio (settings page), e.g. "Recording / Tag" or "Mixing / Stunde".
    /// </summary>
    [Authorize(Roles = Roles.Owner)] // finance: only the owner
    public class StandardTaskController : BaseController<StandardTaskReadDto, StandardTaskCreateDto, StandardTask, IStandardTaskService>
    {
        public StandardTaskController(IStandardTaskService service) : base(service)
        {
        }
    }
}
