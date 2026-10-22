using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudioManager.API.Infrastructure;
using Microsoft.Net.Http.Headers;
using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.FinanceModels;
using StudioManager.ServiceLayer.IService.IFinanceServices;
using StudioManager.ServiceLayer.Service.FinanceServices;

namespace StudioManager.API.Controllers.FinanceControllers
{
    /// <summary>
    /// Expenses with receipts and the yearly overview of income and expenses.
    /// </summary>
    [Authorize(Roles = Roles.Owner)] // finance: only the owner
    public class ExpenseController : BaseController<ExpenseReadDto, ExpenseCreateDto, Expense, IExpenseService>
    {
        private readonly IExpenseService _service;

        public ExpenseController(IExpenseService service) : base(service)
        {
            _service = service;
        }

        /// <summary>
        /// The expenses of one year, newest first.
        /// </summary>
        [HttpGet("year/{year:int}")]
        public async Task<ActionResult<IEnumerable<ExpenseReadDto>>> GetForYear(int year)
        {
            return Ok(await _service.GetForYear(year));
        }

        /// <summary>
        /// Income (payments received) and expenses of one year per month and category, with all bookings.
        /// </summary>
        [HttpGet("overview/{year:int}")]
        public async Task<ActionResult<YearOverviewDto>> GetYearOverview(int year)
        {
            return Ok(await _service.GetYearOverview(year));
        }

        /// <summary>
        /// The receipt file (404 if there is none). Shown in the browser; "Save" uses the original file name.
        /// </summary>
        [HttpGet("{id:int}/receipt")]
        public async Task<IActionResult> GetReceipt(int id)
        {
            var receipt = await _service.GetReceipt(id);
            if (receipt == null)
            {
                return NotFound();
            }

            // "inline" instead of "attachment", so a PDF opens in the browser tab.
            // SetHttpFileName also handles umlauts in the name (e.g. "Rechnung_Müller.pdf").
            var disposition = new ContentDispositionHeaderValue("inline");
            disposition.SetHttpFileName(receipt.FileName ?? "receipt");
            Response.Headers.ContentDisposition = disposition.ToString();
            return File(receipt.Content, receipt.ContentType);
        }

        /// <summary>
        /// Uploads the receipt (PDF, JPG, PNG or WEBP, max. 10 MB) and replaces the existing one.
        /// </summary>
        [HttpPost("{id:int}/receipt")]
        [RequestSizeLimit(ExpenseService.MaxReceiptSizeBytes + 1024 * 1024)]
        public async Task<ActionResult<ExpenseReadDto>> UploadReceipt(int id, IFormFile file)
        {
            await using var stream = file.OpenReadStream();
            return Ok(await _service.SetReceipt(id, stream, file.FileName, file.Length));
        }

        /// <summary>
        /// Removes the receipt.
        /// </summary>
        [HttpDelete("{id:int}/receipt")]
        public async Task<ActionResult<ExpenseReadDto>> DeleteReceipt(int id)
        {
            return Ok(await _service.DeleteReceipt(id));
        }
    }
}
