
using FarmingApi.Modules.Administration.DocumentNumberRange;
using FarmingApi.Modules.Financials.GLAccountDetermination;
using FarmingApi.Modules.Financials.JournalEntry;
using FarmingApi.Modules.SaleAR.ARInvoice;
using BPEntity = FarmingApi.Modules.BusinessPartners.BusinessPartnersMaster.BusinessPartnersMaster;
namespace FarmingApi.Services;

// ═══════════════════════════════════════════════════════════════════
// Place this file at:  /ERP/Services/DocumentNumberService.cs
// Register in Program.cs:
//   builder.Services.AddScoped<IDocumentNumberService, DocumentNumberService>();
// ═══════════════════════════════════════════════════════════════════

public interface IDocumentNumberService
{
    string Next(string documentType, string? seriesName = null);
    string Peek(string documentType, string? seriesName = null);
}

public class DocumentNumberService : IDocumentNumberService
{
    private readonly MyDbContext _db;

    public DocumentNumberService(MyDbContext db) => _db = db;

    public string Next(string documentType, string? seriesName = null)
    {
        var range = GetRange(documentType, seriesName);

        if (range == null)
            throw new InvalidOperationException(
                $"No number range configured for '{documentType}'. " +
                "Go to Administration → Document Numbering to create one.");

        if (range.IsLocked)
            throw new InvalidOperationException(
                $"Series '{range.SeriesName}' for '{documentType}' is locked.");

        if (range.LastNum.HasValue && range.NextNum > range.LastNum.Value)
            throw new InvalidOperationException(
                $"Number range for '{documentType}' is exhausted (max: {range.LastNum}).");

        int num = range.NextNum;
        range.NextNum++;
        range.UpdatedAt = DateTime.UtcNow;
        _db.SaveChanges();
        return Format(range, num);
    }

    public string Peek(string documentType, string? seriesName = null)
    {
        var range = GetRange(documentType, seriesName);
        if (range == null) return $"{documentType}-NOT-CONFIGURED";
        return Format(range, range.NextNum);
    }

    private DocumentNumberRange? GetRange(string documentType, string? seriesName)
    {
        var q = _db.Set<DocumentNumberRange>()
                   .Where(d => d.DocumentType == documentType && !d.IsLocked);
        return !string.IsNullOrEmpty(seriesName)
            ? q.FirstOrDefault(d => d.SeriesName == seriesName)
            : q.FirstOrDefault(d => d.IsDefault) ?? q.FirstOrDefault();
    }

    public static string Format(DocumentNumberRange range, int num)
    {
        var parts = new List<string> { range.Prefix };
        if (range.IncludeYear) parts.Add(DateTime.Now.Year.ToString());
        parts.Add(num.ToString().PadLeft(range.PadLength, '0'));
        return string.Join("-", parts);
    }

}