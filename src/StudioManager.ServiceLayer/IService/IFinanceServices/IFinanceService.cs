using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;

namespace StudioManager.ServiceLayer.IService.IFinanceServices
{
    /// <summary>
    /// Billing settings of the studio and the money view of a project (pricing, hours, value).
    /// </summary>
    public interface IFinanceService
    {
        /// <summary>The billing settings (created with defaults on first use).</summary>
        Task<BusinessSettingsReadDto> GetSettings();

        Task<BusinessSettingsReadDto> UpdateSettings(BusinessSettingsUpdateDto dto);

        /// <summary>Pricing, hours and value of a project.</summary>
        Task<ProjectFinanceDto> GetProjectFinance(int projectId);

        /// <summary>Sets how the project is charged (hourly or flat price).</summary>
        Task<ProjectFinanceDto> SetPricing(int projectId, ProjectPricingDto dto);
    }
}
