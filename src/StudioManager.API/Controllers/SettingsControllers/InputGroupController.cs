using StudioManager.API.Infrastructure;
using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.SettingsModels;
using StudioManager.ServiceLayer.IService.ISettingsServices;

namespace StudioManager.API.Controllers.SettingsControllers
{
    [OwnerForChanges] // studio settings: everybody reads (input lists), only the owner changes
    public class InputGroupController : BaseController<InputGroupReadDto, InputGroupCreateDto, InputGroup, IInputGroupService>
    {
        public InputGroupController(IInputGroupService service) : base(service)
        {
        }
    }
}
