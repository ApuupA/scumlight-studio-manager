using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.GearModels;
using StudioManager.ServiceLayer.Results;

namespace StudioManager.ServiceLayer.IService.IGearServices
{
    public interface IGearItemService : IBaseService<GearItem, GearItemReadDto, GearItemCreateDto>
    {
        /// <summary>
        /// Stores a new photo for the gear item and replaces an existing one.
        /// </summary>
        /// <param name="fileName">Original file name, only used to check the file type.</param>
        /// <param name="length">File size in bytes.</param>
        Task<GearItemReadDto> SetPhoto(int id, Stream content, string fileName, long length);

        /// <summary>
        /// Returns the photo of the gear item, null if it has none.
        /// </summary>
        Task<FileDownload?> GetPhoto(int id);

        Task<GearItemReadDto> DeletePhoto(int id);
    }
}
