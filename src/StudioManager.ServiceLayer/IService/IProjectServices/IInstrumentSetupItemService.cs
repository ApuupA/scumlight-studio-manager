using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.ProjectModels;

namespace StudioManager.ServiceLayer.IService.IProjectServices
{
    /// <summary>
    /// The entries of an instrument setup (studio gear, band gear or free text).
    /// </summary>
    public interface IInstrumentSetupItemService : IBaseService<InstrumentSetupItem, InstrumentSetupItemReadDto, InstrumentSetupItemCreateDto>
    {
        /// <summary>
        /// Moves the entry one position up or down in its chain / list.
        /// </summary>
        Task Move(int id, SetupItemMoveDto dto);

        /// <summary>
        /// In which projects and for which instruments a studio gear item is used.
        /// </summary>
        Task<IEnumerable<GearUsageDto>> GetUsagesOfGearItem(int gearItemId);
    }
}
