using StudioManager.DataLayer.Models.ProjectModels;

namespace StudioManager.DataLayer.DTO.Read
{
    public class MixVersionReadDto
    {
        public int Id { get; set; }
        public int Number { get; set; }
        public DateOnly Date { get; set; }
        public MixVersionStatus Status { get; set; }
        public string? Link { get; set; }
        public string? Notes { get; set; }
        public int SongId { get; set; }

        /// <summary>
        /// A revision beyond the ones included in the price of the project (v1 is the mix, v2 the first revision, ...).
        /// </summary>
        public bool IsExtraRevision { get; set; }

        /// <summary>The feedback points in the order they were entered.</summary>
        public List<MixFeedbackReadDto> Feedback { get; set; } = new();
    }
}
