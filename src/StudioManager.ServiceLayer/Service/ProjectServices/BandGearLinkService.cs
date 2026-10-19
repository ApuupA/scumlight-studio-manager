using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.ProjectModels;
using StudioManager.RepositoryLayer.IRepository;
using StudioManager.ServiceLayer.Exceptions;
using StudioManager.ServiceLayer.IService.IProjectServices;
using System.Security.Cryptography;

namespace StudioManager.ServiceLayer.Service.ProjectServices
{
    public class BandGearLinkService : IBandGearLinkService
    {
        /// <summary>At most this many open suggestions per project - protects against a flood of entries.</summary>
        public const int MaxOpenSuggestions = 100;

        private readonly IRepository<Project> _projectRepository;
        private readonly IRepository<ExternalGear> _gearRepository;

        public BandGearLinkService(IRepository<Project> projectRepository, IRepository<ExternalGear> gearRepository)
        {
            _projectRepository = projectRepository;
            _gearRepository = gearRepository;
        }

        public async Task<BandGearLinkDto> GetLink(int projectId)
        {
            var project = await GetProject(projectId);
            return new BandGearLinkDto { Token = project.BandGearToken };
        }

        public async Task<BandGearLinkDto> CreateLink(int projectId)
        {
            var project = await GetProject(projectId);

            // 32 random bytes as URL-safe text - impossible to guess
            project.BandGearToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
                .Replace('+', '-').Replace('/', '_').TrimEnd('=');
            await _projectRepository.UpdateAsync(project);

            return new BandGearLinkDto { Token = project.BandGearToken };
        }

        public async Task DeleteLink(int projectId)
        {
            var project = await GetProject(projectId);
            project.BandGearToken = null;
            await _projectRepository.UpdateAsync(project);
        }

        public async Task<BandGearFormDto?> GetForm(string token)
        {
            var project = await FindByToken(token);
            return project == null ? null : await BuildForm(project);
        }

        public async Task<BandGearFormDto> AddSuggestion(string token, BandGearSuggestionDto dto)
        {
            var project = await FindByToken(token) ?? throw new EntityNotFoundException("BandGearLink", 0);

            var openSuggestions = await _gearRepository.GetAllAsync(g => g.ProjectId == project.Id && g.IsSuggestion);
            if (openSuggestions.Count() >= MaxOpenSuggestions)
            {
                throw new BusinessRuleException("bandGearLink.tooManySuggestions",
                    "There are too many open entries - please wait until the studio has checked them.");
            }

            await _gearRepository.AddAsync(new ExternalGear
            {
                ProjectId = project.Id,
                Name = dto.Name.Trim(),
                Quantity = dto.Quantity,
                Owner = string.IsNullOrWhiteSpace(dto.Owner) ? null : dto.Owner.Trim(),
                Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim(),
                IsSuggestion = true
            });

            return await BuildForm(project);
        }

        public async Task<BandGearFormDto> DeleteSuggestion(string token, int externalGearId)
        {
            var project = await FindByToken(token) ?? throw new EntityNotFoundException("BandGearLink", 0);

            // Only open suggestions of this project - accepted gear is managed by the studio
            var gear = await _gearRepository.GetAsync(g => g.Id == externalGearId && g.ProjectId == project.Id && g.IsSuggestion)
                ?? throw new EntityNotFoundException(nameof(ExternalGear), externalGearId);

            await _gearRepository.DeleteAsync(gear.Id);
            return await BuildForm(project);
        }

        private async Task<BandGearFormDto> BuildForm(Project project)
        {
            var gear = await _gearRepository.GetAllAsync(g => g.ProjectId == project.Id);

            return new BandGearFormDto
            {
                ProjectTitle = project.Title,
                BandNames = string.Join(", ", project.Bands.OrderBy(b => b.Name).Select(b => b.Name)),
                Entries = gear
                    .OrderBy(g => g.Owner)
                    .ThenBy(g => g.Name)
                    .Select(g => new BandGearEntryDto
                    {
                        Id = g.Id,
                        Name = g.Name,
                        Quantity = g.Quantity,
                        Owner = g.Owner,
                        Notes = g.Notes,
                        IsSuggestion = g.IsSuggestion
                    })
                    .ToList()
            };
        }

        private async Task<Project?> FindByToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return null;
            }

            return await _projectRepository.GetAsync(p => p.BandGearToken == token, p => p.Bands);
        }

        private async Task<Project> GetProject(int projectId)
        {
            return await _projectRepository.GetAsync(projectId) ?? throw new EntityNotFoundException(nameof(Project), projectId);
        }
    }
}
