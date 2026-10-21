using AutoMapper;
using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.ProjectModels;
using StudioManager.RepositoryLayer.IRepository;
using StudioManager.ServiceLayer.Exceptions;
using StudioManager.ServiceLayer.IService.IProjectServices;

namespace StudioManager.ServiceLayer.Service.ProjectServices
{
    /// <summary>
    /// Mix versions of a project. The DTOs are always built for the whole project, because whether a version is
    /// an extra revision depends on the earlier versions of its songs.
    /// </summary>
    public class MixVersionService : BaseService<MixVersion, MixVersionReadDto, MixVersionCreateDto>, IMixVersionService
    {
        private static readonly string[] VersionPaths = ["Songs", "Feedback.Song"];

        private readonly IRepository<Project> _projectRepository;
        private readonly IRepository<Song> _songRepository;

        public MixVersionService(IRepository<MixVersion> repository, IRepository<Project> projectRepository, IRepository<Song> songRepository, IMapper mapper)
            : base(repository, mapper)
        {
            _projectRepository = projectRepository;
            _songRepository = songRepository;
        }

        public async Task<IEnumerable<MixVersionReadDto>> GetForProject(int projectId)
        {
            return (await BuildDtos(projectId)).OrderByDescending(v => v.Number);
        }

        public async Task<IEnumerable<MixVersionReadDto>> GetForSong(int songId)
        {
            var song = await _songRepository.GetAsync(songId) ?? throw new EntityNotFoundException(nameof(Song), songId);
            var versions = await BuildDtos(song.ProjectId);
            return versions.Where(v => v.Songs.Any(s => s.Id == songId)).OrderByDescending(v => v.Number);
        }

        public override async Task<IEnumerable<MixVersionReadDto>> GetAll()
        {
            var projectIds = (await _repository.GetAllAsync()).Select(v => v.ProjectId).Distinct();
            var result = new List<MixVersionReadDto>();
            foreach (var projectId in projectIds)
            {
                result.AddRange(await BuildDtos(projectId));
            }
            return result;
        }

        public override async Task<MixVersionReadDto?> Get(int id)
        {
            var version = await _repository.GetAsync(id);
            return version == null ? null : (await BuildDtos(version.ProjectId)).Single(v => v.Id == id);
        }

        public override async Task<MixVersionReadDto> Insert(MixVersionCreateDto createDto)
        {
            if (!await _projectRepository.ExistsAsync(p => p.Id == createDto.ProjectId))
            {
                throw new EntityNotFoundException(nameof(Project), createDto.ProjectId);
            }

            var songs = await LoadSongs(createDto);
            Clean(createDto);

            var version = _mapper.Map<MixVersion>(createDto);
            var existing = await _repository.GetAllAsync(v => v.ProjectId == createDto.ProjectId);
            version.Number = existing.Select(v => v.Number).DefaultIfEmpty(0).Max() + 1;
            version.Name = createDto.Name ?? $"v{version.Number}";
            version.Songs = songs;

            await _repository.AddAsync(version);
            return (await Get(version.Id))!;
        }

        public override async Task<MixVersionReadDto> Update(int id, MixVersionCreateDto updateDto)
        {
            var version = (await _repository.GetAllWithPathsAsync(v => v.Id == id, "Songs")).SingleOrDefault()
                ?? throw new EntityNotFoundException(nameof(MixVersion), id);
            if (version.ProjectId != updateDto.ProjectId)
            {
                throw new BusinessRuleException("mixVersion.otherProject", "A mix version can not be moved to another project.");
            }

            var songs = await LoadSongs(updateDto);
            Clean(updateDto);

            _mapper.Map(updateDto, version);
            version.Name = updateDto.Name ?? version.Name;
            if (version.Status != MixVersionStatus.Approved)
            {
                version.ApprovedBy = null; // "approved by Simon" only as long as it is approved
            }
            // The song list is replaced (EF removes the rows of the songs that are not exported anymore)
            version.Songs.Clear();
            foreach (var song in songs)
            {
                version.Songs.Add(song);
            }

            await _repository.UpdateAsync(version);
            return (await Get(id))!;
        }

        /// <summary>
        /// All versions of the project as DTOs. Counts per song how often it was exported:
        /// its 2nd version is the 1st revision, and revisions beyond the included ones are "extra".
        /// </summary>
        private async Task<List<MixVersionReadDto>> BuildDtos(int projectId)
        {
            var project = await _projectRepository.GetAsync(projectId) ?? throw new EntityNotFoundException(nameof(Project), projectId);
            var songCount = (await _songRepository.GetAllAsync(s => s.ProjectId == projectId)).Count();
            var versions = (await _repository.GetAllWithPathsAsync(v => v.ProjectId == projectId, VersionPaths)).OrderBy(v => v.Number);

            var versionsPerSong = new Dictionary<int, int>();
            var result = new List<MixVersionReadDto>();

            foreach (var version in versions)
            {
                var dto = _mapper.Map<MixVersionReadDto>(version);
                dto.IsAllSongs = songCount > 0 && version.Songs.Count == songCount;

                foreach (var song in version.Songs.OrderBy(s => s.TrackNumber))
                {
                    versionsPerSong[song.Id] = versionsPerSong.GetValueOrDefault(song.Id) + 1;
                    var revision = versionsPerSong[song.Id] - 1;

                    if (project.IncludedRevisions != null && revision > project.IncludedRevisions)
                    {
                        dto.ExtraRevisionSongs.Add(_mapper.Map<SongShortDto>(song));
                    }
                }

                result.Add(dto);
            }

            return result;
        }

        /// <summary>The exported songs - they must all belong to the project.</summary>
        private async Task<List<Song>> LoadSongs(MixVersionCreateDto dto)
        {
            var songIds = dto.SongIds.Distinct().ToList();
            var songs = (await _songRepository.GetAllAsync(s => songIds.Contains(s.Id) && s.ProjectId == dto.ProjectId)).ToList();

            if (songs.Count == 0 || songs.Count != songIds.Count)
            {
                throw new BusinessRuleException("mixVersion.invalidSongs", "Choose at least one song of the project.");
            }

            return songs;
        }

        private static void Clean(MixVersionCreateDto dto)
        {
            dto.Name = string.IsNullOrWhiteSpace(dto.Name) ? null : dto.Name.Trim();
            dto.Link = string.IsNullOrWhiteSpace(dto.Link) ? null : dto.Link.Trim();
            dto.Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim();
        }
    }
}
