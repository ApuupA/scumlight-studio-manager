using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.ClientModels;
using StudioManager.ServiceLayer.IService.IClientServices;

namespace StudioManager.API.Controllers.ClientControllers
{
    public class ClientController : BaseController<ClientReadDto, ClientCreateDto, Client, IClientService>
    {
        public ClientController(IClientService service) : base(service)
        {
        }
    }
}
