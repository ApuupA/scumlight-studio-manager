using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.SettingsModels;

namespace StudioManager.ServiceLayer.IService.ISettingsServices
{
    /// <summary>
    /// The input channels of the studio (snake -> preamp -> DAW), configured once in the settings.
    /// </summary>
    public interface IStudioInputService : IBaseService<StudioInput, StudioInputReadDto, StudioInputCreateDto>
    {
        /// <summary>
        /// Creates several numbered inputs at once (appended after the existing ones).
        /// </summary>
        Task<IEnumerable<StudioInputReadDto>> BulkCreate(StudioInputBulkCreateDto dto);
    }
}
