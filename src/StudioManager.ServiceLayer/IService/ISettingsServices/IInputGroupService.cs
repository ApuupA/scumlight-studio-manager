using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.SettingsModels;

namespace StudioManager.ServiceLayer.IService.ISettingsServices
{
    /// <summary>
    /// Groups of studio inputs (usually one preamp), each with a color for the input list.
    /// </summary>
    public interface IInputGroupService : IBaseService<InputGroup, InputGroupReadDto, InputGroupCreateDto>
    {
    }
}
