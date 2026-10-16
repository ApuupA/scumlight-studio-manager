using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.SessionModels;

namespace StudioManager.ServiceLayer.IService.ISessionServices
{
    public interface ISessionService : IBaseService<Session, SessionReadDto, SessionCreateDto>
    {
        /// <summary>
        /// All Session entries of one project.
        /// </summary>
        Task<IEnumerable<SessionReadDto>> GetForProject(int projectId);

        /// <summary>
        /// Copies a session (with songs, to-dos and gear) to the given dates. Returns the new sessions.
        /// </summary>
        Task<IEnumerable<SessionReadDto>> Duplicate(int id, SessionDuplicateDto dto);

        /// <summary>
        /// Changes only the status, everything else stays as it is.
        /// </summary>
        Task<SessionReadDto> SetStatus(int id, SessionStatusUpdateDto dto);
    }
}
