using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.SessionModels;

namespace StudioManager.ServiceLayer.IService.ISessionServices
{
    public interface ISessionTaskService : IBaseService<SessionTask, SessionTaskReadDto, SessionTaskCreateDto>
    {
        /// <summary>
        /// All SessionTask entries of one session.
        /// </summary>
        Task<IEnumerable<SessionTaskReadDto>> GetForSession(int sessionId);

        /// <summary>
        /// All to-dos that are not done yet (cancelled sessions left out), oldest session first.
        /// </summary>
        Task<IEnumerable<OpenSessionTaskDto>> GetOpen();
    }
}
