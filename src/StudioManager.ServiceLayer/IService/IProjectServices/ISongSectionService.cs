using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.ProjectModels;

namespace StudioManager.ServiceLayer.IService.IProjectServices
{
    /// <summary>
    /// The arrangement of a song: parts like "Verse 1" or "Chorus 1" with bars and notes.
    /// </summary>
    public interface ISongSectionService : IBaseService<SongSection, SongSectionReadDto, SongSectionCreateDto>
    {
        /// <summary>
        /// The parts of one song in song order.
        /// </summary>
        Task<IEnumerable<SongSectionReadDto>> GetForSong(int songId);

        /// <summary>
        /// Moves a part one position up (-1) or down (+1).
        /// </summary>
        Task Move(int id, MoveDto dto);
    }
}
