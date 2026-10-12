using FarmingApi.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmingApi.Modules.Administration.Security;

// Singleton row — the same shape as Company (Modules.Company): one record
// holds the whole company's password rules, read by UserController on
// every login / password change. No row yet means "use the defaults"
// (the same rules that were hardcoded before this existed), so a fresh
// database behaves exactly as it did before.
public class PasswordPolicy : AuditableEntity
{
    public int  MinLength         { get; set; } = 8;
    public bool RequireUppercase  { get; set; } = true;
    public bool RequireLowercase  { get; set; } = true;
    public bool RequireDigit      { get; set; } = true;
    public bool RequireSpecial    { get; set; } = true;
    public int  MaxFailedAttempts { get; set; } = 5;

    // Days before a password is flagged as due for renewal. Null = never
    // expires. Informational only — an expired password still logs in;
    // see PasswordAdministration/List.jsx for why it doesn't lock anyone out.
    public int? ExpiryDays { get; set; }
}

public class PasswordPolicyConfig : IEntityTypeConfiguration<PasswordPolicy>
{
    public void Configure(EntityTypeBuilder<PasswordPolicy> b)
    {
        b.ToTable("KPWDPOL");
        b.HasKey(x => x.Id);
    }
}
