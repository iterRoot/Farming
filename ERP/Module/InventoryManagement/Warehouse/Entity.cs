using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FarmingApi.Core;

namespace FarmingApi.Modules.Sale.Warehouse;

// Warehouse master. `Code`/`Name` keep the original WhsCode/WhsName columns;
// the remaining fields are added on top. `InActive` (from AuditableEntity) is
// the inverse of the UI's "Active" flag.
public class Warehouse : AuditableEntity
{
    public string  Code     { get; set; } = null!;   // column WhsCode
    public string  Name     { get; set; } = null!;   // column WhsName

    public string? Location { get; set; }
    public string? Manager  { get; set; }
    public string? Phone    { get; set; }
    public string? Email    { get; set; }
    public string? Remarks  { get; set; }
    public string? Branch   { get; set; }

    // Address
    public string? Address  { get; set; }   // freeform address (from the form)
    public string? Street   { get; set; }
    public string? City     { get; set; }
    public string? State    { get; set; }
    public string? ZipCode  { get; set; }
    public string? Country  { get; set; }

    // Behaviour
    public bool    IsDefault { get; set; }
    public bool    Nettable  { get; set; } = true;   // stock counts toward availability
    public bool    DropShip  { get; set; }
}

public class WarehouseConfig : IEntityTypeConfiguration<Warehouse>
{
    public void Configure(EntityTypeBuilder<Warehouse> builder)
    {
        builder.ToTable("Warehouse");

        builder.HasKey(x => x.Id);                      // single PK (was double HasKey)
        builder.HasIndex(x => x.Code).IsUnique();

        builder.Property(x => x.Code).HasColumnName("WhsCode").HasMaxLength(20).IsRequired();
        builder.Property(x => x.Name).HasColumnName("WhsName").HasMaxLength(200).IsRequired();
        builder.Property(x => x.Location).HasMaxLength(200);
        builder.Property(x => x.Manager).HasMaxLength(120);
        builder.Property(x => x.Phone).HasMaxLength(40);
        builder.Property(x => x.Email).HasMaxLength(120);
        builder.Property(x => x.Branch).HasMaxLength(60);
        builder.Property(x => x.City).HasMaxLength(80);
        builder.Property(x => x.State).HasMaxLength(80);
        builder.Property(x => x.ZipCode).HasMaxLength(20);
        builder.Property(x => x.Country).HasMaxLength(80);
    }
}
