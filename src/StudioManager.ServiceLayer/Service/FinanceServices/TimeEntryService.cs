using AutoMapper;
using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.FinanceModels;
using StudioManager.DataLayer.Models.ProjectModels;
using StudioManager.DataLayer.Models.SessionModels;
using StudioManager.DataLayer.Models.UserModels;
using StudioManager.RepositoryLayer.IRepository;
using StudioManager.ServiceLayer.Exceptions;
using StudioManager.ServiceLayer.IService.IFinanceServices;
using System.Linq.Expressions;

namespace StudioManager.ServiceLayer.Service.FinanceServices
{
    public class TimeEntryService : BaseService<TimeEntry, TimeEntryReadDto, TimeEntryCreateDto>, ITimeEntryService
    {
        /// <summary>A forgotten timer is cut at one day - the entry can be corrected afterwards.</summary>
        public const decimal MaxTimerHours = 24;

        private readonly IRepository<Project> _projectRepository;
        private readonly IRepository<Session> _sessionRepository;
        private readonly IRepository<User> _userRepository;

        public TimeEntryService(
            IRepository<TimeEntry> repository,
            IRepository<Project> projectRepository,
            IRepository<Session> sessionRepository,
            IRepository<User> userRepository,
            IMapper mapper) : base(repository, mapper)
        {
            _projectRepository = projectRepository;
            _sessionRepository = sessionRepository;
            _userRepository = userRepository;
        }

        // Load project, session and user, so their names can be shown
        protected override Expression<Func<TimeEntry, object>>[] DefaultIncludes =>
            [t => t.Project!, t => t.Session!, t => t.User!];

        public async Task<IEnumerable<TimeEntryReadDto>> GetForProject(int projectId)
        {
            var entries = await GetAllWhere(t => t.ProjectId == projectId);
            return entries.OrderByDescending(t => t.Date).ThenByDescending(t => t.Id);
        }

        public override async Task<TimeEntryReadDto> Insert(TimeEntryCreateDto createDto)
        {
            await Validate(createDto.ProjectId, createDto.SessionId, createDto.UserId);
            createDto.Description = createDto.Description.Trim();
            return await base.Insert(createDto);
        }

        public override async Task<TimeEntryReadDto> Update(int id, TimeEntryCreateDto updateDto)
        {
            await Validate(updateDto.ProjectId, updateDto.SessionId, updateDto.UserId);
            updateDto.Description = updateDto.Description.Trim();
            return await base.Update(id, updateDto);
        }

        public async Task<TimeEntryReadDto?> GetRunningTimer(int userId)
        {
            return (await GetAllWhere(t => t.UserId == userId && t.StartedAt != null)).FirstOrDefault();
        }

        public async Task<TimeEntryReadDto> StartTimer(int userId, TimerStartDto dto)
        {
            if (await _repository.ExistsAsync(t => t.UserId == userId && t.StartedAt != null))
            {
                throw new BusinessRuleException("timeEntry.timerRunning", "A timer is already running - stop it first.");
            }

            await Validate(dto.ProjectId, dto.SessionId, userId);

            var entry = new TimeEntry
            {
                Date = dto.Date,
                Description = dto.Description?.Trim() ?? string.Empty,
                ProjectId = dto.ProjectId,
                SessionId = dto.SessionId,
                UserId = userId,
                StartedAt = DateTime.UtcNow
            };
            await _repository.AddAsync(entry);

            return (await Get(entry.Id))!;
        }

        public async Task<TimeEntryReadDto> StopTimer(int userId, TimerStopDto dto)
        {
            var entry = await _repository.GetAsync(t => t.UserId == userId && t.StartedAt != null)
                ?? throw new BusinessRuleException("timeEntry.noTimerRunning", "No timer is running.");

            entry.Hours = RoundUpToQuarterHour(DateTime.UtcNow - entry.StartedAt!.Value);
            entry.Description = dto.Description.Trim();
            entry.StartedAt = null;
            await _repository.UpdateAsync(entry);

            return (await Get(entry.Id))!;
        }

        /// <summary>
        /// 1 h 05 min becomes 1.25 h - at least 0.25 h, at most one day.
        /// </summary>
        public static decimal RoundUpToQuarterHour(TimeSpan elapsed)
        {
            var quarters = Math.Ceiling((decimal)elapsed.TotalMinutes / 15m);
            return Math.Clamp(quarters * 0.25m, 0.25m, MaxTimerHours);
        }

        /// <summary>
        /// The project must exist, the session must belong to it, the user must exist.
        /// </summary>
        private async Task Validate(int projectId, int? sessionId, int? userId)
        {
            if (!await _projectRepository.ExistsAsync(p => p.Id == projectId))
            {
                throw new BusinessRuleException("timeEntry.projectNotFound", $"Project with id {projectId} does not exist.");
            }

            if (sessionId != null && !await _sessionRepository.ExistsAsync(s => s.Id == sessionId && s.ProjectId == projectId))
            {
                throw new BusinessRuleException("timeEntry.sessionNotInProject", "The session does not belong to this project.");
            }

            if (userId != null && !await _userRepository.ExistsAsync(u => u.Id == userId))
            {
                throw new BusinessRuleException("timeEntry.userNotFound", $"User with id {userId} does not exist.");
            }
        }
    }
}
