using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.ClientModels;

namespace StudioManager.ServiceLayer.IService.IClientServices
{
    public interface IClientService : IBaseService<Client, ClientReadDto, ClientCreateDto>
    {
    }
}
