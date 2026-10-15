namespace StudioManager.DataLayer.Models.SessionModels
{
    public enum SessionStatus
    {
        Planned,
        Confirmed,
        Completed,

        /// <summary>
        /// Cancelled sessions are ignored by the gear booking conflict check.
        /// </summary>
        Cancelled
    }
}
