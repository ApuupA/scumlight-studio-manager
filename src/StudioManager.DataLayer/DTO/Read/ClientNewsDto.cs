namespace StudioManager.DataLayer.DTO.Read
{
    /// <summary>
    /// What clients wrote on the client page and the studio has not seen yet - one entry per project (start page).
    /// </summary>
    public class ClientNewsDto
    {
        public int ProjectId { get; set; }
        public string ProjectTitle { get; set; } = string.Empty;

        /// <summary>New comments and approvals of mix versions.</summary>
        public int FeedbackCount { get; set; }

        /// <summary>New reference links.</summary>
        public int ReferenceCount { get; set; }

        /// <summary>Who wrote, e.g. ["Simon", "Lena"].</summary>
        public List<string> Authors { get; set; } = new();

        public DateTime LatestAt { get; set; }
    }
}
