using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FarmingApi.Core;

namespace FarmingApi.Modules.BusinessPartners.BusinessPartnersMaster;

// ═══════════════════════════════════════════════════════════════════
// TABLE NAME CONVENTION — 4 letters, starts with K
// ─────────────────────────────────────────────────────────────────
//  KOCRD  → Business Partners Master   (SAP: OCRD)
//  KCRD  → BP Address                  (SAP: CRD1)  ← was KCRD1 (5 letters)
//  KCPR  → BP Contact Person           (SAP: OCPR)  ← was KOCPR (5 letters)
//  KCRB  → BP Bank Account             (SAP: OCRB)  ← was KOCRB (5 letters)
// ═══════════════════════════════════════════════════════════════════

// ═══════════════════════════════════════════════════════════════════
// BUSINESS PARTNERS MASTER — SAP B1 OCRD
// Table: KOCR  (4 letters, starts with K)
// ═══════════════════════════════════════════════════════════════════
public class BusinessPartnersMaster : AuditableEntity
{
    // ── Core Identity ────────────────────────────────────────────
    public string Code { get; set; } = null!;           // BP Code  e.g. "C0001", "V0001"
    public string CardName { get; set; } = null!;       // Full name / Company name
    public string? FrgnName { get; set; }               // Foreign language name

    // ── Type & Status ────────────────────────────────────────────
    public int TypeStatus { get; set; }                 // 0=Customer, 1=Vendor, 2=Lead
    public int ActiveStatus { get; set; }               // 0=Active,   1=Inactive

    // ── Contact Info ─────────────────────────────────────────────
    public string? Tel1 { get; set; }
    public string? Tel2 { get; set; }
    public string? MobilePhone { get; set; }
    public string? Fax { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
    public string? Note { get; set; }

    // ── Financial ────────────────────────────────────────────────
    public string? Currency { get; set; }
    public decimal Balance { get; set; }
    public decimal CreditLimit { get; set; }
    public string? PayTerms { get; set; }
    public string? PriceList { get; set; }
    public string? GroupCode { get; set; }

    // ── Tax ──────────────────────────────────────────────────────
    public string? TaxId { get; set; }
    public string? VatGroup { get; set; }

    // ── System ───────────────────────────────────────────────────
    public int? UserSign { get; set; }
    public int VersionNum { get; set; } = 1;

    // ── Navigation ───────────────────────────────────────────────
    public ICollection<BPAddress>     Addresses { get; set; } = new List<BPAddress>();
    public ICollection<BPContact>     Contacts  { get; set; } = new List<BPContact>();
    public ICollection<BPBankAccount> BankAccts { get; set; } = new List<BPBankAccount>();
}

// ═══════════════════════════════════════════════════════════════════
// BP ADDRESS — SAP B1 CRD1
// Table: KCRD  (4 letters, starts with K)
// ═══════════════════════════════════════════════════════════════════
public class BPAddress : AuditableEntity
{
    public int BPId { get; set; }                       // FK → KOCR.Id

    public int AdresType { get; set; }                  // 0=Ship-To, 1=Bill-To
    public string? AdressName { get; set; }
    public string? Street { get; set; }
    public string? Block { get; set; }
    public string? City { get; set; }
    public string? ZipCode { get; set; }
    public string? County { get; set; }
    public string? Country { get; set; }
    public string? State { get; set; }
    public bool IsDefault { get; set; }

    public BusinessPartnersMaster? BP { get; set; }
}

// ═══════════════════════════════════════════════════════════════════
// BP CONTACT — SAP B1 OCPR
// Table: KCPR  (4 letters, starts with K)
// ═══════════════════════════════════════════════════════════════════
public class BPContact : AuditableEntity
{
    public int BPId { get; set; }                       // FK → KOCR.Id

    public string? Name { get; set; }
    public string? Position { get; set; }
    public string? Tel1 { get; set; }
    public string? MobilePhone { get; set; }
    public string? Email { get; set; }
    public string? Note { get; set; }
    public bool IsDefault { get; set; }

    public BusinessPartnersMaster? BP { get; set; }
}

// ═══════════════════════════════════════════════════════════════════
// BP BANK ACCOUNT — SAP B1 OCRB
// Table: KCRB  (4 letters, starts with K)
// ═══════════════════════════════════════════════════════════════════
public class BPBankAccount : AuditableEntity
{
    public int BPId { get; set; }                       // FK → KOCR.Id

    public string? BankCode { get; set; }
    public string? BankName { get; set; }
    public string? AccountNo { get; set; }
    public string? Branch { get; set; }
    public string? Currency { get; set; }
    public string? Iban { get; set; }
    public string? SwiftNum { get; set; }
    public bool IsDefault { get; set; }

