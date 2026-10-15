using Microsoft.AspNetCore.Mvc;
using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.ClientModels;
using StudioManager.ServiceLayer.IService.IClientServices;

namespace StudioManager.API.Controllers.ClientControllers
{
    public class BandController : BaseController<BandReadDto, BandCreateDto, Band, IBandService>
    {
        private readonly IBandService _service;

        public BandController(IBandService service) : base(service)
        {
            _service = service;
        }

        /// <summary>
        /// All entries of one client.
        /// </summary>
        [HttpGet("client/{clientId:int}")]
        public async Task<ActionResult<IEnumerable<BandReadDto>>> GetForClient(int clientId)
        {
            return Ok(await _service.GetForClient(clientId));
        }
    }
}
