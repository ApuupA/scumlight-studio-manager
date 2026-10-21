using StudioManager.DataLayer.DTO.Read;
using System.Globalization;
using System.Net;
using System.Text;

namespace StudioManager.ServiceLayer.Service.FinanceServices
{
    /// <summary>
    /// An issued invoice as one self-contained HTML file for the finance archive: same content as the printed
    /// invoice (web/src/features/invoices/PrintDocument.tsx), opens in any browser - also without the app.
    /// German texts, because the archive is for the German tax office.
    /// </summary>
    public static class InvoiceArchiveHtml
    {
        private static readonly CultureInfo German = CultureInfo.GetCultureInfo("de-DE");

        private const string Css = """
            body { font-family: Arial, sans-serif; font-size: 10pt; color: #000; max-width: 180mm; margin: 15mm auto; }
            .company { text-align: right; }
            .address { display: flex; justify-content: space-between; margin-top: 15mm; }
            .sender-line { font-size: 7pt; border-bottom: 1px solid #999; margin-bottom: 2mm; }
            .recipient { white-space: pre-line; }
            h1 { font-size: 14pt; margin-top: 12mm; }
            table { border-collapse: collapse; }
            .lines { width: 100%; margin-top: 4mm; }
            .lines th { border-bottom: 2px solid #cc3300; text-align: left; font-size: 8pt; }
            .lines td { border-bottom: 1px solid #ddd; padding: 1.5mm 0; }
            .number { text-align: right; }
            .totals { margin-left: auto; margin-top: 4mm; }
            .totals td { padding: 1mm 0 1mm 10mm; }
            .grand td { font-weight: bold; border-top: 2px solid #000; }
            .text { white-space: pre-line; }
            .footer { display: flex; justify-content: space-between; margin-top: 15mm; font-size: 8pt; border-top: 1px solid #999; padding-top: 2mm; }
            """;

