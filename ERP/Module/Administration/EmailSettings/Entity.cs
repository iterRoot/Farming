using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FarmingApi.Core;

namespace FarmingApi.Modules.Administration.EmailSettings;

// An SMTP profile used to send system e-mail (notifications, document copies…).
// Multiple profiles are supported; exactly one may be flagged as the default.
public class EmailAccount : AuditableEntity
{
    public string  Name        { get; set; } = null!;   // profile label
    public string  SmtpHost    { get; set; } = null!;
    public int     SmtpPort    { get; set; } = 587;
    public string  Encryption  { get; set; } = "TLS";   // None | SSL | TLS
    public bool    UseAuth     { get; set; } = true;
    public string? Username    { get; set; }
    public string? Password    { get; set; }
    public string  FromEmail   { get; set; } = null!;
    public string? FromName    { get; set; }
    public string? ReplyTo     { get; set; }
    public string? Signature   { get; set; }
    public bool    IsDefault   { get; set; }
}

public class EmailAccountConfig : IEntityTypeConfiguration<EmailAccount>
{
    public void Configure(EntityTypeBuilder<EmailAccount> b)
    {
        b.ToTable("KEML");
        b.HasKey(x => x.Id);
        b.Property(x => x.Name).HasMaxLength(120).IsRequired();
        b.Property(x => x.SmtpHost).HasMaxLength(200).IsRequired();
        b.Property(x => x.Encryption).HasMaxLength(10).IsRequired();
        b.Property(x => x.FromEmail).HasMaxLength(200).IsRequired();
    }
}
