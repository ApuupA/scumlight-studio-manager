using AutoMapper;
using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.FinanceModels;
using StudioManager.RepositoryLayer.IRepository;
using StudioManager.ServiceLayer.Exceptions;
using StudioManager.ServiceLayer.IService.IFileServices;
using StudioManager.ServiceLayer.IService.IFinanceServices;
using StudioManager.ServiceLayer.Results;
using System.Linq.Expressions;

namespace StudioManager.ServiceLayer.Service.FinanceServices
{
    public class ExpenseService : BaseService<Expense, ExpenseReadDto, ExpenseCreateDto>, IExpenseService
    {
        public const string ReceiptFolder = "receipts";
        public const long MaxReceiptSizeBytes = 10 * 1024 * 1024;

        /// <summary>
        /// Equipment and software above this net amount (German "GWG" limit) may have to be depreciated over several years.
        /// </summary>
        public const decimal DepreciationLimitNet = 800m;

        /// <summary>
        /// Allowed file extensions of a receipt and the content type it is delivered with.
        /// </summary>
        private static readonly Dictionary<string, string> ReceiptContentTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            [".pdf"] = "application/pdf",
            [".jpg"] = "image/jpeg",
            [".jpeg"] = "image/jpeg",
            [".png"] = "image/png",
            [".webp"] = "image/webp"
        };

        private readonly IRepository<Payment> _paymentRepository;
        private readonly IRepository<BusinessSettings> _settingsRepository;
        private readonly IFileStorageService _fileStorage;

        public ExpenseService(
            IRepository<Expense> repository,
            IRepository<Payment> paymentRepository,
            IRepository<BusinessSettings> settingsRepository,
            IFileStorageService fileStorage,
            IMapper mapper) : base(repository, mapper)
        {
            _paymentRepository = paymentRepository;
            _settingsRepository = settingsRepository;
            _fileStorage = fileStorage;
        }

        // Load the project, so its title can be shown
        protected override Expression<Func<Expense, object>>[] DefaultIncludes => [e => e.Project!];

        /// <summary>The gross amount without VAT, e.g. 119 EUR at 19 % = 100 EUR.</summary>
        public static decimal NetOf(decimal grossAmount, decimal vatRate) =>
            Math.Round(grossAmount / (1 + vatRate / 100m), 2);

        public static bool IsAboveDepreciationLimit(Expense expense) =>
            expense.Category is ExpenseCategory.Equipment or ExpenseCategory.Software
            && NetOf(expense.GrossAmount, expense.VatRate) > DepreciationLimitNet;

        public async Task<IEnumerable<ExpenseReadDto>> GetForYear(int year)
        {
            var (from, to) = YearRange(year);
            var expenses = await GetAllWhere(e => e.Date >= from && e.Date <= to);
            return expenses.OrderByDescending(e => e.Date).ThenByDescending(e => e.Id);
        }

        public override async Task<ExpenseReadDto> Insert(ExpenseCreateDto createDto)
        {
            Clean(createDto);
            return await base.Insert(createDto);
        }

        public override async Task<ExpenseReadDto> Update(int id, ExpenseCreateDto updateDto)
        {
            Clean(updateDto);
            return await base.Update(id, updateDto);
        }

        public override async Task Delete(int id)
        {
            var expense = await LoadExpense(id);
            await base.Delete(id);

            // The receipt belongs to the expense - delete it after the expense is gone
            if (expense.ReceiptPath != null)
            {
                _fileStorage.Delete(expense.ReceiptPath);
            }
        }

        // ---------- Receipt ----------

        public async Task<ExpenseReadDto> SetReceipt(int id, Stream content, string fileName, long length)
        {
            var expense = await LoadExpense(id);
            var extension = Path.GetExtension(fileName).ToLowerInvariant();

            if (!ReceiptContentTypes.ContainsKey(extension))
            {
                throw new BusinessRuleException("receipt.invalidType", "Only PDF, JPG, PNG and WEBP files are allowed.");
            }

            if (length > MaxReceiptSizeBytes)
            {
                throw new BusinessRuleException("receipt.tooLarge", "The receipt must not be larger than 10 MB.");
            }

            var oldPath = expense.ReceiptPath;

            expense.ReceiptPath = await _fileStorage.SaveAsync(content, ReceiptFolder, extension);
            expense.ReceiptFileName = Path.GetFileName(fileName);
            await _repository.UpdateAsync(expense);

            // Delete the old file only after the new one is saved successfully
            if (oldPath != null)
            {
                _fileStorage.Delete(oldPath);
            }

            return (await Get(id))!;
        }

