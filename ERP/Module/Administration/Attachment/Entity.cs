using FarmingApi.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmingApi.Modules.Administration.Attachment;

// ═══════════════════════════════════════════════════════════════
// ATTACHMENT  (KATF)  ·  a real uploaded file linked to a record
// The bytes live in the database (KSFL, see StoredFile and
// AttachmentStorage); this row is the index. OwnerType/OwnerId is a
// plain pair, not an FK, so an attachment never blocks deleting its
// owner — the owner's delete action removes its files instead.
// ═══════════════════════════════════════════════════════════════
public class Attachment : AuditableEntity
{
    public string   OwnerType      { get; set; } = null!;   // "BusinessPartner"
    public int      OwnerId        { get; set; }
    public string   FileName       { get; set; } = null!;   // name as uploaded (sanitised)
    public string   StoredName     { get; set; } = null!;   // GUID + extension — the key of its KSFL row
    public string   MimeType       { get; set; } = null!;
    public long     SizeBytes      { get; set; }
    public string   FileHash       { get; set; } = null!;   // SHA-256
    public DateTime AttachmentDate { get; set; }
    public string?  FreeText       { get; set; }
    public string?  TargetPath     { get; set; }
}

public class AttachmentConfig : IEntityTypeConfiguration<Attachment>
{
    public void Configure(EntityTypeBuilder<Attachment> b)
    {
        b.ToTable("KATF");
        b.HasKey(x => x.Id);
        b.Property(x => x.OwnerType).HasMaxLength(50).IsRequired();
        b.Property(x => x.FileName).HasMaxLength(200).IsRequired();
        b.Property(x => x.StoredName).HasMaxLength(100).IsRequired();
        b.Property(x => x.MimeType).HasMaxLength(100).IsRequired();
        b.Property(x => x.FileHash).HasMaxLength(64).IsRequired();
        b.Property(x => x.FreeText).HasMaxLength(500);
        b.Property(x => x.TargetPath).HasMaxLength(300);
        b.HasIndex(x => new { x.OwnerType, x.OwnerId });
    }
}

// ═══════════════════════════════════════════════════════════════
// STORED FILE  (KSFL)  ·  the bytes of an uploaded file
// Kept apart from the rows that describe files (KATF, ARI2) so
// listing attachments or loading an invoice never pulls file
// contents. StoredName is the key those rows hold. Stored in
// Postgres because the host's local disk is wiped on restart.
// ═══════════════════════════════════════════════════════════════
public class StoredFile : Entity
{
    public string   StoredName { get; set; } = null!;
    public byte[]   Content    { get; set; } = null!;
    public DateTime CreatedAt  { get; set; }
}

public class StoredFileConfig : IEntityTypeConfiguration<StoredFile>
{
    public void Configure(EntityTypeBuilder<StoredFile> b)
    {
        b.ToTable("KSFL");
        b.HasKey(x => x.Id);
        b.Property(x => x.StoredName).HasMaxLength(200).IsRequired();
        b.Property(x => x.Content).IsRequired();
        b.HasIndex(x => x.StoredName).IsUnique();
    }
}
