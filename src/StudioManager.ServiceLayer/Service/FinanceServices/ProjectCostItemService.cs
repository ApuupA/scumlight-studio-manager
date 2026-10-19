using AutoMapper;
using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.FinanceModels;
using StudioManager.RepositoryLayer.IRepository;
using StudioManager.ServiceLayer.IService.IFinanceServices;
using StudioManager.ServiceLayer.Exceptions;
using System.Linq.Expressions;

namespace StudioManager.ServiceLayer.Service.FinanceServices
{
    public class ProjectCostItemService : BaseService<ProjectCostItem, ProjectCostItemReadDto, ProjectCostItemCreateDto>, IProjectCostItemService
    {
        public ProjectCostItemService(IRepository<ProjectCostItem> repository, IMapper mapper) : base(repository, mapper)
        {
        }

        public async Task<IEnumerable<ProjectCostItemReadDto>> GetForProject(int projectId)
        {
            var items = await GetAllWhere(c => c.ProjectId == projectId);
            return items.OrderByDescending(c => c.Date).ThenByDescending(c => c.Id);
        }

        public override async Task<ProjectCostItemReadDto> Insert(ProjectCostItemCreateDto createDto)
        {
            createDto.Description = createDto.Description.Trim();
            return await base.Insert(createDto);
        }

        public override async Task<ProjectCostItemReadDto> Update(int id, ProjectCostItemCreateDto updateDto)
        {
            await EnsureNotInvoiced(id);
            updateDto.Description = updateDto.Description.Trim();
            return await base.Update(id, updateDto);
        }

        public override async Task Delete(int id)
        {
            await EnsureNotInvoiced(id);
            await base.Delete(id);
        }

        // Load the invoice, so its number can be shown
        protected override Expression<Func<ProjectCostItem, object>>[] DefaultIncludes => [c => c.Invoice!];

        /// <summary>
        /// An item on an invoice (or draft) is locked - the invoice would not match anymore.
        /// </summary>
        private async Task EnsureNotInvoiced(int id)
        {
            if (await _repository.ExistsAsync(c => c.Id == id && c.InvoiceId != null))
            {
                throw new BusinessRuleException("costItem.invoiced",
                    "This item is on an invoice - delete the draft or cancel the invoice first.");
            }
        }
    }
}
