using System.ComponentModel.DataAnnotations;

namespace StudioManager.DataLayer.DTO.Create
{
    /// <summary>
    /// One tempo of a song, sent as part of SongCreateDto. The order of the list is the order in the song.
    /// </summary>
    public class SongTempoCreateDto
    {
        [Range(1, 400)]
        public decimal Bpm { get; set; }

        [Range(0, 99999)]
        public int? FromBar { get; set; }
    }
}
