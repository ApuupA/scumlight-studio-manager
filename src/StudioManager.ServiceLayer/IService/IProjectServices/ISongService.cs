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

        /// <summary>
        /// Saves the lyrics of a song (empty text = no lyrics).
        /// </summary>
        Task<SongReadDto> SetLyrics(int id, SongLyricsDto dto);

        /// <summary>
        /// Saves the mixing and mastering details of a song (material, mix recall, LUFS, ISRC, ...).
        /// </summary>
        Task<SongReadDto> SetMixInfo(int id, SongMixInfoDto dto);

        /// <summary>
        /// Sets the same status for several songs at once (e.g. four songs to "Mixing"). Returns the changed songs.
        /// </summary>
        Task<IEnumerable<SongReadDto>> SetStatus(SongStatusUpdateDto dto);
    }
}
