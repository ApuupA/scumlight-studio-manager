using StudioManager.DataLayer.Context;
using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.Models.GearModels;
using StudioManager.RepositoryLayer.Repository;
using StudioManager.ServiceLayer.Exceptions;
using StudioManager.ServiceLayer.Service.GearServices;

namespace StudioManager.Tests.Unit
{
    public class GearCategoryServiceTests
    {
        private readonly AppDbContext _context = TestHelper.CreateDbContext();
        private readonly GearCategoryService _service;

        public GearCategoryServiceTests()
        {
            _service = new GearCategoryService(
                new Repository<GearCategory>(_context),
                new Repository<GearItem>(_context),
                TestHelper.CreateMapper());
        }

        private async Task<GearCategory> AddCategory(string name, int sortOrder = 0)
        {
            var category = new GearCategory { Name = name, SortOrder = sortOrder };
            _context.GearCategories.Add(category);
            await _context.SaveChangesAsync();
            return category;
        }

        [Fact]
        public async Task GetAll_ShouldSortBySortOrderAndCountItems()
        {
            var mics = await AddCategory("Microphones", 20);
            await AddCategory("Preamps", 10);
            _context.GearItems.AddRange(
                new GearItem { Name = "SM57", GearCategoryId = mics.Id },
                new GearItem { Name = "U87", GearCategoryId = mics.Id });
            await _context.SaveChangesAsync();

            var result = (await _service.GetAll()).ToList();

            Assert.Equal(new[] { "Preamps", "Microphones" }, result.Select(c => c.Name));
            Assert.Equal(2, result.Single(c => c.Name == "Microphones").GearItemCount);
        }

        [Fact]
        public async Task Insert_ShouldRejectDuplicateName_IgnoringCase()
        {
            await AddCategory("Microphones");

            var exception = await Assert.ThrowsAsync<BusinessRuleException>(
                () => _service.Insert(new GearCategoryCreateDto { Name = "microphones" }));

            Assert.Equal("gearCategory.nameExists", exception.ErrorCode);
        }

        [Fact]
        public async Task Update_ShouldAllowKeepingTheOwnName()
        {
            var category = await AddCategory("Microphones");

            var result = await _service.Update(category.Id, new GearCategoryCreateDto { Name = "Microphones", Description = "Mics" });

            Assert.Equal("Mics", result.Description);
        }

        [Fact]
        public async Task Delete_ShouldFail_WhenCategoryContainsGear()
        {
            var category = await AddCategory("Microphones");
            _context.GearItems.Add(new GearItem { Name = "SM57", GearCategoryId = category.Id });
            await _context.SaveChangesAsync();

            var exception = await Assert.ThrowsAsync<BusinessRuleException>(() => _service.Delete(category.Id));

            Assert.Equal("gearCategory.hasItems", exception.ErrorCode);
        }

        [Fact]
        public async Task Delete_ShouldRemoveEmptyCategory()
        {
            var category = await AddCategory("Microphones");

            await _service.Delete(category.Id);

            Assert.Empty(_context.GearCategories);
        }

        [Fact]
        public async Task Delete_ShouldThrowNotFound_ForUnknownId()
        {
            await Assert.ThrowsAsync<EntityNotFoundException>(() => _service.Delete(999));
        }
    }
}
