using StudioManager.DataLayer.Models.ProjectModels;

namespace StudioManager.DataLayer.DTO.Read
{
    public class ProjectReadDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public ProjectType Type { get; set; }
        public ProjectStatus Status { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? DueDate { get; set; }
        public string? Description { get; set; }
        public int? SampleRate { get; set; }
        public int? BitDepth { get; set; }
        public string? Daw { get; set; }
        public decimal? TargetLufs { get; set; }
        public string? RecordingNotes { get; set; }
        public string? MixingNotes { get; set; }
        public string? MasteringNotes { get; set; }

        public int ClientId { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public List<BandShortDto> Bands { get; set; } = new();

        // ---------- Progress overview ----------
        public int SongCount { get; set; }

        /// <summary>Songs with status Done - for the progress bar.</summary>
        public int SongsDoneCount { get; set; }

        /// <summary>Scope of the project - decides which tabs are shown.</summary>
        public bool IncludesSongwriting { get; set; }
        public bool IncludesRecording { get; set; }
        public bool IncludesMixing { get; set; }
        public bool IncludesMastering { get; set; }

        /// <summary>The material for the mix (tab "Mixing").</summary>
        public DateOnly? MaterialReceivedOn { get; set; }
        public string? MaterialLink { get; set; }
        public int? MaterialTrackCount { get; set; }
        public string? MaterialNotes { get; set; }

        public int SessionCount { get; set; }

        /// <summary>Date of the next upcoming (not cancelled) session, null if none is planned.</summary>
        public DateOnly? NextSessionDate { get; set; }

        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }

        /// <summary>True if a picture was uploaded (GET .../{id}/photo).</summary>
        public bool HasPhoto { get; set; }
    }
}
