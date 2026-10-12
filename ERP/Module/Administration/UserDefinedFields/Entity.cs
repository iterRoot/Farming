using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FarmingApi.Core;

namespace FarmingApi.Modules.Administration.UserDefinedFields;

// ══════════════════════════════════════════════════════════════════════════════
// USER-DEFINED FIELD DEFINITION (SAP-style UDF)
// One row = one field an admin added to a form/table. It then appears on every
// record of that TableName, and each record stores its own value (in the
// target document's Udf JSON column, keyed by FieldName).
//
// Table: OUDF   (SAP's UDF metadata table is CUFD; we keep an O-prefix here)
// ══════════════════════════════════════════════════════════════════════════════
public class UserDefinedField : AuditableEntity
{
    public string  TableName    { get; set; } = null!;   // target form, e.g. "ARInvoice"
    public string  FieldName    { get; set; } = null!;   // stored key, e.g. "ProjectPhase"
    public string  Label        { get; set; } = null!;   // UI label
    public string  FieldType    { get; set; } = "Text";  // Text|Number|Date|Checkbox|Dropdown
    public int?    Length       { get; set; }            // max length for Text
    public string? DefaultValue { get; set; }
    public List<string> ValidValues { get; set; } = new(); // choices for Dropdown
    public string? LinkedTable  { get; set; }            // Dropdown only: take the choices from this user-defined table's row codes
    public bool    Mandatory    { get; set; }
    public int     SortOrder    { get; set; }
    public bool    Active       { get; set; } = true;
}

public class UserDefinedFieldConfig : IEntityTypeConfiguration<UserDefinedField>
{
    public void Configure(EntityTypeBuilder<UserDefinedField> b)
    {
        b.ToTable("OUDF");
        b.HasKey(x => x.Id);

        b.Property(x => x.TableName).HasMaxLength(50).IsRequired();
        b.Property(x => x.FieldName).HasMaxLength(50).IsRequired();
        b.Property(x => x.Label).HasMaxLength(100).IsRequired();
        b.Property(x => x.FieldType).HasMaxLength(20).IsRequired();
        b.Property(x => x.DefaultValue).HasMaxLength(300);
        b.Property(x => x.LinkedTable).HasMaxLength(20);

        // One field name per table.
        b.HasIndex(x => new { x.TableName, x.FieldName }).IsUnique();
    }
}


// One user-defined field's value on one record (KUDV): the record is the row of the form's own table
// (a business partner, an AR invoice, ...) identified by its Id.
public class UdfValue
{
    public int    Id        { get; set; }
    public string TableName { get; set; } = null!;   // the UDF form key, e.g. "ARInvoice"
    public int    RecordId  { get; set; }
    public string FieldName { get; set; } = null!;
    public string Value     { get; set; } = "";
}

public class UdfValueConfig : IEntityTypeConfiguration<UdfValue>
{
    public void Configure(EntityTypeBuilder<UdfValue> b)
    {
        b.ToTable("KUDV");
        b.HasKey(x => x.Id);
        b.Property(x => x.TableName).HasMaxLength(50).IsRequired();
        b.Property(x => x.FieldName).HasMaxLength(50).IsRequired();
        b.Property(x => x.Value).HasMaxLength(2000).IsRequired();
        b.HasIndex(x => new { x.TableName, x.RecordId, x.FieldName }).IsUnique();
    }
}
