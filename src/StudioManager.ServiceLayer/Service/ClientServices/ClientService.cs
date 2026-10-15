using AutoMapper;
using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.ClientModels;
using StudioManager.DataLayer.Models.ProjectModels;
using StudioManager.RepositoryLayer.IRepository;
using StudioManager.ServiceLayer.Exceptions;
using StudioManager.ServiceLayer.IService.IClientServices;
using System.Linq.Expressions;

namespace StudioManager.ServiceLayer.Service.ClientServices
{
    public class ClientService : BaseService<Client, ClientReadDto, ClientCreateDto>, IClientService
    {
        private readonly IRepository<Project> _projectRepository;

        public ClientService(IRepository<Client> repository, IRepository<Project> projectRepository, IMapper mapper)
            : base(repository, mapper)
        {
            _projectRepository = projectRepository;
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

            // The bands of the client are deleted by the database (cascade)
            await base.Delete(id);
        }
    }
}
