namespace StudioManager.DataLayer.Models.SettingsModels
{
    /// <summary>
    /// A group of studio inputs, usually one preamp / converter, e.g. "Apollo Unison" or "SSL".
    /// The rows of the input list are colored by group - like in the Excel input list.
    /// </summary>
    public class InputGroup : BaseEntity
    {
        public string Name { get; set; } = string.Empty;

        /// <summary>Color name of the theme palette, e.g. "orange", "red", "blue".</summary>
        public string Color { get; set; } = "gray";

        public int SortOrder { get; set; }

        public ICollection<StudioInput> Inputs { get; set; } = new List<StudioInput>();
    }
}
