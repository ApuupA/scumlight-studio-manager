using StudioManager.DataLayer.Models.ProjectModels;

namespace StudioManager.DataLayer.Models.SessionModels
{
    /// <summary>
    /// A block of studio time for a project: either a whole day (StartTime / EndTime are null)
    /// or a time range on one day, e.g. a mixing session from 18:00 to 22:00.
    /// </summary>
    public class Session : BaseEntity
    {
        public string Title { get; set; } = string.Empty;
        public SessionType Type { get; set; } = SessionType.Recording;
        public SessionStatus Status { get; set; } = SessionStatus.Planned;

        public DateOnly Date { get; set; }
        public TimeOnly? StartTime { get; set; }
        public TimeOnly? EndTime { get; set; }

        /// <summary>
        /// True if the session takes the whole day (no times set).
        /// </summary>
        public bool IsAllDay => StartTime == null || EndTime == null;

        public string? Engineer { get; set; }

        /// <summary>Who is there, e.g. "Drummer + guitarist".</summary>
        public string? Attendees { get; set; }

        /// <summary>What should be done in this session (overview, the details are the tasks).</summary>
        public string? Goals { get; set; }

        /// <summary>Where are we now: what was achieved, what is open.</summary>
        public string? Summary { get; set; }

        public string? NextSteps { get; set; }
        public string? Notes { get; set; }

        public int ProjectId { get; set; }
        public Project? Project { get; set; }

        public ICollection<SessionTask> Tasks { get; set; } = new List<SessionTask>();
        public ICollection<GearBooking> GearBookings { get; set; } = new List<GearBooking>();

        /// <summary>
        /// Songs worked on in this session (many-to-many).
        /// </summary>
        public ICollection<Song> Songs { get; set; } = new List<Song>();
    }
}
