using StudioManager.DataLayer.Models.ProjectModels;

namespace StudioManager.DataLayer.DTO.Read
{
    public class MixVersionReadDto
    {
        public int Id { get; set; }
        public int Number { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateOnly Date { get; set; }
        public MixVersionStatus Status { get; set; }
        public string? Link { get; set; }
        public string? Notes { get; set; }
        public int ProjectId { get; set; }

        /// <summary>The songs exported in this version, in track order.</summary>
        public List<SongShortDto> Songs { get; set; } = new();

        /// <summary>True if the version contains all songs of the project - shown as "All songs".</summary>
        public bool IsAllSongs { get; set; }

        /// <summary>
        /// Songs that get a revision beyond the ones included in the price with this version
        /// (counted per song: its 2nd version is the 1st revision). Empty = nothing extra.
        /// </summary>
        public List<SongShortDto> ExtraRevisionSongs { get; set; } = new();

        /// <summary>The feedback points in the order they were entered.</summary>
        public List<MixFeedbackReadDto> Feedback { get; set; } = new();
    }
}
