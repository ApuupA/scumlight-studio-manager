using StudioManager.DataLayer.Models.ProjectModels;

namespace StudioManager.DataLayer.DTO.Read
{
    public class SongReadDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int TrackNumber { get; set; }
        public SongStatus Status { get; set; }
        public List<SongTempoReadDto> Tempos { get; set; } = new();
        public string? Tuning { get; set; }
        public string? Notes { get; set; }
        public string? Lyrics { get; set; }

        /// <summary>Number of tracks on the recording checklist.</summary>
        public int TrackCount { get; set; }

        /// <summary>Tracks already recorded (recorded or edited).</summary>
        public int TracksRecordedCount { get; set; }
        public int ProjectId { get; set; }
        public int? BandId { get; set; }
        public string? BandName { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
    }
}
