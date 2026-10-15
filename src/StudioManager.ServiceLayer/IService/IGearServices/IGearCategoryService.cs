using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.GearModels;

namespace StudioManager.ServiceLayer.IService.IGearServices
{
    public interface IGearCategoryService : IBaseService<GearCategory, GearCategoryReadDto, GearCategoryCreateDto>
    {
    }
}
