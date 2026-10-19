using Microsoft.AspNetCore.Mvc;
using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.ServiceLayer.IService.IFinanceServices;

namespace StudioManager.API.Controllers.FinanceControllers
{
    /// <summary>
    /// Quotes: draft → issue → accepted / declined → invoice (POST /api/Invoice/from-quote/{id}).
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class QuoteController : ControllerBase
    {
        private readonly IQuoteService _quoteService;

        public QuoteController(IQuoteService quoteService)
        {
            _quoteService = quoteService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<QuoteReadDto>>> GetAll()
        {
            return Ok(await _quoteService.GetAll());
        }

        [HttpGet("project/{projectId:int}")]
        public async Task<ActionResult<IEnumerable<QuoteReadDto>>> GetForProject(int projectId)
        {
            return Ok(await _quoteService.GetForProject(projectId));
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<QuoteReadDto>> Get(int id)
        {
            var quote = await _quoteService.Get(id);
            return quote == null ? NotFound() : Ok(quote);
        }

        /// <summary>
        /// Creates a draft for the project.
        /// </summary>
        [HttpPost("project/{projectId:int}/draft")]
        public async Task<ActionResult<QuoteReadDto>> CreateDraft(int projectId)
        {
            return Ok(await _quoteService.CreateDraft(projectId));
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<QuoteReadDto>> UpdateDraft(int id, [FromBody] QuoteUpdateDto dto)
        {
            return Ok(await _quoteService.UpdateDraft(id, dto));
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteDraft(int id)
        {
            await _quoteService.DeleteDraft(id);
            return NoContent();
        }

        /// <summary>
        /// Issues the draft: number, valid until, frozen - status Sent.
        /// </summary>
        [HttpPost("{id:int}/issue")]
        public async Task<ActionResult<QuoteReadDto>> Issue(int id, [FromBody] InvoiceDateDto dto)
        {
            return Ok(await _quoteService.Issue(id, dto));
        }

        /// <summary>
        /// The client's answer: Accepted or Declined (or back to Sent).
        /// </summary>
        [HttpPut("{id:int}/status")]
        public async Task<ActionResult<QuoteReadDto>> SetStatus(int id, [FromBody] QuoteStatusDto dto)
        {
            return Ok(await _quoteService.SetStatus(id, dto));
        }
    }
}
