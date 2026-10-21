using StudioManager.DataLayer.Context;
using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.Models.ClientModels;
using StudioManager.DataLayer.Models.FinanceModels;
using StudioManager.DataLayer.Models.ProjectModels;
using StudioManager.RepositoryLayer.Repository;
using StudioManager.ServiceLayer.Service.FileServices;
using StudioManager.ServiceLayer.Service.FinanceServices;
using System.IO.Compression;
using System.Text;

namespace StudioManager.Tests.Unit
{
    /// <summary>
    /// Tests for the finance archive (ZIP per year) and the backup status. Uses a real file storage in a temp folder.
    /// </summary>
    public class FinanceArchiveTests : IDisposable
    {
        private readonly AppDbContext _context = TestHelper.CreateDbContext();
        private readonly string _storageRoot = Path.Combine(Path.GetTempPath(), "studio-archive-test-" + Guid.NewGuid().ToString("N"));
        private readonly LocalFileStorageService _fileStorage;
        private readonly InvoiceService _invoiceService;
        private readonly ExpenseService _expenseService;
        private readonly FinanceArchiveService _archiveService;
        private readonly Project _project;
        private readonly DateOnly _date = new(2026, 10, 21);

        public FinanceArchiveTests()
        {
            var mapper = TestHelper.CreateMapper();
            _fileStorage = new LocalFileStorageService(_storageRoot);
            _invoiceService = new InvoiceService(
                new Repository<Invoice>(_context), new Repository<Payment>(_context), new Repository<Project>(_context),
                new Repository<TimeEntry>(_context), new Repository<ProjectCostItem>(_context), new Repository<BusinessSettings>(_context),
                new Repository<Quote>(_context), mapper);
            _expenseService = new ExpenseService(
                new Repository<Expense>(_context), new Repository<Payment>(_context), new Repository<BusinessSettings>(_context), _fileStorage, mapper);
            _archiveService = new FinanceArchiveService(_invoiceService, _expenseService, new Repository<Expense>(_context), _fileStorage);

            _project = new Project
            {
                Title = "Album", Client = new Client { Name = "Label GmbH", Address = "Musterstr. 1" },
                PricingMode = PricingMode.FlatPrice, FlatPrice = 1000
            };
            _context.Projects.Add(_project);
            _context.BusinessSettings.Add(new BusinessSettings
            {
                CompanyName = "Scumlight Studio", Street = "Studioweg 1", PostalCode = "54321", City = "Rockstadt",
                TaxNumber = "12/345/67890", Iban = "DE02120300000000202051", IsSmallBusiness = true, NextInvoiceNumber = 18
            });
            _context.SaveChanges();
        }

        public void Dispose()
        {
            if (Directory.Exists(_storageRoot))
            {
                Directory.Delete(_storageRoot, recursive: true);
            }
        }

        private static string ReadEntry(ZipArchive zip, string name)
        {
            var entry = zip.GetEntry(name) ?? throw new Exception($"{name} is missing in the archive");
            using var reader = new StreamReader(entry.Open(), Encoding.UTF8);
            return reader.ReadToEnd();
        }

        [Fact]
        public async Task CreateArchive_ShouldContainTheListsTheInvoicesAndTheReceipts()
        {
            var draft = await _invoiceService.CreateDraft(_project.Id);
            var invoice = await _invoiceService.Issue(draft.Id, new InvoiceDateDto { Date = _date });
            await _invoiceService.AddPayment(invoice.Id, new PaymentCreateDto { Date = _date, Amount = 400 });

            var expense = await _expenseService.Insert(new ExpenseCreateDto
            {
                Date = _date, Description = "Neumann U87 / Ai", Supplier = "Thomann; Treppendorf", Category = ExpenseCategory.Equipment,
                GrossAmount = 2499, VatRate = 19
            });
            await _expenseService.SetReceipt(expense.Id, new MemoryStream(Encoding.UTF8.GetBytes("%PDF receipt")), "rechnung.pdf", 12);

            var download = await _archiveService.CreateArchive(2026);

            Assert.Equal("finanzen-2026.zip", download.FileName);
            using var zip = new ZipArchive(download.Content, ZipArchiveMode.Read);
            var names = zip.Entries.Select(e => e.FullName).ToList();
            Assert.Contains("LIESMICH.txt", names);
            Assert.Contains($"rechnungen/{invoice.Number}.html", names);
            Assert.Contains($"belege/2026-10-21_Neumann-U87-Ai_{expense.Id}.pdf", names);

            var bookings = ReadEntry(zip, "einnahmen-ausgaben-2026.csv");
            Assert.Contains($"21.10.2026;Einnahme;{invoice.Number};Zahlung Rechnung {invoice.Number};;Label GmbH;Album;400,00;0,00;400,00;", bookings);
            // A text with ";" is put in quotes, expenses are negative
            Assert.Contains("\"Thomann; Treppendorf\"", bookings);
            Assert.Contains("-2499,00", bookings);

            var invoices = ReadEntry(zip, "rechnungen-2026.csv");
            Assert.Contains($"{invoice.Number};21.10.2026;", invoices);
            Assert.Contains("1000,00;400,00;600,00;teilweise bezahlt", invoices);

            var html = ReadEntry(zip, $"rechnungen/{invoice.Number}.html");
            Assert.Contains("Gemäß § 19 UStG wird keine Umsatzsteuer berechnet.", html);
            Assert.Contains("IBAN: DE02 1203 0000 0000 2020 51", html);
            Assert.Contains("1.000,00 €", html);

            // The archive is also saved, so the nightly backup takes it along
            var archive = Assert.Single((await _archiveService.GetStatus()).Archives);
            Assert.Equal(2026, archive.Year);
        }

        [Fact]
        public async Task GetStatus_ShouldReadTheBackupStatusOfBackupSh()
        {
            Assert.Null((await _archiveService.GetStatus()).LastBackupAt);

            var json = """{"finishedAt":"2026-10-21T03:00:12+02:00","databaseBytes":123456,"offsite":"hidrive:studio","offsiteOk":true}""";
            await _fileStorage.WriteAsync(FinanceArchiveService.BackupStatusFile, new MemoryStream(Encoding.UTF8.GetBytes(json)));

            var status = await _archiveService.GetStatus();

            Assert.Equal(new DateTime(2026, 10, 21, 1, 0, 12, DateTimeKind.Utc), status.LastBackupAt);
            Assert.Equal(123456, status.LastBackupDatabaseBytes);
            Assert.Equal("hidrive:studio", status.OffsiteTarget);
            Assert.True(status.OffsiteOk);
        }

        [Fact]
        public void SafeFileName_ShouldRemoveCharactersThatAreNotAllowedInFileNames()
        {
            Assert.Equal("Neumann-U87-Ai", FinanceArchiveService.SafeFileName("Neumann U87 / Ai"));
            Assert.Equal("Kabel-Stecker", FinanceArchiveService.SafeFileName("Kabel: \"Stecker\""));
        }
    }
}
