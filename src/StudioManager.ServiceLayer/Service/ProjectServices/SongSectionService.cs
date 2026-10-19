using AutoMapper;
using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.ProjectModels;
using StudioManager.RepositoryLayer.IRepository;
using StudioManager.ServiceLayer.Exceptions;
using StudioManager.ServiceLayer.IService.IProjectServices;

namespace StudioManager.ServiceLayer.Service.ProjectServices
{
    public class SongSectionService : BaseService<SongSection, SongSectionReadDto, SongSectionCreateDto>, ISongSectionService
    {
        private readonly IRepository<Song> _songRepository;

        public SongSectionService(IRepository<SongSection> repository, IRepository<Song> songRepository, IMapper mapper)
            : base(repository, mapper)
        {
            _songRepository = songRepository;
        }

        public async Task<IEnumerable<SongSectionReadDto>> GetForSong(int songId)
        {
            var sections = await GetAllWhere(s => s.SongId == songId);
            return sections.OrderBy(s => s.SortOrder).ThenBy(s => s.Id);
        }

        public override async Task<SongSectionReadDto> Insert(SongSectionCreateDto createDto)
        {
            await Validate(createDto);

            if (createDto.SortOrder == 0)
            {
                var siblings = await _repository.GetAllAsync(s => s.SongId == createDto.SongId);
                createDto.SortOrder = siblings.Select(s => s.SortOrder).DefaultIfEmpty(0).Max() + 1;
            }

            return await base.Insert(createDto);
        }

        public override async Task<SongSectionReadDto> Update(int id, SongSectionCreateDto updateDto)
        {
            await Validate(updateDto);
            return await base.Update(id, updateDto);
        }

        public async Task Move(int id, MoveDto dto)
        {
            var section = await _repository.GetAsync(id) ?? throw new EntityNotFoundException(nameof(SongSection), id);

            var siblings = (await _repository.GetAllAsync(s => s.SongId == section.SongId))
                .OrderBy(s => s.SortOrder)
                .ThenBy(s => s.Id)
                .ToList();

            // Swap with the neighbour, then number the parts 1, 2, 3, ... again
            var index = siblings.FindIndex(s => s.Id == id);
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

        /// <summary>
        /// The song must exist and the bars must be in the right order. Trims the texts.
        /// </summary>
        private async Task Validate(SongSectionCreateDto dto)
        {
            if (!await _songRepository.ExistsAsync(s => s.Id == dto.SongId))
            {
                throw new BusinessRuleException("songSection.songNotFound", $"Song with id {dto.SongId} does not exist.");
            }

            if (dto.FromBar != null && dto.ToBar != null && dto.ToBar < dto.FromBar)
            {
                throw new BusinessRuleException("songSection.barsInvalid", "The last bar must not be before the first bar.");
            }

            dto.Name = dto.Name.Trim();
            dto.Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim();
        }
    }
}
