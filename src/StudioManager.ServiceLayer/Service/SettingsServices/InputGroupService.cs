using AutoMapper;
using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.SettingsModels;
using StudioManager.RepositoryLayer.IRepository;
using StudioManager.ServiceLayer.IService.ISettingsServices;
using System.Linq.Expressions;

namespace StudioManager.ServiceLayer.Service.SettingsServices
{
    public class InputGroupService : BaseService<InputGroup, InputGroupReadDto, InputGroupCreateDto>, IInputGroupService
    {
        public InputGroupService(IRepository<InputGroup> repository, IMapper mapper) : base(repository, mapper)
        {
        }

        // Load the inputs, so InputCount can be filled
        protected override Expression<Func<InputGroup, object>>[] DefaultIncludes => [g => g.Inputs];

        public override async Task<IEnumerable<InputGroupReadDto>> GetAll()
        {
            var groups = await base.GetAll();
            return groups.OrderBy(g => g.SortOrder).ThenBy(g => g.Name);
        }

        public override async Task<InputGroupReadDto> Insert(InputGroupCreateDto createDto)
        {
            if (createDto.SortOrder == 0)
            {
                var groups = await _repository.GetAllAsync();
                createDto.SortOrder = groups.Select(g => g.SortOrder).DefaultIfEmpty(0).Max() + 1;
            }

            return await base.Insert(createDto);
        }
    }
}
