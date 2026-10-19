using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.SessionModels;
using StudioManager.DataLayer.Models.UserModels;
using StudioManager.RepositoryLayer.IRepository;
using StudioManager.ServiceLayer.Exceptions;
using StudioManager.ServiceLayer.IService.ICalendarServices;
using System.Security.Cryptography;
using System.Text;

namespace StudioManager.ServiceLayer.Service.CalendarServices
{
    /// <summary>
    /// Writes the sessions in the iCalendar format (RFC 5545) - plain text, so no library is needed.
    /// Times are written without time zone ("floating"): the calendar shows them as they are, e.g. 9:00.
    /// </summary>
    public class CalendarService : ICalendarService
    {
        /// <summary>Sessions older than this are left out - a calendar does not need the whole history.</summary>
        public const int DaysInThePast = 180;

        private readonly IRepository<User> _userRepository;
        private readonly IRepository<Session> _sessionRepository;

        public CalendarService(IRepository<User> userRepository, IRepository<Session> sessionRepository)
        {
            _userRepository = userRepository;
            _sessionRepository = sessionRepository;
        }

        public async Task<CalendarTokenDto> GetToken(int userId)
        {
            var user = await _userRepository.GetAsync(userId) ?? throw new EntityNotFoundException(nameof(User), userId);
            return new CalendarTokenDto { Token = user.CalendarToken };
        }

        public async Task<CalendarTokenDto> CreateToken(int userId)
        {
            var user = await _userRepository.GetAsync(userId) ?? throw new EntityNotFoundException(nameof(User), userId);

            // 32 random bytes as URL-safe text - impossible to guess
            user.CalendarToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
                .Replace('+', '-').Replace('/', '_').TrimEnd('=');
            await _userRepository.UpdateAsync(user);

            return new CalendarTokenDto { Token = user.CalendarToken };
        }

        public async Task<string?> GetFeed(string token)
        {
            if (string.IsNullOrWhiteSpace(token) || !await _userRepository.ExistsAsync(u => u.CalendarToken == token))
            {
                return null;
            }

            var from = DateOnly.FromDateTime(DateTime.Now).AddDays(-DaysInThePast);
            var sessions = (await _sessionRepository.GetAllWithPathsAsync(s => (s.EndDate ?? s.Date) >= from, "Project.Client"))
                .OrderBy(s => s.Date)
                .ThenBy(s => s.StartTime);

            var lines = new List<string>
            {
                "BEGIN:VCALENDAR",
                "VERSION:2.0",
                "PRODID:-//Studio Manager//Sessions//EN",
                "CALSCALE:GREGORIAN",
                "METHOD:PUBLISH",
                "X-WR-CALNAME:Studio Manager – Sessions",
                // Ask the calendar app to check for changes every hour
                "REFRESH-INTERVAL;VALUE=DURATION:PT1H",
                "X-PUBLISHED-TTL:PT1H"
            };

            foreach (var session in sessions)
            {
                lines.AddRange(EventLines(session));
            }

            lines.Add("END:VCALENDAR");

            // iCalendar wants CRLF line endings and lines of at most 75 bytes
            return string.Join("\r\n", lines.Select(Fold)) + "\r\n";
        }

        /// <summary>
        /// One session as calendar event. A session with times over several days becomes a daily repeating event,
        /// so every day shows e.g. 9:00 - 17:00.
        /// </summary>
        private static IEnumerable<string> EventLines(Session session)
        {
            var project = session.Project;
            var title = project != null ? $"{session.Title} · {project.Title}" : session.Title;
            var details = new[]
            {
                project?.Client != null ? $"Client: {project.Client.Name}" : null,
                $"Type: {session.Type}",
                session.Engineer != null ? $"Engineer: {session.Engineer}" : null,
                session.Attendees != null ? $"Attendees: {session.Attendees}" : null,
                session.Goals != null ? $"Goals: {session.Goals}" : null,
                session.Notes
            }.Where(text => !string.IsNullOrWhiteSpace(text));

            yield return "BEGIN:VEVENT";
            yield return $"UID:session-{session.Id}@studio-manager";
            yield return $"DTSTAMP:{session.DateUpdated.ToUniversalTime():yyyyMMdd'T'HHmmss'Z'}";
            yield return $"LAST-MODIFIED:{session.DateUpdated.ToUniversalTime():yyyyMMdd'T'HHmmss'Z'}";
            yield return $"SUMMARY:{Escape(title)}";
            yield return $"DESCRIPTION:{Escape(string.Join("\n", details))}";
            yield return $"STATUS:{StatusOf(session.Status)}";

            if (session.IsAllDay)
            {
                // All-day: the end date is the day AFTER the last day (iCalendar rule)
                yield return $"DTSTART;VALUE=DATE:{session.Date:yyyyMMdd}";
                yield return $"DTEND;VALUE=DATE:{session.LastDate.AddDays(1):yyyyMMdd}";
            }
            else
            {
                var start = session.Date.ToDateTime(session.StartTime!.Value);
                var end = session.Date.ToDateTime(session.EndTime!.Value);
                if (end <= start)
                {
                    end = end.AddDays(1); // e.g. 20:00 - 02:00 ends the next morning
                }

                yield return $"DTSTART:{start:yyyyMMdd'T'HHmmss}";
                yield return $"DTEND:{end:yyyyMMdd'T'HHmmss}";

                var days = session.LastDate.DayNumber - session.Date.DayNumber + 1;
                if (days > 1)
                {
                    yield return $"RRULE:FREQ=DAILY;COUNT={days}";
                }
            }

            yield return "END:VEVENT";
        }

        private static string StatusOf(SessionStatus status) => status switch
        {
            SessionStatus.Planned => "TENTATIVE",
            SessionStatus.Cancelled => "CANCELLED",
            _ => "CONFIRMED"
        };

        /// <summary>
        /// Texts must not contain raw commas, semicolons, backslashes or line breaks.
        /// </summary>
        private static string Escape(string text) => text
            .Replace("\\", "\\\\")
            .Replace(";", "\\;")
            .Replace(",", "\\,")
            .Replace("\r\n", "\\n")
            .Replace("\n", "\\n");

        /// <summary>
        /// Lines longer than 75 bytes are split; the continuation lines start with a space (iCalendar rule).
        /// </summary>
        private static string Fold(string line)
        {
            const int maxBytes = 75;
            var result = new StringBuilder();
            var lineBytes = 0;

            foreach (var character in line)
            {
                var size = Encoding.UTF8.GetByteCount(character.ToString());
                if (lineBytes + size > maxBytes)
                {
                    result.Append("\r\n ");
                    lineBytes = 1; // the space at the start of the continuation line counts too
                }

                result.Append(character);
                lineBytes += size;
            }

            return result.ToString();
        }
    }
}
