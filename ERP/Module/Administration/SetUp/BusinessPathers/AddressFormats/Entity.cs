using FarmingApi.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmingApi.Modules.Administration.AddressFormats;

// ═══════════════════════════════════════════════════════════════
// ADDRESS FORMAT  (KADF)
// SAP B1 equivalent: Address Formats
// Defines how postal addresses are structured and printed
// for each country. A format is a named template composed
// of ordered field lines that map to address data fields.
//
// Example — Cambodian format:
//   Line 1 → Street / HouseNumber
//   Line 2 → Commune / Sangkat
//   Line 3 → District / Khan
//   Line 4 → Province / City  PostalCode
//   Line 5 → Country
// ═══════════════════════════════════════════════════════════════
public class AddressFormat : AuditableEntity
{
    public string  Code        { get; set; } = null!;  // e.g. "ADDR-KH"
    public string  Name        { get; set; } = null!;  // e.g. "Cambodia (KH)"
    public string? CountryCode { get; set; }           // ISO-3166 e.g. "KH"
    public string? Description { get; set; }
    public bool    IsDefault   { get; set; } = false;
    public bool    IsActive    { get; set; } = true;
    public string? Remarks     { get; set; }

    public ICollection<AddressFormatLine> Lines { get; set; }
        = new List<AddressFormatLine>();
}

// ─────────────────────────────────────────────────────────────
// ADDRESS FORMAT LINE  (KAD1)
// Each line in the address template.
// FieldKey maps to known address fields; Separator is the
// text placed between fields that appear on the same line.
// ─────────────────────────────────────────────────────────────
public class AddressFormatLine
{
    public int     Id              { get; set; }
    public int     AddressFormatId { get; set; }
    public int     LineOrder       { get; set; }    // 1, 2, 3 ...

    // Each line can hold up to 3 address tokens
    public string? Field1          { get; set; }   // e.g. "Street"
    public string? Field2          { get; set; }   // e.g. "HouseNumber"
    public string? Field3          { get; set; }   // e.g. "Building"
    public string? Separator12     { get; set; }   // separator between Field1-Field2 e.g. ", "
    public string? Separator23     { get; set; }   // separator between Field2-Field3
    public bool    PrintIfEmpty    { get; set; } = false;  // skip line if all fields empty

    public AddressFormat? AddressFormat { get; set; }
}

// ─── Known address field keys ─────────────────────────────────
// Used as FieldKey values in AddressFormatLine
public static class AddressFields
{
    public const string Street      = "Street";
    public const string HouseNum    = "HouseNumber";
    public const string Building    = "Building";
    public const string Floor       = "Floor";
    public const string Apartment   = "Apartment";
    public const string Commune     = "Commune";
    public const string District    = "District";
    public const string Province    = "Province";
    public const string City        = "City";
    public const string PostalCode  = "PostalCode";
    public const string StateRegion = "StateRegion";
    public const string Country     = "Country";
    public const string POBox       = "POBox";
    public const string Attention   = "Attention";
    public const string CompanyName = "CompanyName";
}

// ─── EF Configurations ───────────────────────────────────────
public class AddressFormatConfig : IEntityTypeConfiguration<AddressFormat>
{
    public void Configure(EntityTypeBuilder<AddressFormat> b)
    {
        b.ToTable("KADF");
        b.HasKey(x => x.Id);

        b.Property(x => x.Code).HasMaxLength(50).IsRequired();
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.CountryCode).HasMaxLength(10);
        b.Property(x => x.Description).HasMaxLength(500);
        b.Property(x => x.Remarks).HasMaxLength(500);

        b.HasIndex(x => x.Code).IsUnique();

        b.HasMany(x => x.Lines)
         .WithOne(x => x.AddressFormat)
         .HasForeignKey(x => x.AddressFormatId)
         .OnDelete(DeleteBehavior.Cascade);
    }
}

public class AddressFormatLineConfig : IEntityTypeConfiguration<AddressFormatLine>
{
    public void Configure(EntityTypeBuilder<AddressFormatLine> b)
    {
        b.ToTable("KAD1");
        b.HasKey(x => x.Id);

        b.Property(x => x.Field1).HasMaxLength(50);
        b.Property(x => x.Field2).HasMaxLength(50);
        b.Property(x => x.Field3).HasMaxLength(50);
        b.Property(x => x.Separator12).HasMaxLength(20);
        b.Property(x => x.Separator23).HasMaxLength(20);
    }
}