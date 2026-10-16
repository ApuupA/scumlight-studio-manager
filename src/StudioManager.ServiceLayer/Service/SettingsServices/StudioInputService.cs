using AutoMapper;
using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.SettingsModels;
using StudioManager.RepositoryLayer.IRepository;
using StudioManager.ServiceLayer.IService.ISettingsServices;
using System.Linq.Expressions;

namespace StudioManager.ServiceLayer.Service.SettingsServices
{
    public class StudioInputService : BaseService<StudioInput, StudioInputReadDto, StudioInputCreateDto>, IStudioInputService
    {
        public StudioInputService(IRepository<StudioInput> repository, IMapper mapper) : base(repository, mapper)
        {
        }

        // Load the group, so name and color can be shown
        protected override Expression<Func<StudioInput, object>>[] DefaultIncludes => [i => i.InputGroup!];

        public override async Task<IEnumerable<StudioInputReadDto>> GetAll()
        {
            var inputs = await base.GetAll();
            return inputs.OrderBy(i => i.SortOrder).ThenBy(i => i.Id);
        }

        public override async Task<StudioInputReadDto> Insert(StudioInputCreateDto createDto)
        {
            if (createDto.SortOrder == 0)
            {
                createDto.SortOrder = await NextSortOrder();
            }

            return await base.Insert(createDto);
        }

        public async Task<IEnumerable<StudioInputReadDto>> BulkCreate(StudioInputBulkCreateDto dto)
        {
            var sortOrder = await NextSortOrder();
            var created = new List<StudioInput>();

            for (var i = 0; i < dto.Count; i++)
            {
                var input = new StudioInput
                {
                    SortOrder = sortOrder + i,
                    SnakeChannel = Numbered(dto.SnakePrefix, dto.SnakeStart, i),
                    Preamp = Numbered(dto.PreampPrefix, dto.PreampStart, i),
                    DawChannel = Numbered(dto.DawPrefix, dto.DawStart, i),
                    InputGroupId = dto.InputGroupId
                };
                await _repository.AddAsync(input);
                created.Add(input);
            }

            var createdIds = created.Select(i => i.Id).ToList();
            return (await GetAllWhere(i => createdIds.Contains(i.Id))).OrderBy(i => i.SortOrder);
        }

        /// <summary>
        /// "16CH - " + start 9 + index 2 -> "16CH - 11". Without a start number the prefix stays as it is ("UNISON").
        /// </summary>
        private static string? Numbered(string? prefix, int? start, int index)
        {
            if (start == null)
            {
                return string.IsNullOrWhiteSpace(prefix) ? null : prefix.Trim();
            }

            return $"{prefix}{start + index}";
        }

        private async Task<int> NextSortOrder()
        {
            var inputs = await _repository.GetAllAsync();
            return inputs.Select(i => i.SortOrder).DefaultIfEmpty(0).Max() + 1;
        }
    }
}
