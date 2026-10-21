using StudioManager.DataLayer.Models.ProjectModels;

namespace StudioManager.DataLayer.DTO.Read
{
    /// <summary>
    /// What the client sees on the public client page (link without login): the mix versions with feedback and the
    /// references. Only what the client needs - no prices, no internal notes of the songs, no other projects.
    /// The gear of the band comes from the band gear endpoints (same link).
    /// </summary>
    public class ClientPageDto
    {
        public string ProjectTitle { get; set; } = string.Empty;

        /// <summary>The bands of the project, e.g. "Sacroscum".</summary>
        public string BandNames { get; set; } = string.Empty;

        /// <summary>Tabs of the page - from the project scope.</summary>
        public bool ShowMixing { get; set; }
        public bool ShowDemos { get; set; }
        public bool ShowGear { get; set; }

        /// <summary>All songs of the project, for choosing the song of a comment or a reference.</summary>
        public List<SongShortDto> Songs { get; set; } = new();

        /// <summary>Mix versions and demos (IsDemo), newest first.</summary>
        public List<ClientMixVersionDto> MixVersions { get; set; } = new();

        public List<ClientReferenceDto> References { get; set; } = new();
    }

    public class ClientMixVersionDto
    {
        public int Id { get; set; }
        public bool IsDemo { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateOnly Date { get; set; }
        public MixVersionStatus Status { get; set; }
        public string? Link { get; set; }

        /// <summary>What changed in this version.</summary>
        public string? Notes { get; set; }

        public bool IsAllSongs { get; set; }
        public List<SongShortDto> Songs { get; set; } = new();
        public string? ApprovedBy { get; set; }
        public List<ClientFeedbackDto> Feedback { get; set; } = new();
    }

    public class ClientFeedbackDto
    {
        public string? Author { get; set; }
        public bool IsFromClient { get; set; }
        public string? SongTitle { get; set; }
        public string? Timecode { get; set; }
        public string Text { get; set; } = string.Empty;
        public bool IsDone { get; set; }
        public DateTime DateCreated { get; set; }
    }

    public class ClientReferenceDto
    {
        public string Title { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public string? SongTitle { get; set; }
        public string? AddedBy { get; set; }
    }
}
