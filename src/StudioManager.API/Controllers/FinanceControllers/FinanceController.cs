using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudioManager.API.Infrastructure;
using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.ServiceLayer.IService.IFinanceServices;

namespace StudioManager.API.Controllers.FinanceControllers
{
    /// <summary>
    /// Billing settings of the studio and the money view of a project.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize(Roles = Roles.Owner)] // finance: only the owner
    public class FinanceController : ControllerBase
    {
        private readonly IFinanceService _financeService;

        public FinanceController(IFinanceService financeService)
        {
            _financeService = financeService;
        }

        /// <summary>
        /// Billing settings (default hourly rate, currency).
        /// </summary>
        [HttpGet("settings")]
        public async Task<ActionResult<BusinessSettingsReadDto>> GetSettings()
        {
            return Ok(await _financeService.GetSettings());
        }

        [HttpPut("settings")]
        public async Task<ActionResult<BusinessSettingsReadDto>> UpdateSettings([FromBody] BusinessSettingsUpdateDto dto)
        {
            return Ok(await _financeService.UpdateSettings(dto));
        }

        /// <summary>
        /// Pricing, hours and value of a project.
        /// </summary>
        [HttpGet("project/{projectId:int}")]
        public async Task<ActionResult<ProjectFinanceDto>> GetProjectFinance(int projectId)
        {
            return Ok(await _financeService.GetProjectFinance(projectId));
        }

        /// <summary>
        /// Sets how a project is charged: by the hour or with a flat price.
        /// </summary>
        [HttpPut("project/{projectId:int}/pricing")]
        public async Task<ActionResult<ProjectFinanceDto>> SetPricing(int projectId, [FromBody] ProjectPricingDto dto)
        {
            return Ok(await _financeService.SetPricing(projectId, dto));
        }
    }
}
