using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using BPEntity  = FarmingApi.Modules.BusinessPartners.BusinessPartnersMaster.BusinessPartnersMaster;
using COAEntity  = FarmingApi.Modules.Financials.ChartOfAccounts.ChartOfAccounts;
using ItemEntity = FarmingApi.Modules.Inventory.ItemsMaster.ItemsMaster;
using CompanyEntity = FarmingApi.Modules.Company.Company;
using UserEntity = FarmingApi.Modules.Administration.SetUp.User.User;
using UserGroupEntity = FarmingApi.Modules.Administration.SetUp.UserGroup.UserGroup;
using ItemGroupEntity = FarmingApi.Modules.Inventory.ItemGroup.ItemGroup;
using EmployeeEntity = FarmingApi.Modules.HumanResources.EmployeeMaster.EmployeeMaster;
using AttendanceEntity = FarmingApi.Modules.HumanResources.Attendance.Attendance;
using PayrollEntity = FarmingApi.Modules.HumanResources.Payroll.Payroll;
using ExchangeRateEntity = FarmingApi.Modules.Financials.ExchangeRate.ExchangeRate;
using HrAttendanceMath = FarmingApi.Modules.HumanResources.Attendance.AttendanceMath;
using HrPayrollMath = FarmingApi.Modules.HumanResources.Payroll.PayrollMath;
using FarmingApi.Modules.Inventory.ItemsMaster;

namespace FarmingApi.Modules.Administration.DataImportAndExport;

// ═══════════════════════════════════════════════════════════════════
// CSV ENGINE  (RFC 4180)
//
// Deliberately hand-rolled rather than pulling in a package: it is a
// well-defined format, this handles the three cases that actually bite
// (quoted fields, embedded commas/newlines, doubled quotes), and it
// keeps the project's dependency list where it is.
// ═══════════════════════════════════════════════════════════════════
public static class Csv
{
    public static List<string[]> Parse(string text)
    {
        var rows   = new List<string[]>();
        var row    = new List<string>();
        var field  = new StringBuilder();
        var inQuotes = false;

        // Strip a UTF-8 BOM — Excel writes one and it corrupts the first header.
        if (text.Length > 0 && text[0] == '﻿') text = text[1..];

        for (var i = 0; i < text.Length; i++)
        {
            var c = text[i];

            if (inQuotes)
            {
                if (c == '"')
                {
                    if (i + 1 < text.Length && text[i + 1] == '"') { field.Append('"'); i++; }
                    else inQuotes = false;
                }
                else field.Append(c);
                continue;
            }

            switch (c)
            {
                case '"':
                    inQuotes = true;
                    break;

                case ',':
                    row.Add(field.ToString());
                    field.Clear();
                    break;

                case '\r':
                    // swallow — the \n that follows ends the row
                    break;

                case '\n':
                    row.Add(field.ToString());
                    field.Clear();
                    rows.Add(row.ToArray());
                    row = new List<string>();
                    break;

                default:
                    field.Append(c);
                    break;
            }
        }

        // Trailing field / row with no terminating newline
        if (field.Length > 0 || row.Count > 0)
        {
            row.Add(field.ToString());
            rows.Add(row.ToArray());
        }

        // Drop rows that are entirely empty (trailing blank lines)
        return rows.Where(r => r.Any(v => !string.IsNullOrWhiteSpace(v))).ToList();
    }

    public static string Escape(string? value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        var needsQuotes = value.Contains(',') || value.Contains('"')
                       || value.Contains('\n') || value.Contains('\r')
                       || value.StartsWith(' ') || value.EndsWith(' ');
        if (!needsQuotes) return value;
        return $"\"{value.Replace("\"", "\"\"")}\"";
    }

    public static string Line(IEnumerable<string?> values) =>
        string.Join(",", values.Select(Escape));

    /// <summary>UTF-8 with BOM — without it Excel mangles non-ASCII text.</summary>
    public static byte[] ToBytes(string csv) =>
        new UTF8Encoding(true).GetBytes(csv);
}

// ═══════════════════════════════════════════════════════════════════
// TRANSFER SERVICE
// ═══════════════════════════════════════════════════════════════════
public interface IDataTransferService
{
    List<TransferEntityInfo> GetEntities();

    byte[] BuildTemplate(string entityKey);

    ImportPreviewResponse Preview(string entityKey, string mode, string csvText, string? fileName);

    ImportResultResponse Commit(string entityKey, string mode, string csvText, string? fileName);

    (string FileName, byte[] Content) Export(string entityKey);

    List<string> GetExportableTables();

    (string FileName, byte[] Content) ExportTable(string tableName);
}

public class DataTransferService : IDataTransferService
{
    private readonly MyDbContext _db;
    private readonly IDataTransferJobRepository _jobs;

    public DataTransferService(MyDbContext db, IDataTransferJobRepository jobs)
    {
        _db = db;
        _jobs = jobs;
    }

    // ═══════════════════════════════════════════════════════════════
    // CATALOG
    // ═══════════════════════════════════════════════════════════════
    public List<TransferEntityInfo> GetEntities() => TransferCatalog.All;

    // ═══════════════════════════════════════════════════════════════
    // TEMPLATE — header row + one commented example row
    // ═══════════════════════════════════════════════════════════════
    public byte[] BuildTemplate(string entityKey)
    {
        var info = TransferCatalog.Find(entityKey)
            ?? throw new InvalidOperationException($"Unknown entity '{entityKey}'.");

        var sb = new StringBuilder();
        sb.AppendLine(Csv.Line(info.Columns.Select(c => c.Name)));

        // A hint row so whoever fills this in knows the rules. It starts
        // with '#' and the importer skips rows whose first cell does.
        sb.AppendLine(Csv.Line(info.Columns.Select(c =>
        {
            var bits = new List<string>();
            if (c.Required) bits.Add("required");
            if (c.Options is { Count: > 0 }) bits.Add(string.Join(" | ", c.Options));
            else bits.Add(c.Type);
            if (c.MaxLength.HasValue) bits.Add($"max {c.MaxLength}");
            if (c.Min.HasValue && c.Max.HasValue) bits.Add($"{c.Min}-{c.Max}");
            else if (c.Min.HasValue) bits.Add($">= {c.Min}");
            else if (c.Max.HasValue) bits.Add($"<= {c.Max}");
            if (!string.IsNullOrWhiteSpace(c.Hint)) bits.Add(c.Hint!);
            return "# " + string.Join(", ", bits);
        })));

        return Csv.ToBytes(sb.ToString());
    }

    // ═══════════════════════════════════════════════════════════════
    // PREVIEW — parse + validate, write nothing
    // ═══════════════════════════════════════════════════════════════
    public ImportPreviewResponse Preview(string entityKey, string mode, string csvText, string? fileName)
    {
        var info = TransferCatalog.Find(entityKey)
            ?? throw new InvalidOperationException($"Unknown entity '{entityKey}'.");

        if (!info.CanImport)
            throw new InvalidOperationException($"'{info.Label}' does not support import.");

        var result = new ImportPreviewResponse
        {
            EntityKey   = info.Key,
            EntityLabel = info.Label,
            Mode        = mode,
            FileName    = fileName,
        };

        var rows = Csv.Parse(csvText);
        if (rows.Count == 0)
        {
            result.CanCommit = false;
            return result;
        }

        // ── Header mapping ────────────────────────────────────────
        var headers = rows[0].Select(h => h.Trim()).ToArray();
        result.Headers = headers.ToList();

        var known = info.Columns.ToDictionary(c => c.Name, StringComparer.OrdinalIgnoreCase);

        result.UnknownHeaders = headers
            .Where(h => !string.IsNullOrWhiteSpace(h) && !known.ContainsKey(h))
            .ToList();

        result.MissingHeaders = info.Columns
            .Where(c => c.Required && !headers.Contains(c.Name, StringComparer.OrdinalIgnoreCase))
            .Select(c => c.Name)
            .ToList();

        // ── Existing keys, so we can label add vs update ───────────
        var keyColumn = info.Columns.FirstOrDefault(c => c.IsKey);
        var existingKeys = keyColumn != null
            ? LoadExistingKeys(info.Key)
            : new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Keys seen earlier in THIS file — catches in-file duplicates,
        // which are otherwise a very confusing partial-failure.
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        for (var r = 1; r < rows.Count; r++)
        {
            var raw = rows[r];
            if (raw.Length > 0 && raw[0].TrimStart().StartsWith('#')) continue; // hint row

            var row = new TransferRowResult { RowNumber = r };

            for (var c = 0; c < headers.Length && c < raw.Length; c++)
            {
                var header = headers[c];
                if (string.IsNullOrWhiteSpace(header) || !known.ContainsKey(header)) continue;
                row.Values[known[header].Name] = raw[c].Trim();
            }

            ValidateRow(info, row);

            // ── add / update / duplicate ───────────────────────────
            if (keyColumn != null)
            {
                row.Values.TryGetValue(keyColumn.Name, out var key);

                if (string.IsNullOrWhiteSpace(key))
                {
                    row.Action = "error";
                }
                else if (!seen.Add(key))
                {
                    row.Errors.Add($"Duplicate {keyColumn.Label} '{key}' — already appears earlier in this file.");
                    row.Action = "error";
                }
                else
                {
                    var exists = existingKeys.Contains(key);

                    if (exists && mode == "A")
                    {
                        row.Errors.Add($"{keyColumn.Label} '{key}' already exists (mode is Add only).");
                        row.Action = "error";
                    }
                    else if (!exists && mode == "U")
                    {
                        row.Errors.Add($"{keyColumn.Label} '{key}' not found (mode is Update only).");
                        row.Action = "error";
                    }
                    else
                    {
                        row.Action = exists ? "update" : "add";
                    }
                }
            }
            else
            {
                row.Action = "add";
            }

            if (!row.IsValid) row.Action = "error";
            result.Rows.Add(row);
        }

        // ── Entity-specific checks that need the whole file in hand ──
        ValidateAcrossRows(info, result.Rows);

        foreach (var row in result.Rows)
            if (!row.IsValid) row.Action = "error";

        result.TotalRows   = result.Rows.Count;
        result.ValidRows   = result.Rows.Count(x => x.IsValid);
        result.InvalidRows = result.Rows.Count(x => !x.IsValid);
        result.AddCount    = result.Rows.Count(x => x.Action == "add");
        result.UpdateCount = result.Rows.Count(x => x.Action == "update");
        result.CanCommit   = result.MissingHeaders.Count == 0
                          && result.ValidRows > 0
                          && result.InvalidRows == 0;

        return result;
    }

