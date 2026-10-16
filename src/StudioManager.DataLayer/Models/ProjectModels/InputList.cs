using StudioManager.DataLayer.Models.SessionModels;

namespace StudioManager.DataLayer.Models.ProjectModels
{
    /// <summary>
    /// How the studio inputs are used for one recording setup of a project, e.g. "Drums", "Vocals" or
    /// "Live guitar + drums". A project can have several input lists; a session chooses the one it uses.
    /// Only filled inputs are stored as rows - empty inputs are simply not in the table.
    /// </summary>
    public class InputList : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Notes { get; set; }

        /// <summary>Position of the tab (ascending).</summary>
        public int SortOrder { get; set; }

        public int ProjectId { get; set; }
        public Project? Project { get; set; }

        public ICollection<InputListRow> Rows { get; set; } = new List<InputListRow>();
        public ICollection<Session> Sessions { get; set; } = new List<Session>();
    }
}
