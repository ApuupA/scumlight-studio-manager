namespace StudioManager.DataLayer.DTO.Create
{
    /// <summary>
    /// Which parts of the work belong to the project (or are done right now). At least one must stay on.
    /// Switching a part off only hides its tabs - the data stays.
    /// </summary>
    public class ProjectScopeDto
    {
        public bool IncludesSongwriting { get; set; }
        public bool IncludesRecording { get; set; }
        public bool IncludesMixing { get; set; }
        public bool IncludesMastering { get; set; }
    }
}
