using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.UserModels;

namespace StudioManager.ServiceLayer.IService.IUserServices
{
    /// <summary>
    /// Groups of the studio (settings, only the owner): their members see the projects of the group.
    /// </summary>
    public interface IGroupService : IBaseService<Group, GroupReadDto, GroupCreateDto>
    {
    }
}