        public static string Render(InvoiceReadDto invoice)
        {
            var sender = invoice.Sender;
            var senderName = sender.CompanyName ?? sender.OwnerName ?? string.Empty;
            var senderCity = JoinNotEmpty(" ", sender.PostalCode, sender.City);
            var isCancellation = invoice.CancelsInvoiceId != null;
            var title = $"{(isCancellation ? "Stornorechnung" : "Rechnung")} {invoice.Number}";

            var html = new StringBuilder();
            html.AppendLine("<!doctype html>");
            html.AppendLine("<html lang=\"de\"><head><meta charset=\"utf-8\">");
            html.AppendLine($"<title>{E(title)}</title>");
            html.AppendLine($"<style>{Css}</style>");
            html.AppendLine("</head><body>");

            // ---------- Sender (top right) ----------
            html.Append("<div class=\"company\">");
            html.Append($"<strong>{E(senderName)}</strong><br>");
            if (sender.CompanyName != null && sender.OwnerName != null)
            {
                html.Append(Line(sender.OwnerName));
            }
            html.Append(Line(sender.Street)).Append(Line(senderCity)).Append(Line(sender.Phone)).Append(Line(sender.Email)).Append(E(sender.Website));
            html.AppendLine("</div>");

            // ---------- Recipient and details ----------
            html.AppendLine("<div class=\"address\"><div>");
            html.AppendLine($"<div class=\"sender-line\">{E(JoinNotEmpty(" · ", senderName, sender.Street, senderCity))}</div>");
            html.Append($"<div class=\"recipient\"><strong>{E(invoice.RecipientName)}</strong>");
            if (invoice.RecipientAddress != null)
            {
                html.Append('\n').Append(E(invoice.RecipientAddress));
            }
            html.AppendLine("</div></div>");

            html.AppendLine("<table>");
            html.AppendLine(DetailRow("Rechnungsnr.", E(invoice.Number)));
            html.AppendLine(DetailRow("Rechnungsdatum", Date(invoice.InvoiceDate)));
            if (invoice.ServiceFrom != null || invoice.ServiceTo != null)
            {
                html.AppendLine(DetailRow("Leistungszeitraum", ServicePeriod(invoice)));
            }
            html.AppendLine(DetailRow("Projekt", E(invoice.ProjectTitle)));
            html.AppendLine("</table></div>");

            // ---------- Title, intro and lines ----------
            html.AppendLine($"<h1>{E(title)}</h1>");
            if (invoice.IntroText != null)
            {
                html.AppendLine($"<p class=\"text\">{E(invoice.IntroText)}</p>");
            }

            html.AppendLine("<table class=\"lines\">");
            html.AppendLine("<tr><th>Pos.</th><th>Beschreibung</th><th class=\"number\">Menge</th><th class=\"number\">Einzelpreis</th><th class=\"number\">Gesamt</th></tr>");
            for (var i = 0; i < invoice.Lines.Count; i++)
            {
                var line = invoice.Lines[i];
                html.Append($"<tr><td>{i + 1}</td><td>{E(line.Description)}</td>");
                html.Append($"<td class=\"number\">{line.Quantity.ToString("0.##", German)} {E(line.Unit)}</td>");
                html.AppendLine($"<td class=\"number\">{Money(line.UnitPrice)}</td><td class=\"number\">{Money(line.Total)}</td></tr>");
            }
            html.AppendLine("</table>");

            // ---------- Totals ----------
            html.AppendLine("<table class=\"totals\">");
            if (!invoice.IsSmallBusiness)
            {
                html.AppendLine($"<tr><td>Netto</td><td class=\"number\">{Money(invoice.NetTotal)}</td></tr>");
                html.AppendLine($"<tr><td>USt. {invoice.VatRate.ToString("0.##", German)} %</td><td class=\"number\">{Money(invoice.VatAmount)}</td></tr>");
            }
            html.AppendLine($"<tr class=\"grand\"><td>Gesamtbetrag</td><td class=\"number\">{Money(invoice.GrossTotal)}</td></tr>");
            html.AppendLine("</table>");

            // ---------- Notes ----------
            // Required on documents of small businesses
            if (invoice.IsSmallBusiness)
            {
                html.AppendLine("<p>Gemäß § 19 UStG wird keine Umsatzsteuer berechnet.</p>");
            }
            if (!isCancellation && invoice.DueDate != null)
            {
                html.AppendLine($"<p>Bitte überweisen Sie {Money(invoice.GrossTotal)} bis zum {Date(invoice.DueDate.Value)} auf das unten genannte Konto.</p>");
            }
            if (invoice.ClosingText != null)
            {
                html.AppendLine($"<p class=\"text\">{E(invoice.ClosingText)}</p>");
            }

            // ---------- Footer with the business data ----------
            html.AppendLine("<div class=\"footer\">");
            html.AppendLine($"<div>{Line(senderName)}{Line(sender.Street)}{E(senderCity)}</div>");
            html.Append("<div>");
            if (sender.TaxNumber != null)
            {
                html.Append($"Steuernummer: {Line(sender.TaxNumber)}");
            }
            if (sender.VatId != null)
            {
                html.Append($"USt-IdNr.: {Line(sender.VatId)}");
            }
            html.AppendLine($"{E(sender.Email)}</div>");
            html.Append("<div>").Append(Line(sender.BankName));
            if (sender.Iban != null)
            {
                // "DE12 3456 ..." - in blocks of four like on the printed invoice
                html.Append($"IBAN: {Line(string.Join(" ", sender.Iban.Chunk(4).Select(c => new string(c))))}");
            }
            if (sender.Bic != null)
            {
                html.Append($"BIC: {E(sender.Bic)}");
            }
            html.AppendLine("</div>");
            html.AppendLine("</div>");

            html.AppendLine("</body></html>");
            return html.ToString();
        }

        private static string ServicePeriod(InvoiceReadDto invoice)
        {
            if (invoice.ServiceFrom == null || invoice.ServiceTo == null || invoice.ServiceFrom == invoice.ServiceTo)
            {
                return Date((invoice.ServiceFrom ?? invoice.ServiceTo)!.Value);
            }
            return $"{Date(invoice.ServiceFrom.Value)} – {Date(invoice.ServiceTo.Value)}";
        }

        private static string DetailRow(string label, string value) => $"<tr><td>{label}&nbsp;&nbsp;</td><td>{value}</td></tr>";

        private static string JoinNotEmpty(string separator, params string?[] parts) =>
            string.Join(separator, parts.Where(p => !string.IsNullOrEmpty(p)));

        /// <summary>HTML-encodes a text (null = empty).</summary>
        private static string E(string? text) => WebUtility.HtmlEncode(text ?? string.Empty);

        /// <summary>The text with a line break, or nothing if it is empty.</summary>
        private static string Line(string? text) => string.IsNullOrEmpty(text) ? string.Empty : E(text) + "<br>";

        private static string Money(decimal value) => value.ToString("#,##0.00", German) + " €";

        private static string Date(DateOnly date) => date.ToString("dd.MM.yyyy", German);
    }
}
