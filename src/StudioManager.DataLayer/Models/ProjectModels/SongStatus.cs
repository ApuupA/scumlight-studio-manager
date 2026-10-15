namespace StudioManager.DataLayer.Models.ProjectModels
{
    /// <summary>
    /// Production progress of a single song, in the order of the workflow.
    /// </summary>
    public enum SongStatus
    {
        Planned,
        Recording,
        Editing,
        Mixing,
        Mastering,
        Done
    }
}