        public async Task<FileDownload?> GetReceipt(int id)
        {
            var expense = await LoadExpense(id);
            if (expense.ReceiptPath == null)
            {
                return null;
            }

            var stream = _fileStorage.OpenRead(expense.ReceiptPath);
            if (stream == null)
            {
                return null;
            }

            var contentType = ReceiptContentTypes.GetValueOrDefault(Path.GetExtension(expense.ReceiptPath), "application/octet-stream");
            return new FileDownload(stream, contentType, expense.ReceiptFileName);
        }

        public async Task<ExpenseReadDto> DeleteReceipt(int id)
        {
            var expense = await LoadExpense(id);
            if (expense.ReceiptPath != null)
            {
                _fileStorage.Delete(expense.ReceiptPath);
                expense.ReceiptPath = null;
                expense.ReceiptFileName = null;
                await _repository.UpdateAsync(expense);
            }

            return (await Get(id))!;
        }

        // ---------- Yearly overview ----------

        public async Task<YearOverviewDto> GetYearOverview(int year)
        {
            var (from, to) = YearRange(year);
            var settings = await FinanceService.LoadSettings(_settingsRepository);

            // Income = the payments received in the year (not the invoices written) - that is what counts for the EÜR
            var payments = await _paymentRepository.GetAllWithPathsAsync(p => p.Date >= from && p.Date <= to, "Invoice.Project");
            var expenses = await _repository.GetAllAsync(e => e.Date >= from && e.Date <= to, e => e.Project!);

            var bookings = payments.Select(IncomeBooking)
                .Concat(expenses.Select(ExpenseBooking))
                .OrderBy(b => b.Date).ThenBy(b => b.IsIncome ? 0 : 1)
                .ToList();

            var income = bookings.Where(b => b.IsIncome).ToList();
            var spent = bookings.Where(b => !b.IsIncome).ToList();

            return new YearOverviewDto
            {
                Year = year,
                IsSmallBusiness = settings.IsSmallBusiness,
                IncomeTotal = income.Sum(b => b.Gross),
                ExpensesTotal = spent.Sum(b => b.Gross),
                Result = income.Sum(b => b.Gross) - spent.Sum(b => b.Gross),
                IncomeVat = income.Sum(b => b.Vat),
                ExpensesVat = spent.Sum(b => b.Vat),
                Months = Enumerable.Range(1, 12).Select(month => new MonthTotalDto
                {
                    Month = month,
                    Income = income.Where(b => b.Date.Month == month).Sum(b => b.Gross),
                    Expenses = spent.Where(b => b.Date.Month == month).Sum(b => b.Gross)
                }).ToList(),
                Categories = spent
                    .GroupBy(b => b.Category!.Value)
                    .Select(g => new CategoryTotalDto { Category = g.Key, Amount = g.Sum(b => b.Gross) })
                    .OrderByDescending(c => c.Amount)
                    .ToList(),
                Bookings = bookings
            };
        }

        private static BookingDto IncomeBooking(Payment payment)
        {
            var invoice = payment.Invoice!;

            // The VAT part of a payment has the same share as on the invoice (0 for small business invoices)
            var vat = invoice.GrossTotal != 0 ? Math.Round(payment.Amount * invoice.VatAmount / invoice.GrossTotal, 2) : 0;

            return new BookingDto
            {
                Date = payment.Date,
                IsIncome = true,
                SourceId = invoice.Id,
                InvoiceNumber = invoice.Number,
                Description = payment.Note ?? string.Empty,
                Party = invoice.RecipientName,
                ProjectTitle = invoice.Project?.Title,
                Net = payment.Amount - vat,
                Vat = vat,
                Gross = payment.Amount
            };
        }

        private static BookingDto ExpenseBooking(Expense expense)
        {
            var net = NetOf(expense.GrossAmount, expense.VatRate);

            return new BookingDto
            {
                Date = expense.Date,
                IsIncome = false,
                SourceId = expense.Id,
                Description = expense.Description,
                Party = expense.Supplier,
                Category = expense.Category,
                ProjectTitle = expense.Project?.Title,
                Net = net,
                Vat = expense.GrossAmount - net,
                Gross = expense.GrossAmount,
                HasReceipt = expense.ReceiptPath != null
            };
        }

        // ---------- Helpers ----------

        private static (DateOnly From, DateOnly To) YearRange(int year) => (new DateOnly(year, 1, 1), new DateOnly(year, 12, 31));

        private static void Clean(ExpenseCreateDto dto)
        {
            dto.Description = dto.Description.Trim();
            dto.Supplier = string.IsNullOrWhiteSpace(dto.Supplier) ? null : dto.Supplier.Trim();
            dto.Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim();
        }

        private async Task<Expense> LoadExpense(int id)
        {
            return await _repository.GetAsync(id) ?? throw new EntityNotFoundException(nameof(Expense), id);
        }
    }
}
