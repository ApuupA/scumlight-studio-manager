using AutoMapper;
using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.ProjectModels;
using StudioManager.DataLayer.Models.SessionModels;
using StudioManager.RepositoryLayer.IRepository;
using StudioManager.ServiceLayer.Exceptions;
using StudioManager.ServiceLayer.IService.ISessionServices;
using System.Linq.Expressions;

namespace StudioManager.ServiceLayer.Service.SessionServices
{
    public class SessionService : BaseService<Session, SessionReadDto, SessionCreateDto>, ISessionService
    {
        private readonly IRepository<Project> _projectRepository;
        private readonly IRepository<Song> _songRepository;

        public SessionService(
            IRepository<Session> repository,
            IRepository<Project> projectRepository,
            IRepository<Song> songRepository,
            IMapper mapper) : base(repository, mapper)
        {
            _projectRepository = projectRepository;
            _songRepository = songRepository;
        }

        // Project + client for the names, the rest for the counters in the DTO
        protected override Expression<Func<Session, object>>[] DefaultIncludes =>
            [s => s.Project!.Client!, s => s.Songs, s => s.Tasks, s => s.GearBookings];

        public override async Task<IEnumerable<SessionReadDto>> GetAll()
        {
            return SortByTime(await base.GetAll());
        }

        public async Task<IEnumerable<SessionReadDto>> GetForProject(int projectId)
        {
            return SortByTime(await GetAllWhere(s => s.ProjectId == projectId));
        }

        public override async Task<SessionReadDto> Insert(SessionCreateDto createDto)
        {
            EnsureValidTimes(createDto);
            await EnsureProjectExists(createDto.ProjectId);
            var songs = await LoadSongsOfProject(createDto);

            var session = _mapper.Map<Session>(createDto);
            foreach (var song in songs)
            {
                session.Songs.Add(song);
            }

            await _repository.AddAsync(session);
            return (await Get(session.Id))!;
        }

        public override async Task<SessionReadDto> Update(int id, SessionCreateDto updateDto)
        {
            // Load the current songs too, so EF knows which song links to remove
            var session = await _repository.GetAsync(s => s.Id == id, s => s.Songs)
                ?? throw new EntityNotFoundException(nameof(Session), id);

            EnsureValidTimes(updateDto);
            await EnsureProjectExists(updateDto.ProjectId);
            var songs = await LoadSongsOfProject(updateDto);

            _mapper.Map(updateDto, session);
            session.Songs.Clear();
            foreach (var song in songs)
            {
                session.Songs.Add(song);
            }

            await _repository.UpdateAsync(session);
            return (await Get(id))!;
        }

        /// <summary>
        /// Either no times (all-day session) or both, with the end after the start.
        /// Sessions over midnight are not supported - plan them as all-day or split them.
        /// </summary>
        private static void EnsureValidTimes(SessionCreateDto dto)
        {
            var onlyOneTimeSet = (dto.StartTime == null) != (dto.EndTime == null);
            var endNotAfterStart = dto.StartTime != null && dto.EndTime != null && dto.EndTime <= dto.StartTime;

            if (onlyOneTimeSet || endNotAfterStart)
            {
                throw new BusinessRuleException("session.invalidTimes",
                    "Set start and end time (end after start), or none for an all-day session.");
            }
        }

        private async Task EnsureProjectExists(int projectId)
        {
            if (!await _projectRepository.ExistsAsync(p => p.Id == projectId))
            {
                throw new BusinessRuleException("session.projectNotFound", $"Project with id {projectId} does not exist.");
            }
        }

        /// <summary>
        /// Loads the songs of the DTO and makes sure all of them belong to the session's project.
        /// </summary>
        private async Task<List<Song>> LoadSongsOfProject(SessionCreateDto dto)
        {
            var songIds = dto.SongIds.Distinct().ToList();
            var songs = (await _songRepository.GetAllAsync(s => songIds.Contains(s.Id) && s.ProjectId == dto.ProjectId)).ToList();

            if (songs.Count != songIds.Count)
            {
                throw new BusinessRuleException("session.songNotInProject", "All songs of a session must belong to its project.");
            }

            return songs;
        }

        private static IEnumerable<SessionReadDto> SortByTime(IEnumerable<SessionReadDto> sessions)
        {
            // All-day sessions (no start time) first on their day
            return sessions.OrderBy(s => s.Date).ThenBy(s => s.StartTime ?? TimeOnly.MinValue);
        }
    }
}
