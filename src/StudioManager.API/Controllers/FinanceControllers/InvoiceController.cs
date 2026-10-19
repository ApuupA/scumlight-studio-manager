using Microsoft.AspNetCore.Mvc;
using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.ServiceLayer.IService.IFinanceServices;

namespace StudioManager.API.Controllers.FinanceControllers
{
    /// <summary>
    /// Invoices: draft → issue → payments, corrections by cancellation.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class InvoiceController : ControllerBase
    {
        private readonly IInvoiceService _invoiceService;

        public InvoiceController(IInvoiceService invoiceService)
        {
            _invoiceService = invoiceService;
        }

        /// <summary>
        /// All invoices, newest first.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<InvoiceReadDto>>> GetAll()
        {
            return Ok(await _invoiceService.GetAll());
        }

        [HttpGet("project/{projectId:int}")]
        public async Task<ActionResult<IEnumerable<InvoiceReadDto>>> GetForProject(int projectId)
        {
            return Ok(await _invoiceService.GetForProject(projectId));
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<InvoiceReadDto>> Get(int id)
        {
            var invoice = await _invoiceService.Get(id);
            return invoice == null ? NotFound() : Ok(invoice);
        }

        /// <summary>
        /// Creates a draft with everything of the project that was not invoiced yet.
        /// </summary>
        [HttpPost("project/{projectId:int}/draft")]
        public async Task<ActionResult<InvoiceReadDto>> CreateDraft(int projectId)
        {
            return Ok(await _invoiceService.CreateDraft(projectId));
        }

        /// <summary>
        /// Changes a draft (recipient, dates, texts, lines).
        /// </summary>
        [HttpPut("{id:int}")]
        public async Task<ActionResult<InvoiceReadDto>> UpdateDraft(int id, [FromBody] InvoiceUpdateDto dto)
        {
            return Ok(await _invoiceService.UpdateDraft(id, dto));
        }

        /// <summary>
        /// Deletes a draft - issued invoices can not be deleted.
        /// </summary>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteDraft(int id)
        {
            await _invoiceService.DeleteDraft(id);
            return NoContent();
        }

        /// <summary>
        /// Issues a draft: number, frozen business data, locked.
        /// </summary>
        [HttpPost("{id:int}/issue")]
        public async Task<ActionResult<InvoiceReadDto>> Issue(int id, [FromBody] InvoiceDateDto dto)
        {
            return Ok(await _invoiceService.Issue(id, dto));
        }

        /// <summary>
        /// Cancels an issued invoice - returns the cancellation invoice.
        /// </summary>
        [HttpPost("{id:int}/cancel")]
        public async Task<ActionResult<InvoiceReadDto>> Cancel(int id, [FromBody] InvoiceDateDto dto)
        {
            return Ok(await _invoiceService.Cancel(id, dto));
        }

        [HttpPost("{id:int}/payments")]
        public async Task<ActionResult<InvoiceReadDto>> AddPayment(int id, [FromBody] PaymentCreateDto dto)
        {
            return Ok(await _invoiceService.AddPayment(id, dto));
        }

        [HttpDelete("{id:int}/payments/{paymentId:int}")]
        public async Task<ActionResult<InvoiceReadDto>> DeletePayment(int id, int paymentId)
        {
            return Ok(await _invoiceService.DeletePayment(id, paymentId));
        }
    }
}
