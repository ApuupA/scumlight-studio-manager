using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.ProjectModels;

namespace StudioManager.ServiceLayer.IService.IProjectServices
{
    /// <summary>
    /// The recording checklist of a song: its tracks (Drums, Guitar L, Lead vocal, ...) and their progress.
    /// </summary>
    public interface ISongTrackService : IBaseService<SongTrack, SongTrackReadDto, SongTrackCreateDto>
    {
        /// <summary>
        /// The tracks of one song in checklist order.
        /// </summary>
        Task<IEnumerable<SongTrackReadDto>> GetForSong(int songId);

        /// <summary>
        /// Moves a track one position up (-1) or down (+1).
        /// </summary>
        Task Move(int id, MoveDto dto);

        /// <summary>
        /// Adds one track per instrument of the song's project. Names already on the checklist are skipped.
        /// Returns the whole checklist.
        /// </summary>
        Task<IEnumerable<SongTrackReadDto>> AddFromInstruments(int songId);

        /// <summary>
        /// Copies the track names of another song of the same project (all open). Names already on the checklist
        /// are skipped. Returns the whole checklist of the target song.
        /// </summary>
        Task<IEnumerable<SongTrackReadDto>> CopyFromSong(SongTrackCopyDto dto);
    }
}
