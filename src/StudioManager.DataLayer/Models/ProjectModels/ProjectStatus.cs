namespace StudioManager.DataLayer.Models.ProjectModels
{
    /// <summary>
    /// Phase of the whole project. Set by hand - the songs have their own status.
    /// </summary>
    public enum ProjectStatus
    {
        Planning,
        PreProduction,
        Recording,
        Editing,
        Mixing,
        Mastering,
        Completed,
        OnHold,
        Cancelled
    }
}
