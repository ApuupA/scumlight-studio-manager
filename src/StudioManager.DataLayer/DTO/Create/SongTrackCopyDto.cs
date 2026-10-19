using System.ComponentModel.DataAnnotations;

namespace StudioManager.DataLayer.DTO.Create
{
    /// <summary>
    /// Copies the recording checklist of another song of the same project (names only, all open again).
    /// </summary>
    public class SongTrackCopyDto
    {
        [Range(1, int.MaxValue)]
        public int FromSongId { get; set; }

        [Range(1, int.MaxValue)]
        public int ToSongId { get; set; }
    }
}
