using Moq;
using StudioManager.DataLayer.Context;
using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.Models.ClientModels;
using StudioManager.DataLayer.Models.FinanceModels;
using StudioManager.DataLayer.Models.ProjectModels;
using StudioManager.RepositoryLayer.Repository;
using StudioManager.ServiceLayer.Exceptions;
using StudioManager.ServiceLayer.IService.IFileServices;
using StudioManager.ServiceLayer.Service.FinanceServices;

namespace StudioManager.Tests.Unit
{
    /// <summary>
    /// Tests for expenses, receipts and the yearly overview.
    /// </summary>
    public class ExpenseTests
    {
        private readonly AppDbContext _context = TestHelper.CreateDbContext();
        private readonly Mock<IFileStorageService> _mockFileStorage = new();
        private readonly ExpenseService _service;
        private readonly Project _project;

        public ExpenseTests()
        {
            _service = new ExpenseService(
                new Repository<Expense>(_context), new Repository<Payment>(_context), new Repository<BusinessSettings>(_context),
                _mockFileStorage.Object, TestHelper.CreateMapper());

            _project = new Project { Title = "Album", Client = new Client { Name = "Label GmbH" } };
            _context.Projects.Add(_project);
            _context.BusinessSettings.Add(new BusinessSettings { IsSmallBusiness = true });
            _context.SaveChanges();
        }

        private static ExpenseCreateDto NewDto(decimal gross = 119, ExpenseCategory category = ExpenseCategory.Material, DateOnly? date = null) => new()
        {
            Date = date ?? new DateOnly(2026, 3, 10),
            Description = "  Gitarrensaiten  ",
            Supplier = " ",
            Category = category,
            GrossAmount = gross,
            VatRate = 19
        };

        /// <summary>An issued invoice with one payment.</summary>
        private void AddPayment(DateOnly date, decimal amount, decimal vatAmount = 0, decimal grossTotal = 1000)
        {
            _context.Invoices.Add(new Invoice
            {
                Number = $"2026-{_context.Invoices.Count() + 1:000}",
                Status = InvoiceStatus.Issued,
                RecipientName = "Label GmbH",
                ProjectId = _project.Id,
                ClientId = _project.ClientId,
                GrossTotal = grossTotal,
                VatAmount = vatAmount,
                Payments = [new Payment { Date = date, Amount = amount }]
            });
            _context.SaveChanges();
        }

        [Fact]
        public async Task Insert_ShouldCleanTexts_AndCalculateNetAndVat()
        {
            var expense = await _service.Insert(NewDto());

            Assert.Equal("Gitarrensaiten", expense.Description);
            Assert.Null(expense.Supplier);
            Assert.Equal(100m, expense.NetAmount);
            Assert.Equal(19m, expense.VatAmount);
            Assert.False(expense.IsAboveDepreciationLimit);
        }

        [Fact]
        public async Task Equipment_AboveEightHundredNet_ShouldBeMarkedForDepreciation()
        {
            // 952 gross = 800 net -> still fine; 953 gross = 800.84 net -> above the limit
            Assert.False((await _service.Insert(NewDto(952, ExpenseCategory.Equipment))).IsAboveDepreciationLimit);
            Assert.True((await _service.Insert(NewDto(953, ExpenseCategory.Equipment))).IsAboveDepreciationLimit);
            // The limit is only for things you keep - not for e.g. rent
            Assert.False((await _service.Insert(NewDto(5000, ExpenseCategory.Rent))).IsAboveDepreciationLimit);
        }

        [Fact]
        public async Task SetReceipt_ShouldRejectOtherFileTypes()
        {
            var expense = await _service.Insert(NewDto());

            var exception = await Assert.ThrowsAsync<BusinessRuleException>(
                () => _service.SetReceipt(expense.Id, Stream.Null, "receipt.docx", 100));

            Assert.Equal("receipt.invalidType", exception.ErrorCode);
        }

        [Fact]
        public async Task SetReceipt_ShouldReplaceTheOldFile_AndDeleteRemovesIt()
        {
            var expense = await _service.Insert(NewDto());
            _mockFileStorage
                .SetupSequence(x => x.SaveAsync(It.IsAny<Stream>(), ExpenseService.ReceiptFolder, It.IsAny<string>()))
                .ReturnsAsync("receipts/first.jpg")
                .ReturnsAsync("receipts/second.pdf");

            await _service.SetReceipt(expense.Id, Stream.Null, "Foto.JPG", 100);
            var result = await _service.SetReceipt(expense.Id, Stream.Null, "Rechnung Thomann.pdf", 100);

            Assert.True(result.HasReceipt);
            Assert.Equal("Rechnung Thomann.pdf", result.ReceiptFileName);
            _mockFileStorage.Verify(x => x.Delete("receipts/first.jpg"), Times.Once);

            await _service.Delete(expense.Id);

            _mockFileStorage.Verify(x => x.Delete("receipts/second.pdf"), Times.Once);
            Assert.Empty(_context.Expenses);
        }

        [Fact]
        public async Task GetForYear_ShouldOnlyReturnTheExpensesOfThatYear()
        {
            await _service.Insert(NewDto(date: new DateOnly(2025, 12, 31)));
            await _service.Insert(NewDto(date: new DateOnly(2026, 1, 1)));

            var expenses = await _service.GetForYear(2026);

            Assert.Equal(new DateOnly(2026, 1, 1), Assert.Single(expenses).Date);
        }

        [Fact]
        public async Task YearOverview_ShouldSumPaymentsAndExpensesPerMonthAndCategory()
        {
            AddPayment(new DateOnly(2026, 3, 5), 600);
            AddPayment(new DateOnly(2026, 3, 20), 400);
            AddPayment(new DateOnly(2025, 12, 30), 999); // other year
            await _service.Insert(NewDto(119, ExpenseCategory.Material, new DateOnly(2026, 3, 1)));
            await _service.Insert(NewDto(50, ExpenseCategory.Material, new DateOnly(2026, 7, 1)));
            await _service.Insert(NewDto(300, ExpenseCategory.Software, new DateOnly(2026, 7, 2)));

            var overview = await _service.GetYearOverview(2026);

            Assert.True(overview.IsSmallBusiness);
            Assert.Equal(1000m, overview.IncomeTotal);
            Assert.Equal(469m, overview.ExpensesTotal);
            Assert.Equal(531m, overview.Result);
            Assert.Equal(12, overview.Months.Count);
            Assert.Equal(1000m, overview.Months[2].Income);
            Assert.Equal(119m, overview.Months[2].Expenses);
            Assert.Equal(350m, overview.Months[6].Expenses);
            Assert.Equal(ExpenseCategory.Software, overview.Categories[0].Category);
            Assert.Equal(169m, overview.Categories[1].Amount);
            Assert.Equal(5, overview.Bookings.Count);
            Assert.Equal("2026-001", overview.Bookings.First(b => b.IsIncome).InvoiceNumber);
        }

        [Fact]
        public async Task YearOverview_ShouldTakeTheVatShareOfAPartialPayment()
        {
            // Invoice 1190 gross with 190 VAT, half of it paid -> 95 VAT
            AddPayment(new DateOnly(2026, 5, 1), 595, vatAmount: 190, grossTotal: 1190);

            var overview = await _service.GetYearOverview(2026);

            Assert.Equal(95m, overview.IncomeVat);
            Assert.Equal(500m, Assert.Single(overview.Bookings).Net);
        }
    }
}
