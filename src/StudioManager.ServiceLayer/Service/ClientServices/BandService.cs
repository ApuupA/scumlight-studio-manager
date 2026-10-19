using AutoMapper;
using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.ClientModels;
using StudioManager.RepositoryLayer.IRepository;
using StudioManager.ServiceLayer.Exceptions;
using StudioManager.ServiceLayer.IService.IClientServices;
using StudioManager.ServiceLayer.IService.IFileServices;
using StudioManager.ServiceLayer.Results;
using System.Linq.Expressions;

namespace StudioManager.ServiceLayer.Service.ClientServices
{
    public class BandService : BaseService<Band, BandReadDto, BandCreateDto>, IBandService
    {
        private readonly IRepository<Client> _clientRepository;

        public const string PhotoFolder = "band-photos";

        private readonly IPhotoService _photos;

        public BandService(IRepository<Band> repository, IRepository<Client> clientRepository, IPhotoService photos, IMapper mapper)
            : base(repository, mapper)
        {
            _clientRepository = clientRepository;
            _photos = photos;
        }

        // Load the client, so ClientName can be filled
        protected override Expression<Func<Band, object>>[] DefaultIncludes => [b => b.Client!];

        public override async Task<IEnumerable<BandReadDto>> GetAll()
        {
            var bands = await base.GetAll();
            return bands.OrderBy(b => b.Name);
        }

        public async Task<IEnumerable<BandReadDto>> GetForClient(int clientId)
        {
            var bands = await GetAllWhere(b => b.ClientId == clientId);
            return bands.OrderBy(b => b.Name);
        }

        public override async Task<BandReadDto> Insert(BandCreateDto createDto)
        {
            await EnsureClientExists(createDto.ClientId);
            return await base.Insert(createDto);
        }

        public override async Task<BandReadDto> Update(int id, BandCreateDto updateDto)
        {
            await EnsureClientExists(updateDto.ClientId);
            return await base.Update(id, updateDto);
        }

        private async Task EnsureClientExists(int clientId)
        {
            if (!await _clientRepository.ExistsAsync(c => c.Id == clientId))
            {
                throw new BusinessRuleException("band.clientNotFound", $"Client with id {clientId} does not exist.");
            }
        }

        public override async Task Delete(int id)
        {
            var band = await _repository.GetAsync(id);
            await base.Delete(id);
            _photos.DeleteFile(band?.PhotoPath);
        }

        public async Task<BandReadDto> SetPhoto(int id, Stream content, string fileName, long length)
        {
            await _photos.SetPhoto(_repository, id, content, fileName, length, PhotoFolder);
            return (await Get(id))!;
        }

        public async Task<FileDownload?> GetPhoto(int id)
        {
            return await _photos.GetPhoto(_repository, id);
        }

        public async Task<BandReadDto> DeletePhoto(int id)
        {
            await _photos.DeletePhoto(_repository, id);
            return (await Get(id))!;
        }
    }
}
