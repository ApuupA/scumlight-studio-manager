using StudioManager.DataLayer.DTO.Read;

namespace StudioManager.ServiceLayer.IService.ICalendarServices
{
    /// <summary>
    /// Calendar subscription: the sessions as iCalendar feed (.ics) for Nextcloud, phones, Outlook, ...
    /// Calendar apps can not log in, so every user gets a secret link that can be renewed at any time.
    /// </summary>
    public interface ICalendarService
    {
        /// <summary>
        /// The current secret of the user's link, null if none was created yet.
        /// </summary>
        Task<CalendarTokenDto> GetToken(int userId);

        /// <summary>
        /// Creates a new secret - the old link stops working.
        /// </summary>
        Task<CalendarTokenDto> CreateToken(int userId);

        /// <summary>
        /// The sessions as iCalendar text, null if the secret is unknown.
        /// </summary>
        Task<string?> GetFeed(string token);
    }
}
