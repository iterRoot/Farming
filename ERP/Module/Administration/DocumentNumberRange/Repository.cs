using FarmingApi.Core;
using FarmingApi.Modules.Administration.DocumentNumberRange;

namespace FarmingApi.Modules.Administration.DocumentNumberRange;

public interface IDocumentNumberRangeRepository : IRepository<DocumentNumberRange>
{
    // Get default series for a document type
    DocumentNumberRange? GetDefault(string documentType);

    // Generate and reserve the next document number (atomic)
    NextDocNumberResponse GenerateNextNumber(string documentType, string? seriesName = null);
}

public class DocumentNumberRangeRepository
    : Repository<DocumentNumberRange>, IDocumentNumberRangeRepository
{
    private readonly MyDbContext _db;

    public DocumentNumberRangeRepository(MyDbContext context) : base(context)
    {
        _db = context;
    }

    // ── Get default series for a document type ────────────────────
    public DocumentNumberRange? GetDefault(string documentType) =>
        _db.Set<DocumentNumberRange>()
           .FirstOrDefault(d => d.DocumentType == documentType && d.IsDefault && !d.IsLocked);

    // ── Generate + Reserve next number (thread-safe) ──────────────
    public NextDocNumberResponse GenerateNextNumber(string documentType, string? seriesName = null)
    {
        // Use EF in a way that reads + increments in one operation
        DocumentNumberRange? range;

        if (!string.IsNullOrEmpty(seriesName))
            range = _db.Set<DocumentNumberRange>()
                       .FirstOrDefault(d => d.DocumentType == documentType
                                         && d.SeriesName   == seriesName
                                         && !d.IsLocked);
        else
            range = GetDefault(documentType);

        if (range == null)
            throw new InvalidOperationException(
                $"No active number range found for '{documentType}'. " +
                "Please configure one in Administration → Document Numbering.");

        // Check upper limit
        if (range.LastNum.HasValue && range.NextNum > range.LastNum.Value)
            throw new InvalidOperationException(
                $"Number range for '{documentType}' has been exhausted (max: {range.LastNum}). " +
                "Please extend the range or create a new series.");

        // Reserve the number
        int num    = range.NextNum;
        range.NextNum++;
        range.UpdatedAt = DateTime.UtcNow;
        _db.SaveChanges();

        return new NextDocNumberResponse
        {
            DocNo      = FormatDocNo(range, num),
            Number     = num,
            SeriesName = range.SeriesName,
        };
    }

    // ── Format: PREFIX-YEAR-NNNNN ─────────────────────────────────
    public static string FormatDocNo(DocumentNumberRange range, int num)
    {
        var parts = new List<string> { range.Prefix };
        if (range.IncludeYear) parts.Add(DateTime.Now.Year.ToString());
        parts.Add(num.ToString().PadLeft(range.PadLength, '0'));
        return string.Join("-", parts);
    }
}