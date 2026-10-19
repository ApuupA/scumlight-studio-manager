using StudioManager.DataLayer.Models.ProjectModels;
using StudioManager.DataLayer.Models.SessionModels;
using StudioManager.DataLayer.Models.UserModels;

namespace StudioManager.DataLayer.Models.FinanceModels
{
    /// <summary>
    /// Working time on a project, e.g. 6.5 hours "Drum recording" on 30.10. - optionally of a session.
    /// A running timer is an entry with StartedAt set and 0 hours; stopping it fills in the hours.
    /// </summary>
    public class TimeEntry : BaseEntity
    {
        public DateOnly Date { get; set; }

        public decimal Hours { get; set; }

        public string Description { get; set; } = string.Empty;

        /// <summary>False for time that is not charged, e.g. own preparation.</summary>
        public bool IsBillable { get; set; } = true;

        /// <summary>Start of a running timer (UTC). Null for normal entries.</summary>
        public DateTime? StartedAt { get; set; }

        public int ProjectId { get; set; }
        public Project? Project { get; set; }

        public int? SessionId { get; set; }
        public Session? Session { get; set; }

        /// <summary>Who worked. Null if the user was deleted.</summary>
        public int? UserId { get; set; }
        public User? User { get; set; }
    }
}
