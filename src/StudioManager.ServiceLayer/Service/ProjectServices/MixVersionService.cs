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
    public class MixVersionService : BaseService<MixVersion, MixVersionReadDto, MixVersionCreateDto>, IMixVersionService
    {
        private readonly IRepository<Song> _songRepository;

        public MixVersionService(IRepository<MixVersion> repository, IRepository<Song> songRepository, IMapper mapper) : base(repository, mapper)
        {
            _songRepository = songRepository;
        }

        // The feedback, and the project for its included revisions (IsExtraRevision)
        protected override Expression<Func<MixVersion, object>>[] DefaultIncludes => [v => v.Feedback, v => v.Song!.Project!];

        public async Task<IEnumerable<MixVersionReadDto>> GetForSong(int songId)
        {
            var versions = await GetAllWhere(v => v.SongId == songId);
            return versions.OrderByDescending(v => v.Number);
        }

        public override async Task<MixVersionReadDto> Insert(MixVersionCreateDto createDto)
        {
            if (!await _songRepository.ExistsAsync(s => s.Id == createDto.SongId))
            {
                throw new EntityNotFoundException(nameof(Song), createDto.SongId);
            }

            Clean(createDto);
            var version = _mapper.Map<MixVersion>(createDto);

            // The next number of this song: v1, v2, ...
            var existing = await _repository.GetAllAsync(v => v.SongId == createDto.SongId);
            version.Number = existing.Select(v => v.Number).DefaultIfEmpty(0).Max() + 1;

            await _repository.AddAsync(version);
            return (await Get(version.Id))!;
        }

        public override async Task<MixVersionReadDto> Update(int id, MixVersionCreateDto updateDto)
        {
            var version = await _repository.GetAsync(id) ?? throw new EntityNotFoundException(nameof(MixVersion), id);
            if (version.SongId != updateDto.SongId)
            {
                throw new BusinessRuleException("mixVersion.otherSong", "A mix version can not be moved to another song.");
            }

            Clean(updateDto);
            return await base.Update(id, updateDto);
        }

        private static void Clean(MixVersionCreateDto dto)
        {
            dto.Link = string.IsNullOrWhiteSpace(dto.Link) ? null : dto.Link.Trim();
            dto.Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim();
        }
    }
}
