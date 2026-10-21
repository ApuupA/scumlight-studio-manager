using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.ProjectModels;

namespace StudioManager.ServiceLayer.IService.IProjectServices
{
    /// <summary>
    /// Mix versions of a song (v1, v2, ...) with the client's feedback.
    /// </summary>
    public interface IMixVersionService : IBaseService<MixVersion, MixVersionReadDto, MixVersionCreateDto>
    {
        /// <summary>The versions of a song, newest first.</summary>
        Task<IEnumerable<MixVersionReadDto>> GetForSong(int songId);
    }
}