    // ═══════════════════════════════════════════════════════════════
    // COMMIT — validate again, then write in one transaction
    // ═══════════════════════════════════════════════════════════════
    public ImportResultResponse Commit(string entityKey, string mode, string csvText, string? fileName)
    {
        var info = TransferCatalog.Find(entityKey)
            ?? throw new InvalidOperationException($"Unknown entity '{entityKey}'.");

        if (!info.CanImport)
            throw new InvalidOperationException($"'{info.Label}' does not support import.");

        var preview = Preview(entityKey, mode, csvText, fileName);

        var job = _jobs.Start("I", info.Key, info.Label, mode, fileName,
                              Encoding.UTF8.GetByteCount(csvText));

        var errors = new List<string>();

        // Refuse a partial write: if the file has any bad row, nothing lands.
        // A half-imported master-data file is worse than a rejected one.
        if (preview.MissingHeaders.Count > 0)
        {
            errors.Add("Missing required column(s): " + string.Join(", ", preview.MissingHeaders));
            _jobs.Finish(job, "F", preview.TotalRows, 0, preview.TotalRows, 0, errors);
            return ToResult(job, errors);
        }

        if (preview.InvalidRows > 0)
        {
            foreach (var bad in preview.Rows.Where(x => !x.IsValid))
                errors.Add($"Row {bad.RowNumber}: {string.Join("; ", bad.Errors)}");

            _jobs.Finish(job, "F", preview.TotalRows, 0, preview.InvalidRows, 0, errors);
            return ToResult(job, errors);
        }

        if (preview.TotalRows == 0)
        {
            errors.Add("The file contains no data rows.");
            _jobs.Finish(job, "F", 0, 0, 0, 0, errors);
            return ToResult(job, errors);
        }

        var success = 0;
        var failed  = 0;

        using var tx = _db.Database.BeginTransaction();
        try
        {
            switch (info.Key)
            {
                case "BusinessPartner":
                    ImportBusinessPartners(preview.Rows, ref success, ref failed, errors);
                    break;

                case "ChartOfAccount":
                    ImportChartOfAccounts(preview.Rows, ref success, ref failed, errors);
                    break;

                case "ItemMaster":
                    ImportItems(preview.Rows, ref success, ref failed, errors);
                    break;

                case "CompanyDetails":
                    ImportCompanyDetails(preview.Rows, ref success, ref failed, errors);
                    break;

                case "User":
                    ImportUsers(preview.Rows, ref success, ref failed, errors);
                    break;

                case "ItemGroup":
                    ImportItemGroups(preview.Rows, ref success, ref failed, errors);
                    break;

                case "EmployeeMaster":
                    ImportEmployees(preview.Rows, ref success, ref failed, errors);
                    break;

                case "Attendance":
                    ImportAttendance(preview.Rows, ref success, ref failed, errors);
                    break;

                case "Payroll":
                    ImportPayroll(preview.Rows, ref success, ref failed, errors);
                    break;

                default:
                    throw new InvalidOperationException(
                        $"No import handler registered for '{info.Key}'.");
            }

            _db.SaveChanges();
            // Items imported without a barcode get a generated one (needs the new Ids).
            if (info.Key == "ItemMaster"
                && FarmingApi.Modules.Inventory.ItemsMaster.ItemBarcodes.FillMissing(_db) > 0)
                _db.SaveChanges();
            tx.Commit();
        }
        catch (Exception ex)
        {
            tx.Rollback();
            errors.Add("Import rolled back: " + Root(ex).Message);
            _jobs.Finish(job, "F", preview.TotalRows, 0, preview.TotalRows, 0, errors);
            return ToResult(job, errors);
        }

        var status = failed == 0 ? "S" : "W";
        _jobs.Finish(job, status, preview.TotalRows, success, failed, 0, errors);
        return ToResult(job, errors);
    }

    // ═══════════════════════════════════════════════════════════════
    // EXPORT — catalog columns, so the output re-imports cleanly
    // ═══════════════════════════════════════════════════════════════
    public (string FileName, byte[] Content) Export(string entityKey)
    {
        var info = TransferCatalog.Find(entityKey)
            ?? throw new InvalidOperationException($"Unknown entity '{entityKey}'.");

        var job = _jobs.Start("E", info.Key, info.Label, "-", null, 0);

        var sb = new StringBuilder();
        sb.AppendLine(Csv.Line(info.Columns.Select(c => c.Name)));

        var count = 0;

        switch (info.Key)
        {
            case "BusinessPartner":
                foreach (var bp in _db.Set<BPEntity>().OrderBy(x => x.Code).AsNoTracking())
                {
                    sb.AppendLine(Csv.Line(info.Columns.Select(c => BpValue(bp, c.Name))));
                    count++;
                }
                break;

            case "ChartOfAccount":
                // Ordered by level then code so the file reads parents-first
                // and can be re-imported straight back.
                foreach (var a in _db.Set<COAEntity>()
                                     .OrderBy(x => x.Level).ThenBy(x => x.AcctCode)
                                     .AsNoTracking())
                {
                    sb.AppendLine(Csv.Line(info.Columns.Select(c => CoaValue(a, c.Name))));
                    count++;
                }
                break;

            case "ItemMaster":
                foreach (var i in _db.Set<ItemEntity>()
                                     .OrderBy(x => x.ItemCode)
                                     .AsNoTracking())
                {
                    sb.AppendLine(Csv.Line(info.Columns.Select(c => ItemValue(i, c.Name))));
                    count++;
                }
                break;

            case "CompanyDetails":
                // Singleton — zero rows before first save, one row after.
                var company = _db.Set<CompanyEntity>().OrderBy(x => x.Id).AsNoTracking().FirstOrDefault();
                if (company != null)
                {
                    sb.AppendLine(Csv.Line(info.Columns.Select(c => CompanyValue(company, c.Name))));
                    count++;
                }
                break;

            case "User":
                var groupNames = _db.Set<UserGroupEntity>().AsNoTracking()
                    .ToDictionary(g => g.Id, g => g.GroupName);
                foreach (var u in _db.Set<UserEntity>().OrderBy(x => x.UserId).AsNoTracking())
                {
                    sb.AppendLine(Csv.Line(info.Columns.Select(c => UserValue(u, c.Name, groupNames))));
                    count++;
                }
                break;

            case "ItemGroup":
                foreach (var g in _db.Set<ItemGroupEntity>().OrderBy(x => x.Code).AsNoTracking())
                {
                    sb.AppendLine(Csv.Line(info.Columns.Select(c => ItemGroupValue(g, c.Name))));
                    count++;
                }
                break;

            case "EmployeeMaster":
                foreach (var e in _db.Set<EmployeeEntity>().OrderBy(x => x.Code).AsNoTracking())
                {
                    sb.AppendLine(Csv.Line(info.Columns.Select(c => EmployeeValue(e, c.Name))));
                    count++;
                }
                break;

            case "Attendance":
                foreach (var a in _db.Set<AttendanceEntity>()
                                     .OrderByDescending(x => x.Date).ThenBy(x => x.EmployeeCode)
                                     .AsNoTracking())
                {
                    sb.AppendLine(Csv.Line(info.Columns.Select(c => AttendanceValue(a, c.Name))));
                    count++;
                }
                break;

            case "Payroll":
                foreach (var p in _db.Set<PayrollEntity>()
                                     .OrderByDescending(x => x.PeriodYear).ThenByDescending(x => x.PeriodMonth)
                                     .ThenBy(x => x.EmployeeCode)
                                     .AsNoTracking())
                {
                    sb.AppendLine(Csv.Line(info.Columns.Select(c => PayrollValue(p, c.Name))));
                    count++;
                }
                break;

            default:
                // Fall back to the generic dump for entities with no
                // hand-written exporter.
                _jobs.Finish(job, "F", 0, 0, 0, 0,
                    new[] { $"No export handler registered for '{info.Key}'." });
                throw new InvalidOperationException($"No export handler registered for '{info.Key}'.");
        }

        var bytes = Csv.ToBytes(sb.ToString());
        var name  = $"{info.Key}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

        job.FileName = name;
        job.FileSize = bytes.LongLength;
        _jobs.Finish(job, "S", count, count, 0, 0);

        return (name, bytes);
    }

