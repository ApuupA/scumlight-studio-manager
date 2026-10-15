using AutoMapper;
using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.SessionModels;
using StudioManager.RepositoryLayer.IRepository;
using StudioManager.ServiceLayer.IService.ISessionServices;

namespace StudioManager.ServiceLayer.Service.SessionServices
{
    public class SessionTaskService : BaseService<SessionTask, SessionTaskReadDto, SessionTaskCreateDto>, ISessionTaskService
    {
        public SessionTaskService(IRepository<SessionTask> repository, IMapper mapper) : base(repository, mapper)
        {
        }

        public async Task<IEnumerable<SessionTaskReadDto>> GetForSession(int sessionId)
        {
            var tasks = await GetAllWhere(t => t.SessionId == sessionId);
            return tasks.OrderBy(t => t.SortOrder).ThenBy(t => t.Id);
        }
    }
}
