using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FarmingApi.Core;
using UserGroupEntity = FarmingApi.Modules.Administration.SetUp.UserGroup.UserGroup;

namespace FarmingApi.Modules.Administration.SetUp.User;

public class User : AuditableEntity
{
    public string UserId { get; set; } = null!;
    public string UserName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public int UserGroupId { get; set; }
    public string? Phone { get; set; }
    public char ActiveStatus { get; set; } = 'A';
    public DateTime? LastLogin { get; set; }
    public int LoginAttempts { get; set; } = 0;
    public bool IsLocked { get; set; } = false;

    // ── Two-factor (TOTP — Microsoft/Google Authenticator) ────────
    /// <summary>Base32 shared secret. Set at setup; only trusted once TwoFactorEnabled is true.</summary>
    public string? TwoFactorSecret  { get; set; }
    public bool    TwoFactorEnabled { get; set; } = false;
    /// <summary>Comma-separated SHA-256 hashes of single-use recovery codes.</summary>
    public string? TwoFactorRecoveryCodes { get; set; }

    // Navigation property
    public UserGroupEntity UserGroup { get; set; } = null!;
}

public class UserConfig : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("User");

        builder.HasKey(x => x.Id);

        builder.Property(m => m.UserId)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(m => m.UserName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(m => m.Email)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(m => m.PasswordHash)
            .IsRequired();

        builder.Property(m => m.Phone)
            .HasMaxLength(20);

        builder.Property(m => m.ActiveStatus)
            .HasColumnType("char(1)")
            .HasDefaultValue('A');

        // Unique constraints
        builder.HasIndex(m => m.UserId).IsUnique();
        builder.HasIndex(m => m.Email).IsUnique();

        // Foreign Key relationship: One UserGroup has Many Users
        builder.HasOne(m => m.UserGroup)
            .WithMany()
            .HasForeignKey(m => m.UserGroupId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}