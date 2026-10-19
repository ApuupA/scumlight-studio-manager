namespace StudioManager.DataLayer.Models.ProjectModels
{
    /// <summary>
    /// Progress of one track of the recording checklist.
    /// </summary>
    public enum SongTrackStatus
    {
        /// <summary>Still to record.</summary>
        Open,

        /// <summary>Recorded, not edited yet.</summary>
        Recorded,

        /// <summary>Recorded and edited (comped, timed, tuned) - finished.</summary>
        Edited
    }
}
