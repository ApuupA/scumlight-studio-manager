using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;

namespace StudioManager.ServiceLayer.IService.IProjectServices
{
    /// <summary>
    /// Reusable instrument setups. Templates are created from an existing instrument and applied with
    /// IProjectInstrumentService.ApplyTemplate - so there is no "normal" create / update here.
    /// </summary>
    public interface ISetupTemplateService
    {
        Task<IEnumerable<SetupTemplateReadDto>> GetAll();

        /// <summary>
        /// Saves the setup of an instrument as template. Band gear is left out (it belongs to one project).
        /// </summary>
        Task<SetupTemplateReadDto> CreateFromInstrument(SetupTemplateFromInstrumentDto dto);

        Task Delete(int id);
    }
}
