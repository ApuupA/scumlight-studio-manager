namespace StudioManager.DataLayer.Models.ProjectModels
{
    /// <summary>
    /// Production progress of a single song, in the order of the workflow.
    /// Idea, Writing and Demo are the songwriting steps (own bands); stored as text, so new values need no migration.
    /// </summary>
    public enum SongStatus
    {
        Idea,
        Writing,
        Demo,
        Planned,
        Recording,
        Editing,
        Mixing,
        Mastering,
        Done
    }
}
