using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.FinanceModels;

namespace StudioManager.ServiceLayer.IService.IFinanceServices
{
    /// <summary>
    /// Working time on projects - entered by hand, from a session or with the timer.
    /// </summary>
    public interface ITimeEntryService : IBaseService<TimeEntry, TimeEntryReadDto, TimeEntryCreateDto>
    {
        /// <summary>The entries of a project, newest first.</summary>
        Task<IEnumerable<TimeEntryReadDto>> GetForProject(int projectId);

        /// <summary>The running timer of the user, null if none is running.</summary>
        Task<TimeEntryReadDto?> GetRunningTimer(int userId);

        /// <summary>Starts a timer - only one per user can run.</summary>
        Task<TimeEntryReadDto> StartTimer(int userId, TimerStartDto dto);

        /// <summary>Stops the user's timer: the time is rounded up to the quarter hour and saved.</summary>
        Task<TimeEntryReadDto> StopTimer(int userId, TimerStopDto dto);
    }
}
