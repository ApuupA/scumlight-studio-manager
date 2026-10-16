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
        private readonly IRepository<ProjectInstrument> _instrumentRepository;
        private readonly IRepository<InputList> _inputListRepository;

        public SessionService(
            IRepository<Session> repository,
            IRepository<Project> projectRepository,
            IRepository<Song> songRepository,
            IRepository<ProjectInstrument> instrumentRepository,
            IRepository<InputList> inputListRepository,
            IMapper mapper) : base(repository, mapper)
        {
            _projectRepository = projectRepository;
            _songRepository = songRepository;
            _instrumentRepository = instrumentRepository;
            _inputListRepository = inputListRepository;
        }

        // Project + client for the names, the rest for the counters in the DTO
        protected override Expression<Func<Session, object>>[] DefaultIncludes =>
            [s => s.Project!.Client!, s => s.Songs, s => s.Tasks, s => s.Instruments, s => s.InputList!];

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
            EnsureValidDatesAndTimes(createDto);
            await EnsureProjectExists(createDto.ProjectId);
            await EnsureInputListIsInProject(createDto);
            var songs = await LoadSongsOfProject(createDto);
            var instruments = await LoadInstrumentsOfProject(createDto);

            var session = _mapper.Map<Session>(createDto);
            foreach (var song in songs)
            {
                session.Songs.Add(song);
            }
            foreach (var instrument in instruments)
            {
                session.Instruments.Add(instrument);
            }

            await _repository.AddAsync(session);
            return (await Get(session.Id))!;
        }

        public override async Task<SessionReadDto> Update(int id, SessionCreateDto updateDto)
        {
            // Load the current songs and instruments too, so EF knows which links to remove
            var session = await _repository.GetAsync(s => s.Id == id, s => s.Songs, s => s.Instruments)
                ?? throw new EntityNotFoundException(nameof(Session), id);

            EnsureValidDatesAndTimes(updateDto);
            await EnsureProjectExists(updateDto.ProjectId);
            await EnsureInputListIsInProject(updateDto);
            var songs = await LoadSongsOfProject(updateDto);
            var instruments = await LoadInstrumentsOfProject(updateDto);

            _mapper.Map(updateDto, session);
            session.Songs.Clear();
            foreach (var song in songs)
            {
                session.Songs.Add(song);
            }
            session.Instruments.Clear();
            foreach (var instrument in instruments)
            {
                session.Instruments.Add(instrument);
            }

            await _repository.UpdateAsync(session);
            return (await Get(id))!;
        }

        public async Task<SessionReadDto> SetStatus(int id, SessionStatusUpdateDto dto)
        {
            var session = await _repository.GetAsync(id) ?? throw new EntityNotFoundException(nameof(Session), id);

            session.Status = dto.Status;
            await _repository.UpdateAsync(session);

            return (await Get(id))!;
        }

        /// <summary>
        /// Copies a session to one or more other dates, e.g. "drums day 1" to the next four days.
        /// Copied: everything planned (title, type, times, length in days, people, goals, notes, songs, to-dos,
        /// instruments and thereby their gear, input list). Not copied: what happened in the session (summary, next steps) - and all to-dos start open.
        /// </summary>
        public async Task<IEnumerable<SessionReadDto>> Duplicate(int id, SessionDuplicateDto dto)
        {
            var source = await _repository.GetAsync(s => s.Id == id, s => s.Songs, s => s.Tasks, s => s.Instruments)
                ?? throw new EntityNotFoundException(nameof(Session), id);

            var lengthInDays = source.LastDate.DayNumber - source.Date.DayNumber;
            var copies = new List<Session>();

            foreach (var date in dto.Dates.Distinct().Order())
            {
                var copy = new Session
                {
                    Title = source.Title,
                    Type = source.Type,
                    Status = SessionStatus.Planned,
                    Date = date,
                    EndDate = lengthInDays > 0 ? date.AddDays(lengthInDays) : null,
                    StartTime = source.StartTime,
                    EndTime = source.EndTime,
                    Engineer = source.Engineer,
                    Attendees = source.Attendees,
                    Goals = source.Goals,
                    Notes = source.Notes,
                    ProjectId = source.ProjectId,
                    InputListId = source.InputListId,
                    Songs = source.Songs.ToList(),
                    Tasks = source.Tasks
                        .Select(t => new SessionTask { Text = t.Text, SortOrder = t.SortOrder, IsDone = false })
                        .ToList(),
                    Instruments = source.Instruments.ToList()
                };

                await _repository.AddAsync(copy);
                copies.Add(copy);
            }

            var copyIds = copies.Select(c => c.Id).ToList();
            return SortByTime(await GetAllWhere(s => copyIds.Contains(s.Id)));
        }

        /// <summary>
        /// Dates: an end date must be after the start date (the same day counts as a single-day session).
        /// Times: either none (all-day session) or both, with the end after the start - they apply to every day.
        /// Sessions over midnight are not supported - plan them as all-day or split them.
        /// </summary>
        private static void EnsureValidDatesAndTimes(SessionCreateDto dto)
        {
            if (dto.EndDate == dto.Date)
            {
                dto.EndDate = null;
            }

            if (dto.EndDate < dto.Date)
            {
                throw new BusinessRuleException("session.invalidEndDate", "The end date must be after the start date.");
            }

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
        private async Task EnsureInputListIsInProject(SessionCreateDto dto)
        {
            if (dto.InputListId != null
                && !await _inputListRepository.ExistsAsync(l => l.Id == dto.InputListId && l.ProjectId == dto.ProjectId))
            {
                throw new BusinessRuleException("session.inputListNotInProject", "The input list must belong to the session's project.");
            }
        }

        /// <summary>
        /// Loads the instruments of the DTO and makes sure all of them belong to the session's project.
        /// </summary>
        private async Task<List<ProjectInstrument>> LoadInstrumentsOfProject(SessionCreateDto dto)
        {
            var instrumentIds = dto.InstrumentIds.Distinct().ToList();
            var instruments = (await _instrumentRepository.GetAllAsync(i => instrumentIds.Contains(i.Id) && i.ProjectId == dto.ProjectId)).ToList();

            if (instruments.Count != instrumentIds.Count)
            {
                throw new BusinessRuleException("session.instrumentNotInProject", "All instruments of a session must belong to its project.");
            }

            return instruments;
        }

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
