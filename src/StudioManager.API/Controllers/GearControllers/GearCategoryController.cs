using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.GearModels;
using StudioManager.ServiceLayer.IService.IGearServices;

namespace StudioManager.API.Controllers.GearControllers
{
    public class GearCategoryController : BaseController<GearCategoryReadDto, GearCategoryCreateDto, GearCategory, IGearCategoryService>
    {
        public GearCategoryController(IGearCategoryService service) : base(service)
        {
        }
    }
}
