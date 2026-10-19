using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.ClientModels;
using StudioManager.ServiceLayer.Results;

namespace StudioManager.ServiceLayer.IService.IClientServices
{
    public interface IClientService : IBaseService<Client, ClientReadDto, ClientCreateDto>
    {

        /// <summary>
        /// Uploads a picture (JPG, PNG or WEBP, max. 10 MB) and replaces the existing one.
        /// </summary>
        Task<ClientReadDto> SetPhoto(int id, Stream content, string fileName, long length);

        /// <summary>
        /// The picture for downloading, null if there is none.
        /// </summary>
        Task<FileDownload?> GetPhoto(int id);

        /// <summary>
        /// Removes the picture.
        /// </summary>
        Task<ClientReadDto> DeletePhoto(int id);
    }
}
