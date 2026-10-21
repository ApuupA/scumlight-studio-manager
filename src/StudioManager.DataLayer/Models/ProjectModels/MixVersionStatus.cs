namespace StudioManager.DataLayer.Models.ProjectModels
{
    public enum MixVersionStatus
    {
        /// <summary>Bounced, but not sent to the client yet.</summary>
        InProgress,
        /// <summary>Sent to the client, waiting for feedback.</summary>
        Sent,
        /// <summary>The client wants changes - the next version follows.</summary>
        ChangesRequested,
        /// <summary>The client is happy - this is the final mix.</summary>
        Approved,
        /// <summary>
        /// All feedback of this version is worked in and the next version is made (e.g. v1.0 is done, v2.0 is the newest).
        /// Shown folded away. Stored as text, so no database change is needed for this value.
        /// </summary>
        Completed
    }
}
