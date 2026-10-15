using AutoMapper;
using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.GearModels;
using StudioManager.RepositoryLayer.IRepository;
using StudioManager.ServiceLayer.Exceptions;
using StudioManager.ServiceLayer.IService.IGearServices;
using System.Linq.Expressions;

namespace StudioManager.ServiceLayer.Service.GearServices
{
    public class GearCategoryService : BaseService<GearCategory, GearCategoryReadDto, GearCategoryCreateDto>, IGearCategoryService
    {
        private readonly IRepository<GearItem> _gearItemRepository;

        public GearCategoryService(IRepository<GearCategory> repository, IRepository<GearItem> gearItemRepository, IMapper mapper)
            : base(repository, mapper)
        {
            _gearItemRepository = gearItemRepository;
        }

        // Load the items as well, so GearItemCount can be filled
        protected override Expression<Func<GearCategory, object>>[] DefaultIncludes => [c => c.GearItems];

        public override async Task<IEnumerable<GearCategoryReadDto>> GetAll()
        {
            var categories = await base.GetAll();
            return categories.OrderBy(c => c.SortOrder).ThenBy(c => c.Name);
        }

        public override async Task<GearCategoryReadDto> Insert(GearCategoryCreateDto createDto)
        {
            await EnsureNameIsUnique(createDto.Name, currentId: null);
            return await base.Insert(createDto);
        }

        public override async Task<GearCategoryReadDto> Update(int id, GearCategoryCreateDto updateDto)
        {
            await EnsureNameIsUnique(updateDto.Name, currentId: id);
            return await base.Update(id, updateDto);
        }

        public override async Task Delete(int id)
        {
            if (await _gearItemRepository.ExistsAsync(g => g.GearCategoryId == id))
            {
                throw new BusinessRuleException("gearCategory.hasItems",
                    "The category still contains gear. Move or delete the gear first.");
            }

            await base.Delete(id);
        }

        private async Task EnsureNameIsUnique(string name, int? currentId)
        {
            var normalizedName = name.Trim().ToLower();

            if (await _repository.ExistsAsync(c => c.Name.ToLower() == normalizedName && c.Id != currentId))
            {
                throw new BusinessRuleException("gearCategory.nameExists", $"A category named '{name}' already exists.");
            }
        }
    }
}
