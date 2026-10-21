using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.FinanceModels;
using StudioManager.RepositoryLayer.IRepository;
using StudioManager.ServiceLayer.IService.IFileServices;
using StudioManager.ServiceLayer.IService.IFinanceServices;
using StudioManager.ServiceLayer.Results;
using System.Globalization;
using System.IO.Compression;
using System.Text;
using System.Text.Json;

namespace StudioManager.ServiceLayer.Service.FinanceServices
{
    public class FinanceArchiveService : IFinanceArchiveService
    {
        public const string ArchiveFolder = "exports";

        /// <summary>Written by scripts/backup.sh after every backup (in the root of the file storage).</summary>
        public const string BackupStatusFile = "backup-status.json";

        private static readonly CultureInfo German = CultureInfo.GetCultureInfo("de-DE");

        /// <summary>German names of the categories - the archive is for the tax office / tax advisor.</summary>
        private static readonly Dictionary<ExpenseCategory, string> CategoryNames = new()
        {
            [ExpenseCategory.Equipment] = "Technik & Instrumente",
            [ExpenseCategory.Software] = "Software & Plugins",
            [ExpenseCategory.Material] = "Verbrauchsmaterial",
            [ExpenseCategory.ExternalServices] = "Fremdleistungen",
            [ExpenseCategory.Rent] = "Miete & Nebenkosten",
            [ExpenseCategory.Marketing] = "Werbung & Website",
            [ExpenseCategory.Travel] = "Fahrtkosten & Reisen",
            [ExpenseCategory.Insurance] = "Versicherungen & Beiträge",
            [ExpenseCategory.Office] = "Büro, Telefon & Internet",
            [ExpenseCategory.Other] = "Sonstiges"
        };

        private static readonly Dictionary<InvoicePaymentStatus, string> PaymentStatusNames = new()
        {
            [InvoicePaymentStatus.Draft] = "Entwurf",
            [InvoicePaymentStatus.Open] = "offen",
            [InvoicePaymentStatus.PartiallyPaid] = "teilweise bezahlt",
            [InvoicePaymentStatus.Paid] = "bezahlt",
            [InvoicePaymentStatus.Overdue] = "überfällig",
            [InvoicePaymentStatus.Cancelled] = "storniert",
            [InvoicePaymentStatus.Cancellation] = "Stornorechnung"
        };

        private readonly IInvoiceService _invoiceService;
        private readonly IExpenseService _expenseService;
        private readonly IRepository<Expense> _expenseRepository;
        private readonly IFileStorageService _fileStorage;

        public FinanceArchiveService(
            IInvoiceService invoiceService,
            IExpenseService expenseService,
            IRepository<Expense> expenseRepository,
            IFileStorageService fileStorage)
        {
            _invoiceService = invoiceService;
            _expenseService = expenseService;
            _expenseRepository = expenseRepository;
            _fileStorage = fileStorage;
        }

        public static string ArchiveName(int year) => $"finanzen-{year}.zip";

        public async Task<FileDownload> CreateArchive(int year)
        {
            var content = await BuildZip(year);
            await _fileStorage.WriteAsync($"{ArchiveFolder}/{ArchiveName(year)}", new MemoryStream(content));
            return new FileDownload(new MemoryStream(content), "application/zip", ArchiveName(year));
        }

        public async Task WriteCurrentArchives()
        {
            var today = DateOnly.FromDateTime(DateTime.Now);
            await CreateArchive(today.Year);
            if (today.Month <= 3)
            {
                await CreateArchive(today.Year - 1);
            }
        }

