using FarmingApi.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmingApi.Modules.Administration.UserTables;

// ═══════════════════════════════════════════════════════════════
// USER-DEFINED TABLES (SAP B1 "UDT")
// An admin defines a small table — a code, a name and typed columns — and fills it with rows
// (code + name + one value per column). Dropdown user-defined fields can draw their choices
// from a table's row codes.
//   KUDT  the table       KUTF  its columns       KUTR  its rows
// ═══════════════════════════════════════════════════════════════
public class UserTable : AuditableEntity
{
    public string  Code        { get; set; } = null!;   // e.g. "SEASONS"
    public string  Name        { get; set; } = null!;
    public string? Description { get; set; }
    public List<UserTableField> Fields { get; set; } = new();
    public List<UserTableRow>   Rows   { get; set; } = new();
}

public class UserTableField
{
    public int    Id          { get; set; }
    public int    UserTableId { get; set; }
    public int    SortOrder   { get; set; }
    public string FieldCode   { get; set; } = null!;
    public string Label       { get; set; } = null!;
    public string DataType    { get; set; } = "Text";   // Text | Number | Date | Checkbox | Dropdown
    public int?   Length      { get; set; }
    public bool   Required    { get; set; }
    public string? DefaultValue { get; set; }
    public List<string> ValidValues { get; set; } = new();
    public UserTable? UserTable { get; set; }
}

public class UserTableRow
{
    public int      Id          { get; set; }
    public int      UserTableId { get; set; }
    public string   RowKey      { get; set; } = null!;
    public string?  RowName     { get; set; }
    public string   Data        { get; set; } = "{}";    // { fieldCode: value }
    public DateTime CreatedAt   { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt  { get; set; }
    public UserTable? UserTable { get; set; }
}

public class UserTableConfig : IEntityTypeConfiguration<UserTable>
{
    public void Configure(EntityTypeBuilder<UserTable> b)
    {
        b.ToTable("KUDT");
        b.HasKey(x => x.Id);
        b.Property(x => x.Code).HasMaxLength(20).IsRequired();
        b.Property(x => x.Name).HasMaxLength(100).IsRequired();
        b.Property(x => x.Description).HasMaxLength(300);
        b.HasIndex(x => x.Code).IsUnique();
    }
}

public class UserTableFieldConfig : IEntityTypeConfiguration<UserTableField>
{
    public void Configure(EntityTypeBuilder<UserTableField> b)
    {
        b.ToTable("KUTF");
        b.HasKey(x => x.Id);
        b.Property(x => x.FieldCode).HasMaxLength(30).IsRequired();
        b.Property(x => x.Label).HasMaxLength(100).IsRequired();
        b.Property(x => x.DataType).HasMaxLength(20).IsRequired();
        b.Property(x => x.DefaultValue).HasMaxLength(300);
        b.HasIndex(x => new { x.UserTableId, x.FieldCode }).IsUnique();
        b.HasOne(x => x.UserTable).WithMany(t => t.Fields).HasForeignKey(x => x.UserTableId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class UserTableRowConfig : IEntityTypeConfiguration<UserTableRow>
{
    public void Configure(EntityTypeBuilder<UserTableRow> b)
    {
        b.ToTable("KUTR");
        b.HasKey(x => x.Id);
        b.Property(x => x.RowKey).HasMaxLength(50).IsRequired();
        b.Property(x => x.RowName).HasMaxLength(200);
        b.Property(x => x.Data).HasColumnType("jsonb").HasDefaultValue("{}");
        b.HasIndex(x => new { x.UserTableId, x.RowKey }).IsUnique();
        b.HasOne(x => x.UserTable).WithMany(t => t.Rows).HasForeignKey(x => x.UserTableId).OnDelete(DeleteBehavior.Cascade);
    }
}
