using StudioManager.DataLayer.Models.SessionModels;

namespace StudioManager.DataLayer.DTO.Read
{
    public class SessionReadDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public SessionType Type { get; set; }
        public SessionStatus Status { get; set; }
        public DateOnly Date { get; set; }
        public DateOnly? EndDate { get; set; }
        public TimeOnly? StartTime { get; set; }
        public TimeOnly? EndTime { get; set; }
        public bool IsAllDay { get; set; }
        public string? Engineer { get; set; }
        public string? Attendees { get; set; }
        public string? Goals { get; set; }
        public string? Summary { get; set; }
        public string? NextSteps { get; set; }
        public string? Notes { get; set; }

        public int ProjectId { get; set; }
        public string ProjectTitle { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;

        /// <summary>Ids of the songs worked on in this session.</summary>
        public List<int> SongIds { get; set; } = new();

        public int TaskCount { get; set; }
        public int TasksDoneCount { get; set; }

        /// <summary>Instruments recorded in this session (id + name).</summary>
        public List<InstrumentShortDto> Instruments { get; set; } = new();

        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
    }
}
