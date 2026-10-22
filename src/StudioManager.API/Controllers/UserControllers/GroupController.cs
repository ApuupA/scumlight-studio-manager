using Microsoft.AspNetCore.Authorization;
using StudioManager.API.Infrastructure;
using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.UserModels;
using StudioManager.ServiceLayer.IService.IUserServices;

namespace StudioManager.API.Controllers.UserControllers
{
    /// <summary>
    /// Groups of the studio with their members (settings). Only the owner manages them.
    /// </summary>
    [Authorize(Roles = Roles.Owner)]
    public class GroupController : BaseController<GroupReadDto, GroupCreateDto, Group, IGroupService>
    {
        public GroupController(IGroupService service) : base(service)
        {
        }
    }
}
