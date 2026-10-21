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
        Approved
    }
}
