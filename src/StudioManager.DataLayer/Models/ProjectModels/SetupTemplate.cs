namespace StudioManager.DataLayer.Models.ProjectModels
{
    /// <summary>
    /// A reusable instrument setup, e.g. "Standard drum setup (9 mics)". Saved from an instrument of a project
    /// and applied to new projects with one click. Contains studio gear and free text only - band gear
    /// belongs to one project.
    /// </summary>
    public class SetupTemplate : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public bool IsChain { get; set; }
        public string? Notes { get; set; }

        public ICollection<SetupTemplateItem> Items { get; set; } = new List<SetupTemplateItem>();
    }
}
