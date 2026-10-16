using StudioManager.DataLayer.Models.SessionModels;

namespace StudioManager.DataLayer.DTO.Create
{
    /// <summary>
    /// Changes only the status of a session (e.g. "Completed" directly from the calendar).
    /// </summary>
    public class SessionStatusUpdateDto
    {
        public SessionStatus Status { get; set; }
    }
}
