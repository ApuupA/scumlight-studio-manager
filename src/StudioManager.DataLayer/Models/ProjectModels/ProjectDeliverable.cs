namespace StudioManager.DataLayer.Models.ProjectModels
{
    /// <summary>
    /// Something to deliver at the end of a project, e.g. "WAV 24 Bit / 44,1 kHz" or "Vinyl-Master" - ticked off when sent.
    /// </summary>
    public class ProjectDeliverable : BaseEntity
    {
        public string Name { get; set; } = string.Empty;

        public bool IsDone { get; set; }

        public string? Notes { get; set; }

        /// <summary>Position in the checklist (ascending).</summary>
        public int SortOrder { get; set; }

        public int ProjectId { get; set; }
        public Project? Project { get; set; }
    }
}
