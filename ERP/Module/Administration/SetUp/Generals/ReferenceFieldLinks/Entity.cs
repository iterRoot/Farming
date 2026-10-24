using FarmingApi.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmingApi.Modules.Administration.ReferenceFieldLink;

// ═══════════════════════════════════════════════════════════════
// REFERENCE FIELD LINK  (KRFL)
// Defines how a field in one module/document resolves its
// values from another module or table.
//
// Use cases:
//  • ARInvoice.CustomerId     → BusinessPartner [Code/Name]
//  • PurchaseOrder.VendorId   → BusinessPartner [Code/Name]
//  • GoodsReceipt.WarehouseId → Warehouse       [Code/Name]
//  • SalesOrder.SalesPersonId → User            [UserCode/FullName]
//  • Invoice.TerritoryCode    → Territory       [Code/Name]
//
// LinkType:
//  Lookup   → read-only reference for display (most common)
//  Cascade  → when source changes, auto-fill related fields
//  Validate → value must exist in target table
//  Copy     → copies value from target field at save time
//
// Each link also carries optional AutoFillFields:
//  A JSON array of { sourceField, targetField } pairs that
//  are auto-populated when a user selects the linked record.
//  e.g. select Customer → fill Name, Address, PaymentTerms
// ═══════════════════════════════════════════════════════════════
public class ReferenceFieldLink : AuditableEntity
{
    // ── Identity ──────────────────────────────────────────────
    public string   Code          { get; set; } = null!;   // e.g. "RFL-ARINV-CUST"
    public string   Name          { get; set; } = null!;   // e.g. "AR Invoice → Customer"

    // ── Source (where the field lives) ────────────────────────
    public string   SourceModule  { get; set; } = null!;   // e.g. "ARInvoice"
    public string   SourceField   { get; set; } = null!;   // e.g. "CustomerId"
    public string?  SourceLabel   { get; set; }            // display label on the form

    // ── Target (where values come from) ───────────────────────
    public string   TargetTable   { get; set; } = null!;   // e.g. "BusinessPartner"
    public string   TargetKeyField{ get; set; } = null!;   // field used as the value  e.g. "Code"
    public string   TargetDisplayField { get; set; } = null!; // field shown in dropdown  e.g. "Name"
    public string?  TargetFilter  { get; set; }            // JSON filter e.g. {"Type":"Customer"}
    public string?  TargetOrderBy { get; set; }            // e.g. "Name ASC"
    public string?  TargetApiEndpoint { get; set; }        // REST endpoint to fetch options

    // ── Behaviour ─────────────────────────────────────────────
    public string   LinkType      { get; set; } = "Lookup"; // Lookup / Cascade / Validate / Copy
    public bool     IsRequired    { get; set; } = false;
    public bool     AllowSearch   { get; set; } = true;    // enable type-to-search
    public bool     AllowCreate   { get; set; } = false;   // allow quick-create from field
    public int      SortOrder     { get; set; } = 0;

    // ── Auto-fill fields ──────────────────────────────────────
    // JSON array: [{"sourceField":"CustomerName","targetField":"Name"},...]
    // When user selects a record, these fields are filled automatically
    public string?  AutoFillFields { get; set; }

    // ── Metadata ──────────────────────────────────────────────
    public string?  Description   { get; set; }
    public bool     IsActive      { get; set; } = true;
    public string?  Remarks       { get; set; }
}

public class ReferenceFieldLinkConfig
    : IEntityTypeConfiguration<ReferenceFieldLink>
{
    public void Configure(EntityTypeBuilder<ReferenceFieldLink> b)
    {
        b.ToTable("KRFL");
        b.HasKey(x => x.Id);

        b.Property(x => x.Code).HasMaxLength(100).IsRequired();
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.SourceModule).HasMaxLength(100).IsRequired();
        b.Property(x => x.SourceField).HasMaxLength(100).IsRequired();
        b.Property(x => x.SourceLabel).HasMaxLength(200);
        b.Property(x => x.TargetTable).HasMaxLength(100).IsRequired();
        b.Property(x => x.TargetKeyField).HasMaxLength(100).IsRequired();
        b.Property(x => x.TargetDisplayField).HasMaxLength(100).IsRequired();
        b.Property(x => x.TargetFilter).HasMaxLength(1000);
        b.Property(x => x.TargetOrderBy).HasMaxLength(200);
        b.Property(x => x.TargetApiEndpoint).HasMaxLength(300);
        b.Property(x => x.LinkType).HasMaxLength(20);
        b.Property(x => x.AutoFillFields).HasMaxLength(2000);
        b.Property(x => x.Description).HasMaxLength(500);
        b.Property(x => x.Remarks).HasMaxLength(500);

        b.HasIndex(x => x.Code).IsUnique();
        // Unique per source field per module
        b.HasIndex(x => new { x.SourceModule, x.SourceField }).IsUnique();
    }
}