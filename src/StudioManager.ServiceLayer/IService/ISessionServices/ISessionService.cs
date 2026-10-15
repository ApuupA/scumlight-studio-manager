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
    }
}
