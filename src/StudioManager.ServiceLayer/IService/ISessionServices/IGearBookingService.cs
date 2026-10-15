using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.SessionModels;

namespace StudioManager.ServiceLayer.IService.ISessionServices
{
    /// <summary>
    /// Gear bookings of sessions. Every returned booking contains the conflict warnings
    /// (overbooked at the same time, gear not available).
    /// </summary>
    public interface IGearBookingService : IBaseService<GearBooking, GearBookingReadDto, GearBookingCreateDto>
    {
        Task<IEnumerable<GearBookingReadDto>> GetForSession(int sessionId);

        /// <summary>
        /// All bookings of all sessions of a project - the studio gear used in the project.
        /// </summary>
        Task<IEnumerable<GearBookingReadDto>> GetForProject(int projectId);

        /// <summary>
        /// Bookings of one gear item from today on (not cancelled) - shown on the gear detail page.
        /// </summary>
        Task<IEnumerable<GearBookingReadDto>> GetUpcomingForGearItem(int gearItemId);
    }
}