    // ═══════════════════════════════════════════════════════════════
    // GENERIC EXPORT — any table in the EF model, all scalar columns
    // ═══════════════════════════════════════════════════════════════
    public List<string> GetExportableTables() =>
        _db.Model.GetEntityTypes()
           .Select(e => e.GetTableName())
           .Where(t => !string.IsNullOrEmpty(t))
           .Select(t => t!)
           .Distinct()
           .OrderBy(t => t)
           .ToList();

    public (string FileName, byte[] Content) ExportTable(string tableName)
    {
        var et = _db.Model.GetEntityTypes()
            .FirstOrDefault(e => string.Equals(e.GetTableName(), tableName, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException($"Unknown table '{tableName}'.");

        var clr = et.ClrType;

        var props = et.GetProperties()
            .Where(p => p.PropertyInfo != null)
            .Select(p => p.PropertyInfo!)
            .ToList();

        var job = _jobs.Start("E", tableName, $"Table {tableName}", "-", null, 0);

        var sb = new StringBuilder();
        sb.AppendLine(Csv.Line(props.Select(p => p.Name)));

        var setMethod = typeof(DbContext).GetMethods()
            .First(m => m.Name == nameof(DbContext.Set)
                     && m.IsGenericMethodDefinition
                     && m.GetParameters().Length == 0);

        var query = (IEnumerable)setMethod.MakeGenericMethod(clr).Invoke(_db, null)!;

        var count = 0;
        foreach (var row in query)
        {
            sb.AppendLine(Csv.Line(props.Select(p => Stringify(p.GetValue(row)))));
            count++;
        }

        var bytes = Csv.ToBytes(sb.ToString());
        var name  = $"{tableName}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

        job.FileName = name;
        job.FileSize = bytes.LongLength;
        _jobs.Finish(job, "S", count, count, 0, 0);

        return (name, bytes);
    }

    // ═══════════════════════════════════════════════════════════════
    // VALIDATION — driven entirely by TransferColumn
    // ═══════════════════════════════════════════════════════════════
    private static void ValidateRow(TransferEntityInfo info, TransferRowResult row)
    {
        foreach (var col in info.Columns)
        {
            row.Values.TryGetValue(col.Name, out var raw);
            var value = raw?.Trim();

            if (string.IsNullOrWhiteSpace(value))
            {
                if (col.Required) row.Errors.Add($"{col.Label} is required.");
                continue;
            }

            if (col.MaxLength.HasValue && value.Length > col.MaxLength.Value)
                row.Errors.Add($"{col.Label} is {value.Length} characters (max {col.MaxLength}).");

            switch (col.Type)
            {
                case "int":
                    if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var n))
                        row.Errors.Add($"{col.Label} '{value}' is not a whole number.");
                    else
                        CheckRange(col, n, row);
                    break;

                case "decimal":
                    if (!decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var dec))
                        row.Errors.Add($"{col.Label} '{value}' is not a number.");
                    else
                        CheckRange(col, dec, row);
                    break;

                case "bool":
                    if (ParseBool(value) == null)
                        row.Errors.Add($"{col.Label} '{value}' is not Yes/No.");
                    break;

                case "date":
                    if (!DateTime.TryParse(value, CultureInfo.InvariantCulture,
                                           DateTimeStyles.None, out _))
                        row.Errors.Add($"{col.Label} '{value}' is not a date (use YYYY-MM-DD).");
                    break;

                case "enum":
                    if (col.Options is { Count: > 0 } &&
                        !col.Options.Contains(value, StringComparer.OrdinalIgnoreCase))
                        row.Errors.Add(
                            $"{col.Label} '{value}' is not valid — expected one of: " +
                            string.Join(", ", col.Options) + ".");
                    break;
            }
        }
    }

    private static void CheckRange(TransferColumn col, decimal value, TransferRowResult row)
    {
        if (col.Min.HasValue && col.Max.HasValue && (value < col.Min || value > col.Max))
            row.Errors.Add($"{col.Label} must be between {col.Min} and {col.Max}.");
        else if (col.Min.HasValue && value < col.Min)
            row.Errors.Add($"{col.Label} cannot be less than {col.Min}.");
        else if (col.Max.HasValue && value > col.Max)
            row.Errors.Add($"{col.Label} cannot be more than {col.Max}.");
    }

    // ═══════════════════════════════════════════════════════════════
    // ITEM MASTER IMPORTER
    // ═══════════════════════════════════════════════════════════════
    private void ImportItems(List<TransferRowResult> rows,
                             ref int success, ref int failed,
                             List<string> errors)
    {
        var codes = rows.Select(r => Get(r, "ItemCode")!).Where(c => c != null).ToList();

        var existing = _db.Set<ItemEntity>()
            .Where(i => codes.Contains(i.ItemCode))
            .ToDictionary(i => i.ItemCode, StringComparer.OrdinalIgnoreCase);

        // barcode → owning item code, so the file can't give two items the same barcode
        var barcodeOwner = _db.Set<ItemEntity>().Where(i => i.Barcode != null && i.Barcode != "")
            .Select(i => new { i.Barcode, i.ItemCode }).ToList()
            .GroupBy(i => i.Barcode).ToDictionary(g => g.Key, g => g.First().ItemCode);

        foreach (var row in rows)
        {
            var code = Get(row, "ItemCode")!;

            try
            {
                var barcode = Get(row, "Barcode")?.Trim();
                if (!string.IsNullOrEmpty(barcode) && barcodeOwner.TryGetValue(barcode, out var owner)
                    && !string.Equals(owner, code, StringComparison.OrdinalIgnoreCase))
                {
                    failed++;
                    errors.Add($"Row {row.RowNumber}: Barcode '{barcode}' is already used by item {owner}.");
                    continue;
                }
                if (!string.IsNullOrEmpty(barcode)) barcodeOwner[barcode] = code;

                var isNew = !existing.TryGetValue(code, out var it);
                if (isNew)
                {
                    it = new ItemEntity
                    {
                        ItemCode  = code,
                        CreatedAt = DateTime.UtcNow,
                        InActive  = false,
                    };
                }

                // Non-nullable string columns default to "" on the entity,
                // so only overwrite when the CSV actually carried a value.
                it!.ItemName        = Get(row, "ItemName") ?? it.ItemName;
                it.ForeignName      = Get(row, "ForeignName")   ?? it.ForeignName;
                it.ItemGroup        = Get(row, "ItemGroup")     ?? it.ItemGroup;
                it.ItemType         = Get(row, "ItemType")      ?? it.ItemType;
                it.Barcode          = Get(row, "Barcode")       ?? it.Barcode;
                it.Manufacturer     = Get(row, "Manufacturer")  ?? it.Manufacturer;
                it.UomGroup         = Get(row, "UomGroup")      ?? it.UomGroup;
                it.UomCode          = Get(row, "UomCode")       ?? it.UomCode;
                it.UomName          = Get(row, "UomName")       ?? it.UomName;
                it.ManageBy         = Get(row, "ManageBy")      ?? it.ManageBy;
                it.Warehouse        = Get(row, "Warehouse")     ?? it.Warehouse;
                it.ValuationMethod  = Get(row, "ValuationMethod") ?? it.ValuationMethod;
                it.PriceList        = Get(row, "PriceList")     ?? it.PriceList;
                it.SaleTaxGroup     = Get(row, "SaleTaxGroup")  ?? it.SaleTaxGroup;
                it.SaleUom          = Get(row, "SaleUom")       ?? it.SaleUom;
                it.SaleDescription  = Get(row, "SaleDescription") ?? it.SaleDescription;
                it.PreferredVendor  = Get(row, "PreferredVendor") ?? it.PreferredVendor;
                it.VendorItemNo     = Get(row, "VendorItemNo")  ?? it.VendorItemNo;
                it.PurchaseUom      = Get(row, "PurchaseUom")   ?? it.PurchaseUom;
                it.PurchaseTaxGroup = Get(row, "PurchaseTaxGroup") ?? it.PurchaseTaxGroup;
                it.CountryOfOrigin  = Get(row, "CountryOfOrigin") ?? it.CountryOfOrigin;
                it.ShippingType     = Get(row, "ShippingType")  ?? it.ShippingType;
                it.Remarks          = Get(row, "Remarks")       ?? it.Remarks;

                if (Get(row, "Status") is { } st)
                    it.Status = st.Equals("InActive", StringComparison.OrdinalIgnoreCase)
                        ? ItemStatus.InActive : ItemStatus.Active;

                if (Bool(row, "SalesItem")    is { } si) it.SalesItem    = si;
                if (Bool(row, "PurchaseItem") is { } pi) it.PurchaseItem = pi;
                if (Bool(row, "StockItem")    is { } sk) it.StockItem    = sk;

                if (Dec(row, "MinStock")      is { } mn) it.MinStock      = mn;
                if (Dec(row, "MaxStock")      is { } mx) it.MaxStock      = mx;
                if (Dec(row, "SalePrice")     is { } sp) it.SalePrice     = sp;
                if (Dec(row, "SaleDiscount")  is { } sd) it.SaleDiscount  = sd;
                if (Dec(row, "PurchasePrice") is { } pp) it.PurchasePrice = pp;

                if (Get(row, "LeadTime") is { } lt && int.TryParse(lt, out var lead))
                    it.LeadTime = lead;

                if (isNew)
                {
                    _db.Set<ItemEntity>().Add(it);
                    existing[code] = it;
                }
                else
                {
                    it.UpdatedAt = DateTime.UtcNow;
                }

                success++;
            }
            catch (Exception ex)
            {
                failed++;
                errors.Add($"Row {row.RowNumber} ({code}): {Root(ex).Message}");
            }
        }
    }

    private static string ItemValue(ItemEntity i, string column) => column switch
    {
        "ItemCode"        => i.ItemCode,
        "ItemName"        => i.ItemName,
        "ForeignName"     => i.ForeignName ?? "",
        "ItemGroup"       => i.ItemGroup ?? "",
        "ItemType"        => i.ItemType ?? "",
        "Status"          => i.Status == ItemStatus.InActive ? "InActive" : "Active",
        "Barcode"         => i.Barcode ?? "",
        "Manufacturer"    => i.Manufacturer ?? "",
        "SalesItem"       => i.SalesItem ? "Yes" : "No",
        "PurchaseItem"    => i.PurchaseItem ? "Yes" : "No",
        "StockItem"       => i.StockItem ? "Yes" : "No",
        "UomGroup"        => i.UomGroup ?? "",
        "UomCode"         => i.UomCode ?? "",
        "UomName"         => i.UomName ?? "",
        "ManageBy"        => i.ManageBy ?? "",
        "Warehouse"       => i.Warehouse ?? "",
        "MinStock"        => i.MinStock.ToString(CultureInfo.InvariantCulture),
        "MaxStock"        => i.MaxStock.ToString(CultureInfo.InvariantCulture),
        "ValuationMethod" => i.ValuationMethod ?? "",
        "SalePrice"       => i.SalePrice.ToString(CultureInfo.InvariantCulture),
        "PriceList"       => i.PriceList ?? "",
        "SaleTaxGroup"    => i.SaleTaxGroup ?? "",
        "SaleDiscount"    => i.SaleDiscount.ToString(CultureInfo.InvariantCulture),
        "SaleUom"         => i.SaleUom ?? "",
        "SaleDescription" => i.SaleDescription ?? "",
        "PurchasePrice"   => i.PurchasePrice.ToString(CultureInfo.InvariantCulture),
        "PreferredVendor" => i.PreferredVendor ?? "",
        "VendorItemNo"    => i.VendorItemNo ?? "",
        "PurchaseUom"     => i.PurchaseUom ?? "",
        "LeadTime"        => i.LeadTime.ToString(CultureInfo.InvariantCulture),
        "PurchaseTaxGroup"=> i.PurchaseTaxGroup ?? "",
        "CountryOfOrigin" => i.CountryOfOrigin ?? "",
        "ShippingType"    => i.ShippingType ?? "",
        "Remarks"         => i.Remarks ?? "",
        _                 => "",
    };

    // ═══════════════════════════════════════════════════════════════
    // COMPANY DETAILS IMPORTER  (singleton — upsert the one record)
    // Column names match Company entity properties 1:1, so the apply
    // is driven straight off the catalog.
    // ═══════════════════════════════════════════════════════════════
    private void ImportCompanyDetails(List<TransferRowResult> rows,
                                      ref int success, ref int failed,
                                      List<string> errors)
    {
        var row = rows.FirstOrDefault();
        if (row == null) { errors.Add("The file has no data row."); return; }

        try
        {
            var c = _db.Set<CompanyEntity>().OrderBy(x => x.Id).FirstOrDefault();
            var isNew = c == null;
            if (isNew)
                c = new CompanyEntity { Name = "", CreatedAt = DateTime.UtcNow, InActive = false };

            var info = TransferCatalog.Find("CompanyDetails")!;
            foreach (var col in info.Columns)
            {
                var raw = Get(row, col.Name);
                if (raw == null) continue;                       // blank cell — leave the field as it was

                var prop = typeof(CompanyEntity).GetProperty(col.Name);
                if (prop == null || !prop.CanWrite) continue;

                switch (col.Type)
                {
                    case "bool":
                        if (ParseBool(raw) is { } b) prop.SetValue(c, b);
                        break;
                    case "decimal":
                        if (decimal.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out var d))
                            prop.SetValue(c, d);
                        break;
                    default:
                        prop.SetValue(c, raw);
                        break;
                }
            }

            // Keep the legacy summary Name in step with the local company name.
            if (!string.IsNullOrWhiteSpace(c!.LocalCompanyName))
                c.Name = c.LocalCompanyName!;

            if (isNew) _db.Set<CompanyEntity>().Add(c);
            else       c.UpdatedAt = DateTime.UtcNow;

            success++;
        }
        catch (Exception ex)
        {
            failed++;
            errors.Add($"Row {row.RowNumber}: {Root(ex).Message}");
        }
    }

    private static string CompanyValue(CompanyEntity c, string column)
    {
        var prop = typeof(CompanyEntity).GetProperty(column);
        var v = prop?.GetValue(c);
        return v switch
        {
            null      => "",
            bool b    => b ? "Yes" : "No",
            decimal d => d.ToString(CultureInfo.InvariantCulture),
            _         => v.ToString() ?? "",
        };
    }

    // ═══════════════════════════════════════════════════════════════
    // USER IMPORTER
    // Matched on UserId. Password comes in plain text and is stored as
    // the same SHA-256/Base64 hash the User controller writes.
    // ═══════════════════════════════════════════════════════════════
    private void ImportUsers(List<TransferRowResult> rows,
                             ref int success, ref int failed,
                             List<string> errors)
    {
        var ids = rows.Select(r => Get(r, "UserId")!).Where(c => c != null).ToList();

        var existing = _db.Set<UserEntity>()
            .Where(u => ids.Contains(u.UserId))
            .ToDictionary(u => u.UserId, StringComparer.OrdinalIgnoreCase);

        // Group name → id. Names are not enforced unique, so first one wins.
        var groups = _db.Set<UserGroupEntity>()
            .OrderBy(g => g.Id)
            .ToList()
            .GroupBy(g => g.GroupName, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First().Id, StringComparer.OrdinalIgnoreCase);

        foreach (var row in rows)
        {
            var uid = Get(row, "UserId")!;

            try
            {
                var isNew = !existing.TryGetValue(uid, out var u);
                if (isNew)
                {
                    u = new UserEntity
                    {
                        UserId       = uid,
                        PasswordHash = "",
                        CreatedAt    = DateTime.UtcNow,
                        InActive     = false,
                    };
                }

                u!.UserName = Get(row, "UserName") ?? u.UserName;
                u.Email     = Get(row, "Email")    ?? u.Email;
                u.Phone     = Get(row, "Phone")    ?? u.Phone;

                if (Get(row, "UserGroup") is { } gname && groups.TryGetValue(gname, out var gid))
                    u.UserGroupId = gid;

                if (Get(row, "ActiveStatus") is { Length: > 0 } st)
                    u.ActiveStatus = char.ToUpperInvariant(st[0]);

                if (Bool(row, "IsLocked") is { } locked) u.IsLocked = locked;

                if (Get(row, "Password") is { } pwd)
                    u.PasswordHash = HashPassword(pwd);

                if (isNew)
                {
                    _db.Set<UserEntity>().Add(u);
                    existing[uid] = u;
                }
                else
                {
                    u.UpdatedAt = DateTime.UtcNow;
                }

                success++;
            }
            catch (Exception ex)
            {
                failed++;
                errors.Add($"Row {row.RowNumber} ({uid}): {Root(ex).Message}");
            }
        }
    }

    private static string UserValue(UserEntity u, string column, Dictionary<int, string> groupNames) => column switch
    {
        "UserId"       => u.UserId,
        "UserName"     => u.UserName,
        "Email"        => u.Email ?? "",
        "Password"     => "",                       // hashes are one-way — never exported
        "UserGroup"    => groupNames.TryGetValue(u.UserGroupId, out var n) ? n : u.UserGroupId.ToString(),
        "Phone"        => u.Phone ?? "",
        "ActiveStatus" => u.ActiveStatus.ToString(),
        "IsLocked"     => u.IsLocked ? "Yes" : "No",
        _              => "",
    };

    /// <summary>Same scheme as UserController — SHA-256 of the UTF-8 bytes, Base64.</summary>
    private static string HashPassword(string password)
    {
        using var sha = SHA256.Create();
        return Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(password)));
    }

    private static bool StrongPassword(string p) =>
        !string.IsNullOrWhiteSpace(p) && p.Length >= 8
        && p.Any(char.IsUpper) && p.Any(char.IsLower)
        && p.Any(char.IsDigit) && p.Any(c => "@$!%*?&".Contains(c));

    // ═══════════════════════════════════════════════════════════════
    // ITEM GROUP IMPORTER
    // Matched on Code. Codes are stored upper-case, matching the
    // Item Group screen.
    // ═══════════════════════════════════════════════════════════════
    private void ImportItemGroups(List<TransferRowResult> rows,
                                  ref int success, ref int failed,
                                  List<string> errors)
    {
        var codes = rows.Select(r => Get(r, "Code")?.ToUpperInvariant()!)
                        .Where(c => c != null).ToList();

        var existing = _db.Set<ItemGroupEntity>()
            .Where(g => codes.Contains(g.Code))
            .ToDictionary(g => g.Code, StringComparer.OrdinalIgnoreCase);

        foreach (var row in rows)
        {
            var code = Get(row, "Code")!.ToUpperInvariant();

            try
            {
                var isNew = !existing.TryGetValue(code, out var g);
                if (isNew)
                {
                    g = new ItemGroupEntity
                    {
                        Code      = code,
                        Name      = "",
                        CreatedAt = DateTime.UtcNow,
                        InActive  = false,
                    };
                }

                g!.Name        = Get(row, "Name")        ?? g.Name;
                g.Description   = Get(row, "Description") ?? g.Description;
                g.DefaultUom    = Get(row, "DefaultUom") ?? g.DefaultUom;
                g.Remarks       = Get(row, "Remarks")    ?? g.Remarks;

                if (Bool(row, "IsActive") is { } active) g.IsActive = active;

                if (isNew)
                {
                    _db.Set<ItemGroupEntity>().Add(g);
                    existing[code] = g;
                }
                else
                {
                    g.UpdatedAt = DateTime.UtcNow;
                }

                success++;
            }
            catch (Exception ex)
            {
                failed++;
                errors.Add($"Row {row.RowNumber} ({code}): {Root(ex).Message}");
            }
        }
    }

    private static string ItemGroupValue(ItemGroupEntity g, string column) => column switch
    {
        "Code"        => g.Code,
        "Name"        => g.Name,
        "Description" => g.Description ?? "",
        "DefaultUom"  => g.DefaultUom ?? "",
        "IsActive"    => g.IsActive ? "Yes" : "No",
        "Remarks"     => g.Remarks ?? "",
        _             => "",
    };

    // ═══════════════════════════════════════════════════════════════
    // EMPLOYEE MASTER IMPORTER
    // Matched on Code (stored upper-case, as the Employee Master screen).
    // ═══════════════════════════════════════════════════════════════
    private void ImportEmployees(List<TransferRowResult> rows,
                                 ref int success, ref int failed,
                                 List<string> errors)
    {
        var codes = rows.Select(r => Get(r, "Code")?.ToUpperInvariant()!)
                        .Where(c => c != null).ToList();

        var existing = _db.Set<EmployeeEntity>()
            .Where(e => codes.Contains(e.Code))
            .ToDictionary(e => e.Code, StringComparer.OrdinalIgnoreCase);

        foreach (var row in rows)
        {
            var code = Get(row, "Code")!.ToUpperInvariant();

            try
            {
                var isNew = !existing.TryGetValue(code, out var e);
                if (isNew)
                {
                    e = new EmployeeEntity
                    {
                        Code      = code,
                        FirstName = "",
                        LastName  = "",
                        CreatedAt = DateTime.UtcNow,
                        InActive  = false,
                    };
                }

                e!.FirstName    = Get(row, "FirstName")    ?? e.FirstName;
                e.LastName      = Get(row, "LastName")     ?? e.LastName;
                e.EmployeeType  = Get(row, "EmployeeType") ?? e.EmployeeType;
                e.JobTitle      = Get(row, "JobTitle")     ?? e.JobTitle;
                e.Department    = Get(row, "Department")   ?? e.Department;
                e.Branch        = Get(row, "Branch")       ?? e.Branch;
                e.Manager       = Get(row, "Manager")      ?? e.Manager;
                e.Gender        = Get(row, "Gender")       ?? e.Gender;
                e.Nationality   = Get(row, "Nationality")  ?? e.Nationality;
                e.MaritalStatus = Get(row, "MaritalStatus") ?? e.MaritalStatus;
                e.NationalId    = Get(row, "NationalId")   ?? e.NationalId;
                e.Mobile        = Get(row, "Mobile")       ?? e.Mobile;
                e.Phone         = Get(row, "Phone")        ?? e.Phone;
                e.Email         = Get(row, "Email")        ?? e.Email;
                e.Address       = Get(row, "Address")      ?? e.Address;
                e.BankName      = Get(row, "BankName")     ?? e.BankName;
                e.BankAccountNo = Get(row, "BankAccountNo") ?? e.BankAccountNo;
                e.UserCode      = Get(row, "UserCode")     ?? e.UserCode;
                e.ExternalId    = Get(row, "ExternalId")   ?? e.ExternalId;
                e.Remarks       = Get(row, "Remarks")      ?? e.Remarks;

                if (DateVal(row, "DateOfBirth")     is { } dob) e.DateOfBirth     = dob;
                if (DateVal(row, "HireDate")        is { } hd)  e.HireDate        = hd;
                if (DateVal(row, "TerminationDate") is { } td)  e.TerminationDate = td;
                if (Dec(row, "Salary")             is { } sal) e.Salary          = sal;
                if (IntVal(row, "Dependents")      is { } dep) e.Dependents      = dep;
                if (Bool(row, "IsActive")          is { } act) e.IsActive        = act;

                if (isNew)
                {
                    _db.Set<EmployeeEntity>().Add(e);
                    existing[code] = e;
                }
                else
                {
                    e.UpdatedAt = DateTime.UtcNow;
                }

                success++;
            }
            catch (Exception ex)
            {
                failed++;
                errors.Add($"Row {row.RowNumber} ({code}): {Root(ex).Message}");
            }
        }
    }

    private static string EmployeeValue(EmployeeEntity e, string column) => column switch
    {
        "Code"            => e.Code,
        "FirstName"       => e.FirstName,
        "LastName"        => e.LastName,
        "EmployeeType"    => e.EmployeeType,
        "JobTitle"        => e.JobTitle ?? "",
        "Department"      => e.Department ?? "",
        "Branch"          => e.Branch ?? "",
        "Manager"         => e.Manager ?? "",
        "Gender"          => e.Gender ?? "",
        "DateOfBirth"     => e.DateOfBirth?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "",
        "Nationality"     => e.Nationality ?? "",
        "MaritalStatus"   => e.MaritalStatus ?? "",
        "NationalId"      => e.NationalId ?? "",
        "Dependents"      => e.Dependents.ToString(CultureInfo.InvariantCulture),
        "Mobile"          => e.Mobile ?? "",
        "Phone"           => e.Phone ?? "",
        "Email"           => e.Email ?? "",
        "Address"         => e.Address ?? "",
        "HireDate"        => e.HireDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "",
        "TerminationDate" => e.TerminationDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "",
        "Salary"          => e.Salary?.ToString(CultureInfo.InvariantCulture) ?? "",
        "BankName"        => e.BankName ?? "",
        "BankAccountNo"   => e.BankAccountNo ?? "",
        "UserCode"        => e.UserCode ?? "",
        "ExternalId"      => e.ExternalId ?? "",
        "IsActive"        => e.IsActive ? "Yes" : "No",
        "Remarks"         => e.Remarks ?? "",
        _                 => "",
    };

    private static DateTime? DateVal(TransferRowResult row, string column)
    {
        var raw = Get(row, column);
        if (raw == null) return null;
        return DateTime.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.None, out var d)
            ? d : null;
    }

    private static int? IntVal(TransferRowResult row, string column)
    {
        var raw = Get(row, column);
        if (raw == null) return null;
        return int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var n)
            ? n : null;
    }

    // ═══════════════════════════════════════════════════════════════
    // ATTENDANCE IMPORTER  (upsert on EmployeeCode + Date)
    // ═══════════════════════════════════════════════════════════════
    private void ImportAttendance(List<TransferRowResult> rows,
                                  ref int success, ref int failed,
                                  List<string> errors)
    {
        var codes = rows.Select(r => Get(r, "EmployeeCode")?.ToUpperInvariant()!)
                        .Where(c => c != null).Distinct().ToList();

        var existing = _db.Set<AttendanceEntity>()
            .Where(a => codes.Contains(a.EmployeeCode))
            .ToList();

        foreach (var row in rows)
        {
            var code = Get(row, "EmployeeCode")!.ToUpperInvariant();
            var date = DateVal(row, "Date")!.Value.Date;

            try
            {
                var a = existing.FirstOrDefault(x =>
                    string.Equals(x.EmployeeCode, code, StringComparison.OrdinalIgnoreCase)
                    && x.Date.Date == date);
                var isNew = a == null;
                if (isNew)
                {
                    a = new AttendanceEntity
                    {
                        EmployeeCode = code,
                        Date         = date,
                        CreatedAt    = DateTime.UtcNow,
                        InActive     = false,
                    };
                }

                a!.EmployeeName = Get(row, "EmployeeName") ?? a.EmployeeName;
                a.Status        = Get(row, "Status")       ?? a.Status;
                a.CheckIn       = Get(row, "CheckIn")      ?? a.CheckIn;
                a.CheckOut      = Get(row, "CheckOut")     ?? a.CheckOut;
                a.LeaveType     = Get(row, "LeaveType")    ?? a.LeaveType;
                a.Remarks       = Get(row, "Remarks")      ?? a.Remarks;

                if (Dec(row, "HoursWorked")   is { } hw) a.HoursWorked   = hw;
                if (Dec(row, "OvertimeHours") is { } oh) a.OvertimeHours = oh;

                if (isNew)
                {
                    _db.Set<AttendanceEntity>().Add(a);
                    existing.Add(a);
                }
                else
                {
                    a.UpdatedAt = DateTime.UtcNow;
                }

                success++;
            }
            catch (Exception ex)
            {
                failed++;
                errors.Add($"Row {row.RowNumber} ({code} {date:yyyy-MM-dd}): {Root(ex).Message}");
            }
        }
    }

    private static string AttendanceValue(AttendanceEntity a, string column) => column switch
    {
        "EmployeeCode"  => a.EmployeeCode,
        "EmployeeName"  => a.EmployeeName ?? "",
        "Date"          => a.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
        "Status"        => a.Status,
        "CheckIn"       => a.CheckIn ?? "",
        "CheckOut"      => a.CheckOut ?? "",
        "HoursWorked"   => a.HoursWorked?.ToString(CultureInfo.InvariantCulture) ?? "",
        "OvertimeHours" => a.OvertimeHours?.ToString(CultureInfo.InvariantCulture) ?? "",
        "LeaveType"     => a.LeaveType ?? "",
        "Remarks"       => a.Remarks ?? "",
        _               => "",
    };

    // ═══════════════════════════════════════════════════════════════
    // PAYROLL IMPORTER  (upsert on EmployeeCode + PeriodYear + PeriodMonth)
    // ═══════════════════════════════════════════════════════════════
    private void ImportPayroll(List<TransferRowResult> rows,
                               ref int success, ref int failed,
                               List<string> errors)
    {
        var codes = rows.Select(r => Get(r, "EmployeeCode")?.ToUpperInvariant()!)
                        .Where(c => c != null).Distinct().ToList();

        var existing = _db.Set<PayrollEntity>()
            .Where(p => codes.Contains(p.EmployeeCode))
            .ToList();

        foreach (var row in rows)
        {
            var code  = Get(row, "EmployeeCode")!.ToUpperInvariant();
            var year  = IntVal(row, "PeriodYear")!.Value;
            var month = IntVal(row, "PeriodMonth")!.Value;

            try
            {
                var p = existing.FirstOrDefault(x =>
                    string.Equals(x.EmployeeCode, code, StringComparison.OrdinalIgnoreCase)
                    && x.PeriodYear == year && x.PeriodMonth == month);
                var isNew = p == null;
                if (isNew)
                {
                    p = new PayrollEntity
                    {
                        EmployeeCode = code,
                        PeriodYear   = year,
                        PeriodMonth  = month,
                        CreatedAt    = DateTime.UtcNow,
                        InActive     = false,
                    };
                }

                p!.EmployeeName  = Get(row, "EmployeeName")  ?? p.EmployeeName;
                p.PaymentMethod  = Get(row, "PaymentMethod") ?? p.PaymentMethod;
                p.Status         = Get(row, "Status")        ?? p.Status;
                p.Remarks        = Get(row, "Remarks")       ?? p.Remarks;

                if (Dec(row, "BasicSalary")      is { } bs) p.BasicSalary      = bs;
                if (Dec(row, "TaxableAllowance") is { } ta) p.TaxableAllowance = ta;
                if (Dec(row, "ExemptAllowance")  is { } ea) p.ExemptAllowance  = ea;
                if (Dec(row, "Bonus")           is { } bo) p.Bonus           = bo;
                if (Dec(row, "OtherDeductions") is { } od) p.OtherDeductions = od;
                if (DateVal(row, "PaymentDate") is { } pd) p.PaymentDate     = pd;

                var rowRate = Dec(row, "ExchangeRate");
                p.ExchangeRate = rowRate is > 0 ? rowRate!.Value
                               : p.ExchangeRate > 0 ? p.ExchangeRate
                               : LatestUsdRate();

                RecalculatePayroll(p);

                if (isNew)
                {
                    _db.Set<PayrollEntity>().Add(p);
                    existing.Add(p);
                }
                else
                {
                    p.UpdatedAt = DateTime.UtcNow;
                }

                success++;
            }
            catch (Exception ex)
            {
                failed++;
                errors.Add($"Row {row.RowNumber} ({code} {year}-{month}): {Root(ex).Message}");
            }
        }
    }

    private static string PayrollValue(PayrollEntity p, string column) => column switch
    {
        "EmployeeCode"     => p.EmployeeCode,
        "EmployeeName"     => p.EmployeeName ?? "",
        "PeriodYear"       => p.PeriodYear.ToString(CultureInfo.InvariantCulture),
        "PeriodMonth"      => p.PeriodMonth.ToString(CultureInfo.InvariantCulture),
        "BasicSalary"      => p.BasicSalary.ToString(CultureInfo.InvariantCulture),
        "TaxableAllowance" => p.TaxableAllowance?.ToString(CultureInfo.InvariantCulture) ?? "",
        "ExemptAllowance"  => p.ExemptAllowance?.ToString(CultureInfo.InvariantCulture) ?? "",
        "Bonus"            => p.Bonus?.ToString(CultureInfo.InvariantCulture) ?? "",
        "OtherDeductions"  => p.OtherDeductions?.ToString(CultureInfo.InvariantCulture) ?? "",
        "ExchangeRate"     => p.ExchangeRate.ToString(CultureInfo.InvariantCulture),
        "PaymentDate"      => p.PaymentDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "",
        "PaymentMethod"    => p.PaymentMethod ?? "",
        "Status"           => p.Status,
        "Remarks"          => p.Remarks ?? "",
        _                  => "",
    };

    // Latest USD→KHR rate, or the 4,100 fallback.
    private decimal LatestUsdRate()
    {
        var r = _db.Set<ExchangeRateEntity>()
            .Where(x => x.Currency == "USD")
            .OrderByDescending(x => x.EffectiveDate)
            .FirstOrDefault();
        return r is { Rate: > 0 } ? r.Rate : 4100m;
    }

    // Same Cambodian statutory calc the PayrollController runs, applied
    // during import so bulk-loaded payslips carry TOS / NSSF / OT too.
    private void RecalculatePayroll(PayrollEntity p)
    {
        var rate = p.ExchangeRate > 0 ? p.ExchangeRate : LatestUsdRate();
        p.ExchangeRate = rate;

        var emp = _db.Set<EmployeeEntity>()
            .FirstOrDefault(e => e.Code == p.EmployeeCode);
        p.Dependents = emp?.Dependents ?? 0;

        var attRows = _db.Set<AttendanceEntity>()
            .Where(a => a.EmployeeCode == p.EmployeeCode).ToList();
        var att = HrAttendanceMath.Summarize(attRows, p.EmployeeCode, p.PeriodYear, p.PeriodMonth);

        var dailyRate  = p.BasicSalary / HrPayrollMath.StandardDaysPerMonth;
        var hourlyRate = dailyRate / HrPayrollMath.StandardHoursPerDay;

        p.OvertimePay          = Money(att.TotalOvertimeHours * hourlyRate * HrPayrollMath.OvertimeMultiplier);
        p.UnpaidLeaveDeduction = Money(att.UnpaidDays * dailyRate);

        var taxableUsd = p.BasicSalary + (p.TaxableAllowance ?? 0m)
                       + (p.OvertimePay ?? 0m) + (p.Bonus ?? 0m);
        var taxableKhr = taxableUsd * rate - p.Dependents * HrPayrollMath.DependentReliefKhr;
        if (taxableKhr < 0m) taxableKhr = 0m;
        p.TaxOnSalary = Money(HrPayrollMath.TosKhr(taxableKhr) / rate);

        var baseKhr = Math.Clamp(p.BasicSalary * rate,
            HrPayrollMath.NssfBaseMinKhr, HrPayrollMath.NssfBaseMaxKhr);
        p.NssfEmployee = Money(baseKhr * HrPayrollMath.NssfEmployeeRate / rate);
        p.NssfEmployer = Money(baseKhr * HrPayrollMath.NssfEmployerRate / rate);
    }

    private static decimal Money(decimal v) => Math.Round(v, 2, MidpointRounding.AwayFromZero);

    // ═══════════════════════════════════════════════════════════════
    // CROSS-ROW VALIDATION
    // Rules that cannot be judged one row at a time.
    // ═══════════════════════════════════════════════════════════════
    private void ValidateAcrossRows(TransferEntityInfo info, List<TransferRowResult> rows)
    {
        // Company Details is a singleton — keep the first data row, reject the rest.
        if (info.Key == "CompanyDetails")
        {
            foreach (var extra in rows.Skip(1))
                extra.Errors.Add(
                    "Company Details is a single record — the file may contain only one data row.");
            return;
        }

        // Users — password rules, the group must exist, e-mail must be unique.
        if (info.Key == "User")
        {
            var groupNames = _db.Set<UserGroupEntity>()
                .Select(g => g.GroupName)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var emailsInDb = _db.Set<UserEntity>()
                .Select(u => new { u.Email, u.UserId })
                .ToList();

            var seenEmail = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            foreach (var row in rows)
            {
                var pwd = Get(row, "Password");

                if (row.Action == "add" && pwd == null)
                    row.Errors.Add("Password is required for a new user.");

                if (pwd != null && !StrongPassword(pwd))
                    row.Errors.Add(
                        "Password must be at least 8 characters with an uppercase letter, a lowercase " +
                        "letter, a digit and a special character (@$!%*?&).");

                var group = Get(row, "UserGroup");
                if (group != null && !groupNames.Contains(group))
                    row.Errors.Add($"User group '{group}' does not exist — create it first.");

                var email = Get(row, "Email");
                var uid   = Get(row, "UserId");
                if (email != null)
                {
                    if (seenEmail.TryGetValue(email, out var firstRow))
                        row.Errors.Add($"E-mail '{email}' also appears on row {firstRow}.");
                    else
                        seenEmail[email] = row.RowNumber;

                    var clash = emailsInDb.FirstOrDefault(e =>
                        string.Equals(e.Email, email, StringComparison.OrdinalIgnoreCase));
                    if (clash != null &&
                        !string.Equals(clash.UserId, uid, StringComparison.OrdinalIgnoreCase))
                        row.Errors.Add($"E-mail '{email}' is already used by another user.");
                }
            }
            return;
        }

        // Attendance — one row per employee per day; flag in-file duplicates.
        if (info.Key == "Attendance")
        {
            var seen = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var row in rows)
            {
                var code = Get(row, "EmployeeCode")?.ToUpperInvariant();
                var date = Get(row, "Date");
                if (code == null || date == null) continue;
                var key = $"{code}|{date}";
                if (seen.TryGetValue(key, out var first))
                    row.Errors.Add($"'{code}' on {date} also appears on row {first}.");
                else
                    seen[key] = row.RowNumber;
            }
            return;
        }

        // Payroll — one row per employee per period; flag in-file duplicates.
        if (info.Key == "Payroll")
        {
            var seen = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var row in rows)
            {
                var code = Get(row, "EmployeeCode")?.ToUpperInvariant();
                var year = Get(row, "PeriodYear");
                var month = Get(row, "PeriodMonth");
                if (code == null || year == null || month == null) continue;
                var key = $"{code}|{year}|{month}";
                if (seen.TryGetValue(key, out var first))
                    row.Errors.Add($"'{code}' for {year}-{month} also appears on row {first}.");
                else
                    seen[key] = row.RowNumber;
            }
            return;
        }

        if (info.Key != "ChartOfAccount") return;

        // A parent may live in the database already, or appear later in
        // this same file — both are legitimate, so build one combined view.
        var inFile = rows
            .Where(r => Get(r, "AcctCode") != null)
            .GroupBy(r => Get(r, "AcctCode")!, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        var inDb = _db.Set<COAEntity>()
            .Select(a => new { a.AcctCode, a.IsGroup })
            .ToDictionary(a => a.AcctCode, a => a.IsGroup, StringComparer.OrdinalIgnoreCase);

        foreach (var row in rows)
        {
            var code   = Get(row, "AcctCode");
            var father = Get(row, "FatherNum");
            var levelS = Get(row, "Level");

            // "null" as a literal is a common export artefact — treat it as blank.
            if (father != null && father.Equals("null", StringComparison.OrdinalIgnoreCase))
                father = null;

            if (!int.TryParse(levelS, out var level)) continue; // already flagged by ValidateRow

            if (level < 1)
            {
                row.Errors.Add("Level must be 1 or greater — level 1 is the root.");
                continue;
            }

            if (level == 1)
            {
                if (father != null)
                    row.Errors.Add($"Level 1 is a root account and cannot have a parent ('{father}').");
                continue;
            }

            // ── level > 1 ─────────────────────────────────────────
            if (father == null)
            {
                row.Errors.Add($"Level {level} needs a Parent Code.");
                continue;
            }

            if (code != null && father.Equals(code, StringComparison.OrdinalIgnoreCase))
            {
                row.Errors.Add($"Account '{code}' cannot be its own parent.");
                continue;
            }

            // Does the parent exist at all?
            var parentInFile = inFile.TryGetValue(father, out var pRow);
            var parentInDb   = inDb.TryGetValue(father, out var dbIsGroup);

            if (!parentInFile && !parentInDb)
            {
                row.Errors.Add(
                    $"Parent '{father}' does not exist — add it to this file or create it first.");
                continue;
            }

            // Is the parent a title/group account?
            var parentIsGroup = parentInFile
                ? ParseBool(Get(pRow!, "IsGroup") ?? "No") ?? false
                : dbIsGroup;

            if (!parentIsGroup)
                row.Errors.Add($"Parent '{father}' must be a Title account (IsGroup = Yes).");

            // Does the level follow the parent's?
            if (parentInFile && int.TryParse(Get(pRow!, "Level"), out var parentLevel)
                && level != parentLevel + 1)
                row.Errors.Add(
                    $"Level {level} does not follow parent '{father}' at level {parentLevel} " +
                    $"— expected {parentLevel + 1}.");
        }

        // ── Cycles (A → B → A) — these would hang the ordering pass ──
        foreach (var row in rows)
        {
            var code = Get(row, "AcctCode");
            if (code == null) continue;

            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { code };
            var cur  = Get(row, "FatherNum");

            while (cur != null && inFile.TryGetValue(cur, out var next))
            {
                if (!seen.Add(cur))
                {
                    row.Errors.Add($"Parent chain for '{code}' loops back on itself via '{cur}'.");
                    break;
                }
                cur = Get(next, "FatherNum");
            }
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // CHART OF ACCOUNTS IMPORTER
    // Rows are written parents-first so the self-referencing FK holds.
    // ═══════════════════════════════════════════════════════════════
    private void ImportChartOfAccounts(List<TransferRowResult> rows,
                                       ref int success, ref int failed,
                                       List<string> errors)
    {
        var codes = rows.Select(r => Get(r, "AcctCode")!).Where(c => c != null).ToList();

        var existing = _db.Set<COAEntity>()
            .Where(a => codes.Contains(a.AcctCode))
            .ToDictionary(a => a.AcctCode, StringComparer.OrdinalIgnoreCase);

        foreach (var row in Order(rows))
        {
            var code = Get(row, "AcctCode")!;

            try
            {
                var isNew = !existing.TryGetValue(code, out var a);
                if (isNew)
                {
                    a = new COAEntity
                    {
                        AcctCode  = code,
                        CreatedAt = DateTime.UtcNow,
                        InActive  = false,
                    };
                }

                a!.AcctName     = Get(row, "AcctName") ?? a.AcctName;
                a.FrgnName      = Get(row, "FrgnName")     ?? a.FrgnName;
                a.AcctType      = Get(row, "AcctType")?.ToUpperInvariant() ?? a.AcctType;
                a.Currency      = Get(row, "Currency")?.ToUpperInvariant() ?? a.Currency;
                a.DfltVat       = Get(row, "DfltVat")      ?? a.DfltVat;
                a.TaxCode       = Get(row, "TaxCode")      ?? a.TaxCode;
                a.Project       = Get(row, "Project")      ?? a.Project;
                a.CostCenter    = Get(row, "CostCenter")   ?? a.CostCenter;
                a.Department    = Get(row, "Department")   ?? a.Department;
                a.ExternalCode  = Get(row, "ExternalCode") ?? a.ExternalCode;
                a.Description   = Get(row, "Description")  ?? a.Description;

                if (int.TryParse(Get(row, "Level"), out var lvl)) a.Level = lvl;

                var father = Get(row, "FatherNum");
                a.FatherNum = father != null &&
                              !father.Equals("null", StringComparison.OrdinalIgnoreCase)
                    ? father
                    : null;

                if (Bool(row, "IsGroup")            is { } g)  a.IsGroup            = g;
                if (Bool(row, "IsActive")           is { } ia) a.IsActive           = ia;
                if (Bool(row, "AllowPosting")       is { } ap) a.AllowPosting       = ap;
                if (Bool(row, "BlockManualPosting") is { } bm) a.BlockManualPosting = bm;
                if (Bool(row, "ControlAccount")     is { } ca) a.ControlAccount     = ca;
                if (Bool(row, "CashAccount")        is { } cs) a.CashAccount        = cs;
                if (Bool(row, "CashFlowRelevant")   is { } cf) a.CashFlowRelevant   = cf;
                if (Bool(row, "Confidential")       is { } cd) a.Confidential       = cd;

                if (isNew)
                {
                    _db.Set<COAEntity>().Add(a);
                    existing[code] = a;
                    // Flush now: the next row's FatherNum may point at this
                    // account, and the FK is checked on insert.
                    _db.SaveChanges();
                }
                else
                {
                    a.UpdatedAt = DateTime.UtcNow;
                }

                success++;
            }
            catch (Exception ex)
            {
                failed++;
                errors.Add($"Row {row.RowNumber} ({code}): {Root(ex).Message}");
            }
        }
    }

    /// <summary>
    /// Sort so every account comes after its parent. Rows whose parent is
    /// not in this file (already in the database, or root) go first; the
    /// rest follow as their parents land. Cycles are rejected during
    /// validation, so the leftover pass is a safety net, not a code path.
    /// </summary>
    private static List<TransferRowResult> Order(List<TransferRowResult> rows)
    {
        var byCode = rows
            .Where(r => Get(r, "AcctCode") != null)
            .GroupBy(r => Get(r, "AcctCode")!, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        var ordered = new List<TransferRowResult>();
        var placed  = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var pending = new List<TransferRowResult>(rows);

        while (pending.Count > 0)
        {
            var ready = pending.Where(r =>
            {
                var father = Get(r, "FatherNum");
                if (father == null || father.Equals("null", StringComparison.OrdinalIgnoreCase))
                    return true;
                // Parent not in this file → it is already in the database.
                return !byCode.ContainsKey(father) || placed.Contains(father);
            }).ToList();

            if (ready.Count == 0)
            {
                // Should be unreachable — validation rejects cycles first.
                ordered.AddRange(pending);
                break;
            }

            foreach (var r in ready)
            {
                ordered.Add(r);
                var c = Get(r, "AcctCode");
                if (c != null) placed.Add(c);
                pending.Remove(r);
            }
        }

        return ordered;
    }

    private static string CoaValue(COAEntity a, string column) => column switch
    {
        "AcctCode"           => a.AcctCode,
        "AcctName"           => a.AcctName,
        "FrgnName"           => a.FrgnName ?? "",
        "Level"              => a.Level.ToString(CultureInfo.InvariantCulture),
        "FatherNum"          => a.FatherNum ?? "",
        "AcctType"           => a.AcctType,
        "IsGroup"            => a.IsGroup ? "Yes" : "No",
        "IsActive"           => a.IsActive ? "Yes" : "No",
        "Currency"           => a.Currency ?? "",
        "AllowPosting"       => a.AllowPosting ? "Yes" : "No",
        "BlockManualPosting" => a.BlockManualPosting ? "Yes" : "No",
        "ControlAccount"     => a.ControlAccount ? "Yes" : "No",
        "CashAccount"        => a.CashAccount ? "Yes" : "No",
        "CashFlowRelevant"   => a.CashFlowRelevant ? "Yes" : "No",
        "Confidential"       => a.Confidential ? "Yes" : "No",
        "DfltVat"            => a.DfltVat ?? "",
        "TaxCode"            => a.TaxCode ?? "",
        "Project"            => a.Project ?? "",
        "CostCenter"         => a.CostCenter ?? "",
        "Department"         => a.Department ?? "",
        "ExternalCode"       => a.ExternalCode ?? "",
        "Description"        => a.Description ?? "",
        _                    => "",
    };

    // ═══════════════════════════════════════════════════════════════
    // BUSINESS PARTNER IMPORTER
    // ═══════════════════════════════════════════════════════════════
    private void ImportBusinessPartners(List<TransferRowResult> rows,
                                        ref int success, ref int failed,
                                        List<string> errors)
    {
        var codes = rows.Select(r => Get(r, "Code")!).Where(c => c != null).ToList();

        var existing = _db.Set<BPEntity>()
            .Where(b => codes.Contains(b.Code))
            .ToDictionary(b => b.Code, StringComparer.OrdinalIgnoreCase);

        foreach (var row in rows)
        {
            var code = Get(row, "Code")!;

            try
            {
                var isNew = !existing.TryGetValue(code, out var bp);
                if (isNew)
                {
                    bp = new BPEntity
                    {
                        Code      = code,
                        CreatedAt = DateTime.UtcNow,
                        InActive  = false,
                    };
                }

                bp!.CardName     = Get(row, "CardName") ?? bp.CardName;
                bp.FrgnName      = Get(row, "FrgnName")      ?? bp.FrgnName;
                bp.TypeStatus    = ParseType(Get(row, "TypeStatus"))     ?? bp.TypeStatus;
                bp.ActiveStatus  = ParseActive(Get(row, "ActiveStatus")) ?? bp.ActiveStatus;
                bp.GroupCode     = Get(row, "GroupCode")     ?? bp.GroupCode;
                bp.Currency      = Get(row, "Currency")?.ToUpperInvariant() ?? bp.Currency;
                bp.VatNumber     = Get(row, "VatNumber")     ?? bp.VatNumber;
                bp.Tel1          = Get(row, "Tel1")          ?? bp.Tel1;
                bp.Tel2          = Get(row, "Tel2")          ?? bp.Tel2;
                bp.MobilePhone   = Get(row, "MobilePhone")   ?? bp.MobilePhone;
                bp.Fax           = Get(row, "Fax")           ?? bp.Fax;
                bp.Email         = Get(row, "Email")         ?? bp.Email;
                bp.Website       = Get(row, "Website")       ?? bp.Website;
                bp.Industry      = Get(row, "Industry")      ?? bp.Industry;
                bp.Territory     = Get(row, "Territory")     ?? bp.Territory;
                bp.PayTerms      = Get(row, "PayTerms")      ?? bp.PayTerms;
                bp.Remarks       = Get(row, "Remarks")       ?? bp.Remarks;

                if (Dec(row, "CreditLimit")     is { } cl) bp.CreditLimit     = cl;
                if (Dec(row, "CommitmentLimit") is { } ml) bp.CommitmentLimit = ml;
                if (Dec(row, "TotalDiscount")   is { } td) bp.TotalDiscount   = td;

                if (isNew)
                {
                    _db.Set<BPEntity>().Add(bp);
                    existing[code] = bp;
                }
                else
                {
                    bp.UpdatedAt = DateTime.UtcNow;
                }

                success++;
            }
            catch (Exception ex)
            {
                failed++;
                errors.Add($"Row {row.RowNumber} ({code}): {Root(ex).Message}");
            }
        }
    }

    private static string BpValue(BPEntity bp, string column) => column switch
    {
        "Code"            => bp.Code,
        "CardName"        => bp.CardName,
        "FrgnName"        => bp.FrgnName ?? "",
        "TypeStatus"      => bp.TypeStatus switch { 1 => "Vendor", 2 => "Lead", _ => "Customer" },
        "ActiveStatus"    => bp.ActiveStatus switch { 1 => "Inactive", 2 => "Advanced", _ => "Active" },
        "GroupCode"       => bp.GroupCode ?? "",
        "Currency"        => bp.Currency ?? "",
        "VatNumber"       => bp.VatNumber ?? "",
        "Tel1"            => bp.Tel1 ?? "",
        "Tel2"            => bp.Tel2 ?? "",
        "MobilePhone"     => bp.MobilePhone ?? "",
        "Fax"             => bp.Fax ?? "",
        "Email"           => bp.Email ?? "",
        "Website"         => bp.Website ?? "",
        "Industry"        => bp.Industry ?? "",
        "Territory"       => bp.Territory ?? "",
        "PayTerms"        => bp.PayTerms ?? "",
        "CreditLimit"     => bp.CreditLimit.ToString(CultureInfo.InvariantCulture),
        "CommitmentLimit" => bp.CommitmentLimit.ToString(CultureInfo.InvariantCulture),
        "TotalDiscount"   => bp.TotalDiscount.ToString(CultureInfo.InvariantCulture),
        "Remarks"         => bp.Remarks ?? "",
        _                 => "",
    };

    private HashSet<string> LoadExistingKeys(string entityKey) => entityKey switch
    {
        "BusinessPartner" => _db.Set<BPEntity>()
                                .Select(b => b.Code)
                                .ToHashSet(StringComparer.OrdinalIgnoreCase),
        "ChartOfAccount"  => _db.Set<COAEntity>()
                                .Select(a => a.AcctCode)
                                .ToHashSet(StringComparer.OrdinalIgnoreCase),
        "ItemMaster"      => _db.Set<ItemEntity>()
                                .Select(i => i.ItemCode)
                                .ToHashSet(StringComparer.OrdinalIgnoreCase),
        "User"            => _db.Set<UserEntity>()
                                .Select(u => u.UserId)
                                .ToHashSet(StringComparer.OrdinalIgnoreCase),
        "ItemGroup"       => _db.Set<ItemGroupEntity>()
                                .Select(g => g.Code)
                                .ToHashSet(StringComparer.OrdinalIgnoreCase),
        "EmployeeMaster"  => _db.Set<EmployeeEntity>()
                                .Select(e => e.Code)
                                .ToHashSet(StringComparer.OrdinalIgnoreCase),
        _ => new HashSet<string>(StringComparer.OrdinalIgnoreCase),
    };

    // ═══════════════════════════════════════════════════════════════
    // HELPERS
    // ═══════════════════════════════════════════════════════════════
    private static string? Get(TransferRowResult row, string column) =>
        row.Values.TryGetValue(column, out var v) && !string.IsNullOrWhiteSpace(v)
            ? v.Trim()
            : null;

    private static decimal? Dec(TransferRowResult row, string column)
    {
        var raw = Get(row, column);
        if (raw == null) return null;
        return decimal.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out var d)
            ? d : null;
    }

    private static bool? Bool(TransferRowResult row, string column)
    {
        var raw = Get(row, column);
        return raw == null ? null : ParseBool(raw);
    }

    private static int? ParseType(string? v) => v?.Trim().ToLowerInvariant() switch
    {
        "customer" or "c" or "0" => 0,
        "vendor"   or "v" or "1" => 1,
        "lead"     or "l" or "2" => 2,
        _ => null,
    };

    private static int? ParseActive(string? v) => v?.Trim().ToLowerInvariant() switch
    {
        "active"   or "a" or "0" => 0,
        "inactive" or "i" or "1" => 1,
        "advanced" or "2"        => 2,
        _ => null,
    };

    private static bool? ParseBool(string v) => v.Trim().ToLowerInvariant() switch
    {
        "yes" or "y" or "true"  or "t" or "1" => true,
        "no"  or "n" or "false" or "f" or "0" => false,
        _ => null,
    };

    private static string Stringify(object? value) => value switch
    {
        null            => "",
        bool b          => b ? "Yes" : "No",
        DateTime dt     => dt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
        decimal d       => d.ToString(CultureInfo.InvariantCulture),
        double db       => db.ToString(CultureInfo.InvariantCulture),
        float f         => f.ToString(CultureInfo.InvariantCulture),
        IFormattable fm => fm.ToString(null, CultureInfo.InvariantCulture),
        _               => value.ToString() ?? "",
    };

    /// <summary>EF wraps the real problem — surface the innermost message.</summary>
    private static Exception Root(Exception ex)
    {
        while (ex.InnerException != null) ex = ex.InnerException;
        return ex;
    }

    private static ImportResultResponse ToResult(DataTransferJob job, List<string> errors) => new()
    {
        JobId       = job.Id,
        JobNo       = job.JobNo,
        Status      = job.Status,
        EntityKey   = job.EntityKey,
        EntityLabel = job.EntityLabel,
        TotalRows   = job.TotalRows,
        SuccessRows = job.SuccessRows,
        FailedRows  = job.FailedRows,
        SkippedRows = job.SkippedRows,
        DurationMs  = job.DurationMs,
        Errors      = errors.Take(200).ToList(),
    };
}
