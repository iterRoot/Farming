using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FarmingApi.Core;

namespace FarmingApi.Modules.Administration.DocumentSettings;

// SAP B1 "Document Settings" splits into a company-wide General tab (singleton)
// and Per-Document settings (one row per document type). Both are read/written
// as whole flag maps, so each is stored as a jsonb document — new flags need
// no migration.

// General tab — singleton.
public class DocumentGeneralSetting : AuditableEntity
{
    public string Settings { get; set; } = "{}";
}

// Per-Document tab — one row per document type.
public class DocumentTypeSetting : AuditableEntity
{
    public string DocumentType { get; set; } = null!;   // e.g. "SaleOrder"
    public string Settings     { get; set; } = "{}";
}

public class DocumentGeneralSettingConfig : IEntityTypeConfiguration<DocumentGeneralSetting>
{
    public void Configure(EntityTypeBuilder<DocumentGeneralSetting> b)
    {
        b.ToTable("KDGS");
        b.HasKey(x => x.Id);
        b.Property(x => x.Settings).HasColumnType("jsonb").IsRequired();
    }
}

public class DocumentTypeSettingConfig : IEntityTypeConfiguration<DocumentTypeSetting>
{
    public void Configure(EntityTypeBuilder<DocumentTypeSetting> b)
    {
        b.ToTable("KDDS");
        b.HasKey(x => x.Id);
        b.HasIndex(x => x.DocumentType).IsUnique();
        b.Property(x => x.DocumentType).HasMaxLength(50).IsRequired();
        b.Property(x => x.Settings).HasColumnType("jsonb").IsRequired();
    }
}
