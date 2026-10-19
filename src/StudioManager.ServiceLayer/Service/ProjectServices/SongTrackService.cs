using AutoMapper;
using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.ProjectModels;
using StudioManager.RepositoryLayer.IRepository;
using StudioManager.ServiceLayer.Exceptions;
using StudioManager.ServiceLayer.IService.IProjectServices;

namespace StudioManager.ServiceLayer.Service.ProjectServices
{
    public class SongTrackService : BaseService<SongTrack, SongTrackReadDto, SongTrackCreateDto>, ISongTrackService
    {
        private readonly IRepository<Song> _songRepository;
        private readonly IRepository<ProjectInstrument> _instrumentRepository;

        public SongTrackService(
            IRepository<SongTrack> repository,
            IRepository<Song> songRepository,
            IRepository<ProjectInstrument> instrumentRepository,
            IMapper mapper) : base(repository, mapper)
        {
            _songRepository = songRepository;
            _instrumentRepository = instrumentRepository;
        }

        public async Task<IEnumerable<SongTrackReadDto>> GetForSong(int songId)
        {
            var tracks = await GetAllWhere(t => t.SongId == songId);
            return tracks.OrderBy(t => t.SortOrder).ThenBy(t => t.Id);
        }

        public override async Task<SongTrackReadDto> Insert(SongTrackCreateDto createDto)
        {
            await Validate(createDto);

            if (createDto.SortOrder == 0)
            {
                createDto.SortOrder = await NextSortOrder(createDto.SongId);
            }

            return await base.Insert(createDto);
        }

        public override async Task<SongTrackReadDto> Update(int id, SongTrackCreateDto updateDto)
        {
            await Validate(updateDto);
            return await base.Update(id, updateDto);
        }

        public async Task Move(int id, MoveDto dto)
        {
            var track = await _repository.GetAsync(id) ?? throw new EntityNotFoundException(nameof(SongTrack), id);

            var siblings = (await _repository.GetAllAsync(t => t.SongId == track.SongId))
                .OrderBy(t => t.SortOrder)
                .ThenBy(t => t.Id)
                .ToList();

            // Swap with the neighbour, then number the tracks 1, 2, 3, ... again
            var index = siblings.FindIndex(t => t.Id == id);
            var neighbourIndex = index + dto.Direction;
            if (neighbourIndex >= 0 && neighbourIndex < siblings.Count)
            {
                (siblings[index], siblings[neighbourIndex]) = (siblings[neighbourIndex], siblings[index]);
            }

            for (var i = 0; i < siblings.Count; i++)
            {
                siblings[i].SortOrder = i + 1;
            }

            await _repository.UpdateRangeAsync(siblings);
        }

        public async Task<IEnumerable<SongTrackReadDto>> AddFromInstruments(int songId)
        {
            var song = await GetSong(songId);
            var instruments = (await _instrumentRepository.GetAllAsync(i => i.ProjectId == song.ProjectId))
                .OrderBy(i => i.SortOrder)
                .ThenBy(i => i.Id);

            await AddTracks(songId, instruments.Select(i => i.Name));
            return await GetForSong(songId);
        }

        public async Task<IEnumerable<SongTrackReadDto>> CopyFromSong(SongTrackCopyDto dto)
        {
            var source = await GetSong(dto.FromSongId);
            var target = await GetSong(dto.ToSongId);

            if (source.ProjectId != target.ProjectId)
            {
                throw new BusinessRuleException("songTrack.otherProject", "Tracks can only be copied from a song of the same project.");
            }

            var sourceTracks = (await _repository.GetAllAsync(t => t.SongId == source.Id))
                .OrderBy(t => t.SortOrder)
                .ThenBy(t => t.Id);

            await AddTracks(target.Id, sourceTracks.Select(t => t.Name));
            return await GetForSong(target.Id);
        }

        /// <summary>
        /// Appends open tracks with the given names - names the song already has are skipped (ignoring case).
        /// </summary>
        private async Task AddTracks(int songId, IEnumerable<string> names)
        {
            var existingNames = (await _repository.GetAllAsync(t => t.SongId == songId))
                .Select(t => t.Name)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            var sortOrder = await NextSortOrder(songId);

            foreach (var name in names.Where(name => existingNames.Add(name)))
            {
                await _repository.AddAsync(new SongTrack { SongId = songId, Name = name, SortOrder = sortOrder++ });
            }
        }

        private async Task<int> NextSortOrder(int songId)
        {
            var siblings = await _repository.GetAllAsync(t => t.SongId == songId);
            return siblings.Select(t => t.SortOrder).DefaultIfEmpty(0).Max() + 1;
        }

        private async Task<Song> GetSong(int songId)
        {
            return await _songRepository.GetAsync(songId)
                ?? throw new BusinessRuleException("songTrack.songNotFound", $"Song with id {songId} does not exist.");
        }

        /// <summary>
        /// The song must exist. Trims the texts.
        /// </summary>
        private async Task Validate(SongTrackCreateDto dto)
        {
            await GetSong(dto.SongId);

            dto.Name = dto.Name.Trim();
            dto.Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim();
        }
    }
}