        public Task<DataSafetyStatusDto> GetStatus()
        {
            var status = new DataSafetyStatusDto();

            using (var stream = _fileStorage.OpenRead(BackupStatusFile))
            {
                if (stream != null)
                {
                    var backup = JsonSerializer.Deserialize<BackupStatus>(stream, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    status.LastBackupAt = backup?.FinishedAt?.UtcDateTime;
                    status.LastBackupDatabaseBytes = backup?.DatabaseBytes;
                    status.OffsiteTarget = string.IsNullOrEmpty(backup?.Offsite) ? null : backup.Offsite;
                    status.OffsiteOk = status.OffsiteTarget == null ? null : backup?.OffsiteOk;
                }
            }

            status.Archives = _fileStorage.List(ArchiveFolder)
                .Select(file => new { File = file, Year = YearOfArchive(file.RelativePath) })
                .Where(a => a.Year != null)
                .Select(a => new FinanceArchiveDto { Year = a.Year!.Value, SizeBytes = a.File.Length, CreatedAt = a.File.LastModifiedUtc })
                .OrderByDescending(a => a.Year)
                .ToList();

            return Task.FromResult(status);
        }

        // ---------- The ZIP ----------

        private async Task<byte[]> BuildZip(int year)
        {
            var (from, to) = (new DateOnly(year, 1, 1), new DateOnly(year, 12, 31));
            var overview = await _expenseService.GetYearOverview(year);
            var invoices = (await _invoiceService.GetAll())
                .Where(i => i.Number != null && i.InvoiceDate >= from && i.InvoiceDate <= to)
                .OrderBy(i => i.Number)
                .ToList();
            var expenses = (await _expenseRepository.GetAllAsync(e => e.Date >= from && e.Date <= to, e => e.Project!))
                .OrderBy(e => e.Date).ThenBy(e => e.Id)
                .ToList();

            using var memory = new MemoryStream();
            using (var zip = new ZipArchive(memory, ZipArchiveMode.Create, leaveOpen: true))
            {
                AddText(zip, "LIESMICH.txt", ReadMe(year, invoices.Count, expenses.Count));
                AddText(zip, $"einnahmen-ausgaben-{year}.csv", BookingsCsv(overview));
                AddText(zip, $"rechnungen-{year}.csv", InvoicesCsv(invoices));

                foreach (var invoice in invoices)
                {
                    AddText(zip, $"rechnungen/{SafeFileName(invoice.Number!)}.html", InvoiceArchiveHtml.Render(invoice));
                }

                // The receipts with readable names: "belege/2026-03-12_Neumann-U87_7.pdf"
                var receiptNames = new Dictionary<int, string>();
                foreach (var expense in expenses.Where(e => e.ReceiptPath != null))
                {
                    using var receipt = _fileStorage.OpenRead(expense.ReceiptPath!);
                    if (receipt == null)
                    {
                        continue; // the file is missing - the CSV then shows no file name
                    }

                    var name = $"belege/{expense.Date:yyyy-MM-dd}_{SafeFileName(expense.Description)}_{expense.Id}{Path.GetExtension(expense.ReceiptPath)}";
                    var entry = zip.CreateEntry(name);
                    await using (var target = entry.Open())
                    {
                        await receipt.CopyToAsync(target);
                    }
                    receiptNames[expense.Id] = name;
                }

                AddText(zip, $"ausgaben-{year}.csv", ExpensesCsv(expenses, receiptNames));
            }

            return memory.ToArray();
        }

        private static string ReadMe(int year, int invoiceCount, int expenseCount) => $"""
            Scumlight Studio Manager - Finanzarchiv {year}
            Erstellt am {DateTime.Now.ToString("dd.MM.yyyy HH:mm", German)}

            einnahmen-ausgaben-{year}.csv  Alle Zahlungseingänge und Ausgaben des Jahres (Grundlage für die EÜR)
            rechnungen-{year}.csv          Alle {invoiceCount} ausgestellten Rechnungen mit Status und offenem Betrag
            ausgaben-{year}.csv            Alle {expenseCount} Ausgaben mit Kategorie und Dateiname des Belegs
            rechnungen/                    Jede Rechnung als HTML-Datei (im Browser öffnen, als PDF drucken)
            belege/                        Die hochgeladenen Belege

            Die CSV-Dateien öffnen sich direkt in Excel oder LibreOffice (Trennzeichen Semikolon, Dezimalkomma).
            Das Archiv wird jede Nacht neu geschrieben und mit der Datensicherung gesichert.
            """;

        private static string BookingsCsv(YearOverviewDto overview)
        {
            var csv = new CsvBuilder("Datum", "Art", "Rechnungsnr.", "Beschreibung", "Kategorie", "Kunde / Lieferant", "Projekt", "Netto", "USt.", "Brutto", "Beleg");
            foreach (var b in overview.Bookings)
            {
                // Expenses negative, so the sum of the column is the result
                var sign = b.IsIncome ? 1 : -1;
                var description = b.IsIncome ? $"Zahlung Rechnung {b.InvoiceNumber}" + (b.Description != "" ? $" ({b.Description})" : "") : b.Description;
                csv.Add(Date(b.Date), b.IsIncome ? "Einnahme" : "Ausgabe", b.InvoiceNumber, description,
                    b.Category != null ? CategoryNames[b.Category.Value] : null, b.Party, b.ProjectTitle,
                    Money(sign * b.Net), Money(sign * b.Vat), Money(sign * b.Gross), b.IsIncome ? null : (b.HasReceipt ? "ja" : "nein"));
            }
            return csv.ToString();
        }

        private static string InvoicesCsv(List<InvoiceReadDto> invoices)
        {
            var csv = new CsvBuilder("Rechnungsnr.", "Datum", "Fällig", "Kunde", "Projekt", "Netto", "USt.", "Brutto", "Bezahlt", "Offen", "Status", "Storniert durch / storniert");
            foreach (var i in invoices)
            {
                csv.Add(i.Number, Date(i.InvoiceDate), i.DueDate != null ? Date(i.DueDate.Value) : null, i.RecipientName, i.ProjectTitle,
                    Money(i.NetTotal), Money(i.VatAmount), Money(i.GrossTotal), Money(i.PaidAmount), Money(i.OpenAmount),
                    PaymentStatusNames[i.PaymentStatus], i.CancelledByNumber ?? i.CancelsInvoiceNumber);
            }
            return csv.ToString();
        }

        private static string ExpensesCsv(List<Expense> expenses, Dictionary<int, string> receiptNames)
        {
            var csv = new CsvBuilder("Datum", "Beschreibung", "Lieferant", "Kategorie", "Projekt", "Netto", "USt.", "USt.-Satz", "Brutto", "Beleg", "Notizen");
            foreach (var e in expenses)
            {
                var net = ExpenseService.NetOf(e.GrossAmount, e.VatRate);
                csv.Add(Date(e.Date), e.Description, e.Supplier, CategoryNames[e.Category], e.Project?.Title,
                    Money(net), Money(e.GrossAmount - net), e.VatRate.ToString("0.##", German) + " %", Money(e.GrossAmount),
                    receiptNames.GetValueOrDefault(e.Id) ?? "fehlt", e.Notes);
            }
            return csv.ToString();
        }

        // ---------- Helpers ----------

        private static void AddText(ZipArchive zip, string name, string text)
        {
            var entry = zip.CreateEntry(name);
            using var writer = new StreamWriter(entry.Open(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
            writer.Write(text);
        }

        /// <summary>"finanzen-2026.zip" -> 2026</summary>
        private static int? YearOfArchive(string relativePath)
        {
            var name = Path.GetFileNameWithoutExtension(relativePath);
            return name.StartsWith("finanzen-") && int.TryParse(name["finanzen-".Length..], out var year) ? year : null;
        }

        /// <summary>A text usable as file name: "Neumann U87 / Ai" -> "Neumann-U87-Ai" (max. 60 characters).</summary>
        public static string SafeFileName(string text)
        {
            // Fixed list instead of Path.GetInvalidFileNameChars() - on Linux that only contains "/", but the ZIP is
            // opened on Windows, where : * ? " < > | are not allowed either
            const string invalid = " /\\:*?\"<>|";
            var cleaned = new string(text.Select(c => invalid.Contains(c) || char.IsControl(c) ? '-' : c).ToArray());
            while (cleaned.Contains("--"))
            {
                cleaned = cleaned.Replace("--", "-");
            }
            cleaned = cleaned.Trim('-', '.');
            return cleaned.Length > 60 ? cleaned[..60] : cleaned;
        }

        private static string Money(decimal value) => value.ToString("0.00", German);

        private static string Date(DateOnly date) => date.ToString("dd.MM.yyyy", German);

        /// <summary>The content of backup-status.json (see scripts/backup.sh).</summary>
        private class BackupStatus
        {
            public DateTimeOffset? FinishedAt { get; set; }
            public long? DatabaseBytes { get; set; }
            public string? Offsite { get; set; }
            public bool? OffsiteOk { get; set; }
        }

        /// <summary>
        /// A CSV file as Excel and LibreOffice open it in Germany: separator ";", line break CRLF,
        /// texts with ; " or line breaks in quotes.
        /// </summary>
        private class CsvBuilder
        {
            private readonly StringBuilder _text = new();

            public CsvBuilder(params string[] header) => Add(header);

            public void Add(params string?[] cells)
            {
                _text.Append(string.Join(';', cells.Select(Cell))).Append("\r\n");
            }

            private static string Cell(string? value)
            {
                var text = value ?? string.Empty;
                return text.IndexOfAny([';', '"', '\n', '\r']) >= 0 ? $"\"{text.Replace("\"", "\"\"")}\"" : text;
            }

            public override string ToString() => _text.ToString();
        }
    }
}
