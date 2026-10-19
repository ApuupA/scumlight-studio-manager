using AutoMapper;
using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.ProjectModels;
using StudioManager.RepositoryLayer.IRepository;
using StudioManager.ServiceLayer.Exceptions;
using StudioManager.ServiceLayer.IService.IProjectServices;
using System.Linq.Expressions;

namespace StudioManager.ServiceLayer.Service.ProjectServices
{
    public class SongService : BaseService<Song, SongReadDto, SongCreateDto>, ISongService
    {
        private readonly IRepository<Project> _projectRepository;

        public SongService(IRepository<Song> repository, IRepository<Project> projectRepository, IMapper mapper)
            : base(repository, mapper)
        {
            _projectRepository = projectRepository;
        }

        // Load the band (for BandName) and the tempos
        protected override Expression<Func<Song, object>>[] DefaultIncludes => [s => s.Band!, s => s.Tempos];

        public async Task<IEnumerable<SongReadDto>> GetForProject(int projectId)
        {
            var songs = await GetAllWhere(s => s.ProjectId == projectId);
            return songs.OrderBy(s => s.TrackNumber).ThenBy(s => s.Title);
        }

        public async Task<IEnumerable<SongReadDto>> SetStatus(SongStatusUpdateDto dto)
        {
            var songIds = dto.SongIds.Distinct().ToList();
            var songs = (await _repository.GetAllAsync(s => songIds.Contains(s.Id))).ToList();

            foreach (var song in songs)
            {
                song.Status = dto.Status;
            }

            await _repository.UpdateRangeAsync(songs);

            return (await GetAllWhere(s => songIds.Contains(s.Id))).OrderBy(s => s.TrackNumber);
        }

        public override async Task<SongReadDto> Insert(SongCreateDto createDto)
        {
            await EnsureBandIsInProject(createDto);

            var song = _mapper.Map<Song>(createDto);
            ReplaceTempos(song, createDto);
            await _repository.AddAsync(song);

            return (await Get(song.Id))!;
        }

        public override async Task<SongReadDto> Update(int id, SongCreateDto updateDto)
        {
            await EnsureBandIsInProject(updateDto);

            var song = await _repository.GetAsync(s => s.Id == id, s => s.Tempos)
                ?? throw new EntityNotFoundException(nameof(Song), id);

            _mapper.Map(updateDto, song);
            ReplaceTempos(song, updateDto);
            await _repository.UpdateAsync(song);

            return (await Get(id))!;
        }

        /// <summary>
        /// Replaces the tempos of the song with the ones from the form (EF deletes the removed ones).
        /// </summary>
        private static void ReplaceTempos(Song song, SongCreateDto dto)
        {
            song.Tempos.Clear();

            for (var i = 0; i < dto.Tempos.Count; i++)
            {
                song.Tempos.Add(new SongTempo
                {
                    Bpm = dto.Tempos[i].Bpm,
                    FromBar = dto.Tempos[i].FromBar,
                    SortOrder = i + 1
                });
            }
        }

        /// <summary>
        /// The project must exist and the song's band (if set) must be one of the project's bands.
        /// </summary>
        private async Task EnsureBandIsInProject(SongCreateDto dto)
        {
            var project = await _projectRepository.GetAsync(p => p.Id == dto.ProjectId, p => p.Bands)
                ?? throw new BusinessRuleException("song.projectNotFound", $"Project with id {dto.ProjectId} does not exist.");

            if (dto.BandId != null && project.Bands.All(b => b.Id != dto.BandId))
            {
                throw new BusinessRuleException("song.bandNotInProject", "The band does not take part in this project.");
            }
        }
    }
}
