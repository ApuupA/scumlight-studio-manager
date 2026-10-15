using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.ProjectModels;

namespace StudioManager.ServiceLayer.IService.IProjectServices
{
    public interface ISongService : IBaseService<Song, SongReadDto, SongCreateDto>
    {
        /// <summary>
        /// All Song entries of one project.
        /// </summary>
        Task<IEnumerable<SongReadDto>> GetForProject(int projectId);
    }
}
