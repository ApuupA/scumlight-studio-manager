using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.ProjectModels;

namespace StudioManager.ServiceLayer.IService.IProjectServices
{
    /// <summary>
    /// Mix versions of a project ("v1" with all songs, "v1.1" with some) with the client's feedback.
    /// </summary>
    public interface IMixVersionService : IBaseService<MixVersion, MixVersionReadDto, MixVersionCreateDto>
    {
        /// <summary>The versions of a project, newest first.</summary>
        Task<IEnumerable<MixVersionReadDto>> GetForProject(int projectId);

        /// <summary>The versions a song was exported in, newest first - for the song page.</summary>
        Task<IEnumerable<MixVersionReadDto>> GetForSong(int songId);
    }
}
