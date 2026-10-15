using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.ClientModels;

namespace StudioManager.ServiceLayer.IService.IClientServices
{
    public interface IBandService : IBaseService<Band, BandReadDto, BandCreateDto>
    {
        /// <summary>
        /// All Band entries of one client.
        /// </summary>
        Task<IEnumerable<BandReadDto>> GetForClient(int clientId);
    }
}
