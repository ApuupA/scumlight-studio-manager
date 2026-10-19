using AutoMapper;
using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.ClientModels;
using StudioManager.DataLayer.Models.ProjectModels;
using StudioManager.RepositoryLayer.IRepository;
using StudioManager.ServiceLayer.Exceptions;
using StudioManager.ServiceLayer.IService.IClientServices;
using StudioManager.ServiceLayer.IService.IFileServices;
using StudioManager.ServiceLayer.Results;
using System.Linq.Expressions;

namespace StudioManager.ServiceLayer.Service.ClientServices
{
    public class ClientService : BaseService<Client, ClientReadDto, ClientCreateDto>, IClientService
    {
        private readonly IRepository<Project> _projectRepository;

        public const string PhotoFolder = "client-photos";

        private readonly IPhotoService _photos;

        public ClientService(IRepository<Client> repository, IRepository<Project> projectRepository, IPhotoService photos, IMapper mapper)
            : base(repository, mapper)
        {
            _projectRepository = projectRepository;
            _photos = photos;
        }

        // Load bands and projects, so BandCount and ProjectCount can be filled
        protected override Expression<Func<Client, object>>[] DefaultIncludes => [c => c.Bands, c => c.Projects];

        public override async Task<IEnumerable<ClientReadDto>> GetAll()
        {
            var clients = await base.GetAll();
            return clients.OrderBy(c => c.Name);
        }

        public override async Task Delete(int id)
        {
            if (await _projectRepository.ExistsAsync(p => p.ClientId == id))
            {
                throw new BusinessRuleException("client.hasProjects",
                    "The client still has projects. Delete the projects first.");
            }

            // The bands of the client are deleted by the database (cascade) - their pictures here
            var client = await _repository.GetAsync(c => c.Id == id, c => c.Bands);
            await base.Delete(id);

            _photos.DeleteFile(client?.PhotoPath);
            foreach (var band in client?.Bands ?? [])
            {
                _photos.DeleteFile(band.PhotoPath);
            }
        }

        public async Task<ClientReadDto> SetPhoto(int id, Stream content, string fileName, long length)
        {
            await _photos.SetPhoto(_repository, id, content, fileName, length, PhotoFolder);
            return (await Get(id))!;
        }

        public async Task<FileDownload?> GetPhoto(int id)
        {
            return await _photos.GetPhoto(_repository, id);
        }

        public async Task<ClientReadDto> DeletePhoto(int id)
        {
            await _photos.DeletePhoto(_repository, id);
            return (await Get(id))!;
        }
    }
}
