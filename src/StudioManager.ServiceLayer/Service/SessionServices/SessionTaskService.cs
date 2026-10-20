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

        public async Task<IEnumerable<OpenSessionTaskDto>> GetOpen()
        {
            var tasks = await _repository.GetAllWithPathsAsync(
                t => !t.IsDone && t.Session!.Status != SessionStatus.Cancelled, "Session.Project");

            return tasks
                .OrderBy(t => t.Session!.Date).ThenBy(t => t.SessionId).ThenBy(t => t.SortOrder)
                .Select(t => new OpenSessionTaskDto
                {
                    Id = t.Id,
                    Text = t.Text,
                    SortOrder = t.SortOrder,
                    SessionId = t.SessionId,
                    SessionTitle = t.Session!.Title,
                    SessionDate = t.Session.Date,
                    ProjectTitle = t.Session.Project?.Title ?? string.Empty
                });
        }
    }
}
