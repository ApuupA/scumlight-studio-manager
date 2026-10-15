using StudioManager.DataLayer.Models.SessionModels;

namespace StudioManager.DataLayer.DTO.Read
{
    /// <summary>
    /// The most important data of a session, used inside other DTOs (e.g. conflicting sessions of a gear booking).
    /// </summary>
    public class SessionShortDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public SessionStatus Status { get; set; }
        public DateOnly Date { get; set; }
        public TimeOnly? StartTime { get; set; }
        public TimeOnly? EndTime { get; set; }
        public int ProjectId { get; set; }
        public string ProjectTitle { get; set; } = string.Empty;
    }
}