    public BusinessPartnersMaster? BP { get; set; }
}

// ═══════════════════════════════════════════════════════════════════
// ENTITY CONFIGURATIONS
// ═══════════════════════════════════════════════════════════════════

public class BusinessPartnersMasterConfig : IEntityTypeConfiguration<BusinessPartnersMaster>
{
    public void Configure(EntityTypeBuilder<BusinessPartnersMaster> builder)
    {
        builder.ToTable("KOCR");                        // ✅ 4 letters, starts with K
        builder.HasKey(x => x.Id);

        // Core
        builder.Property(m => m.Code).HasMaxLength(50).IsRequired();
        builder.HasIndex(m => m.Code).IsUnique();
        builder.Property(m => m.CardName).HasMaxLength(200).IsRequired();
        builder.Property(m => m.FrgnName).HasMaxLength(200);

        // Type & Status
        builder.Property(m => m.TypeStatus).IsRequired().HasDefaultValue(0);
        builder.Property(m => m.ActiveStatus).IsRequired().HasDefaultValue(0);

        // Contact
        builder.Property(m => m.Tel1).HasMaxLength(30);
        builder.Property(m => m.Tel2).HasMaxLength(30);
        builder.Property(m => m.MobilePhone).HasMaxLength(30);
        builder.Property(m => m.Fax).HasMaxLength(30);
        builder.Property(m => m.Email).HasMaxLength(100);
        builder.Property(m => m.Website).HasMaxLength(100);
        builder.Property(m => m.Note).HasMaxLength(500);

        // Financial
        builder.Property(m => m.Currency).HasMaxLength(3);
        builder.Property(m => m.Balance).HasPrecision(18, 2).HasDefaultValue(0);
        builder.Property(m => m.CreditLimit).HasPrecision(18, 2).HasDefaultValue(0);
        builder.Property(m => m.PayTerms).HasMaxLength(20);
        builder.Property(m => m.PriceList).HasMaxLength(20);
        builder.Property(m => m.GroupCode).HasMaxLength(20);

        // Tax
        builder.Property(m => m.TaxId).HasMaxLength(50);
        builder.Property(m => m.VatGroup).HasMaxLength(20);

        // System
        builder.Property(m => m.VersionNum).HasDefaultValue(1);

        // Indexes
        builder.HasIndex(m => m.TypeStatus);
        builder.HasIndex(m => m.ActiveStatus);
        builder.HasIndex(m => m.CardName);

        // Relations — cascade delete children when BP is deleted
        builder.HasMany(m => m.Addresses)
            .WithOne(a => a.BP)
            .HasForeignKey(a => a.BPId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(m => m.Contacts)
            .WithOne(c => c.BP)
            .HasForeignKey(c => c.BPId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(m => m.BankAccts)
            .WithOne(b => b.BP)
            .HasForeignKey(b => b.BPId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class BPAddressConfig : IEntityTypeConfiguration<BPAddress>
{
    public void Configure(EntityTypeBuilder<BPAddress> builder)
    {
        builder.ToTable("KCRD");                        // ✅ 4 letters, starts with K
        builder.HasKey(x => x.Id);

        builder.Property(m => m.BPId).IsRequired();
        builder.Property(m => m.AdresType).HasDefaultValue(0);
        builder.Property(m => m.AdressName).HasMaxLength(100);
        builder.Property(m => m.Street).HasMaxLength(200);
        builder.Property(m => m.Block).HasMaxLength(100);
        builder.Property(m => m.City).HasMaxLength(100);
        builder.Property(m => m.ZipCode).HasMaxLength(20);
        builder.Property(m => m.County).HasMaxLength(100);
        builder.Property(m => m.Country).HasMaxLength(50);
        builder.Property(m => m.State).HasMaxLength(100);
        builder.Property(m => m.IsDefault).HasDefaultValue(false);

        builder.HasIndex(m => m.BPId);
    }
}

public class BPContactConfig : IEntityTypeConfiguration<BPContact>
{
    public void Configure(EntityTypeBuilder<BPContact> builder)
    {
        builder.ToTable("KCPR");                        // ✅ 4 letters, starts with K
        builder.HasKey(x => x.Id);

        builder.Property(m => m.BPId).IsRequired();
        builder.Property(m => m.Name).HasMaxLength(100);
        builder.Property(m => m.Position).HasMaxLength(100);
        builder.Property(m => m.Tel1).HasMaxLength(30);
        builder.Property(m => m.MobilePhone).HasMaxLength(30);
        builder.Property(m => m.Email).HasMaxLength(100);
        builder.Property(m => m.Note).HasMaxLength(500);
        builder.Property(m => m.IsDefault).HasDefaultValue(false);

        builder.HasIndex(m => m.BPId);
    }
}

public class BPBankAccountConfig : IEntityTypeConfiguration<BPBankAccount>
{
    public void Configure(EntityTypeBuilder<BPBankAccount> builder)
    {
        builder.ToTable("KCRB");                        // ✅ 4 letters, starts with K
        builder.HasKey(x => x.Id);

        builder.Property(m => m.BPId).IsRequired();
        builder.Property(m => m.BankCode).HasMaxLength(20);
        builder.Property(m => m.BankName).HasMaxLength(100);
        builder.Property(m => m.AccountNo).HasMaxLength(50);
        builder.Property(m => m.Branch).HasMaxLength(100);
        builder.Property(m => m.Currency).HasMaxLength(3);
        builder.Property(m => m.Iban).HasMaxLength(50);
        builder.Property(m => m.SwiftNum).HasMaxLength(20);
        builder.Property(m => m.IsDefault).HasDefaultValue(false);

        builder.HasIndex(m => m.BPId);
    }
}