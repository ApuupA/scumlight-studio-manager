using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.ServiceLayer.IService.IFinanceServices;

namespace StudioManager.API.Controllers.FinanceControllers
{
    /// <summary>
    /// Data safety: state of the last backup and the finance archives (one ZIP per year).
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class DataSafetyController : ControllerBase
    {
        private readonly IFinanceArchiveService _archiveService;

        public DataSafetyController(IFinanceArchiveService archiveService)
        {
            _archiveService = archiveService;
        }

        /// <summary>
        /// When the last backup ran (and if it was copied off the server) and which finance archives exist.
        /// </summary>
        [HttpGet("status")]
        public async Task<ActionResult<DataSafetyStatusDto>> GetStatus()
        {
            return Ok(await _archiveService.GetStatus());
        }

        /// <summary>
        /// Creates the finance archive of a year fresh and downloads it (ZIP: CSV lists, invoices as HTML, receipts).
        /// </summary>
        [HttpGet("archive/{year:int}")]
        [Produces("application/zip")]
        public async Task<IActionResult> DownloadArchive(int year)
        {
            var archive = await _archiveService.CreateArchive(year);

            var disposition = new ContentDispositionHeaderValue("attachment");
            disposition.SetHttpFileName(archive.FileName ?? $"finanzen-{year}.zip");
            Response.Headers.ContentDisposition = disposition.ToString();
            return File(archive.Content, archive.ContentType);
        }
    }
}
