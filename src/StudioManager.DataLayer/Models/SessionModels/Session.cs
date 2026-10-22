using StudioManager.DataLayer.Models.UserModels;
using StudioManager.DataLayer.Models.ProjectModels;

namespace StudioManager.DataLayer.Models.SessionModels
{
    /// <summary>
    /// A block of studio time for a project, on one day or over several days (Date to EndDate).
    /// Either all-day (StartTime / EndTime are null) or with times that apply to every day,
    /// e.g. a mixing session from 18:00 to 22:00 or an album production Fri - Sat, 9:00 to 17:00.
    /// </summary>
    public class Session : BaseEntity
    {
        public string Title { get; set; } = string.Empty;
        public SessionType Type { get; set; } = SessionType.Recording;
        public SessionStatus Status { get; set; } = SessionStatus.Planned;

        /// <summary>The (first) day of the session.</summary>
        public DateOnly Date { get; set; }

        /// <summary>Last day of a multi-day session, null for a single day.</summary>
        public DateOnly? EndDate { get; set; }

        public TimeOnly? StartTime { get; set; }
        public TimeOnly? EndTime { get; set; }

        /// <summary>
        /// True if the session takes the whole day (no times set).
        /// </summary>
        public bool IsAllDay => StartTime == null || EndTime == null;

        /// <summary>
        /// The last day of the session: EndDate for multi-day sessions, otherwise Date.
        /// </summary>
        public DateOnly LastDate => EndDate ?? Date;

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

        /// <summary>The input list used in this session (one of the project's input lists), null if none.</summary>
        public int? InputListId { get; set; }
        public InputList? InputList { get; set; }

        public ICollection<SessionTask> Tasks { get; set; } = new List<SessionTask>();

        /// <summary>The assistant who created the session - shown to the owner ("by Assi"). Null = created by an owner.</summary>
        public int? CreatedByUserId { get; set; }
        public User? CreatedByUser { get; set; }

        /// <summary>
        /// Instruments recorded in this session (many-to-many) - the session's gear comes from their setups.
        /// </summary>
        public ICollection<ProjectInstrument> Instruments { get; set; } = new List<ProjectInstrument>();

        /// <summary>
        /// Songs worked on in this session (many-to-many).
        /// </summary>
        public ICollection<Song> Songs { get; set; } = new List<Song>();
    }
}
