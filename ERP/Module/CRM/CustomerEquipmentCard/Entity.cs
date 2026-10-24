using FarmingApi.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmingApi.Modules.CRM.CustomerEquipmentCard;

// ═══════════════════════════════════════════════════════════════════
// CUSTOMER EQUIPMENT CARD  (KEQP)
// SAP B1 equivalent: Service > Customer Equipment Card. One serialised
// unit a customer owns — the thing a Service Call is raised against.
// Tracks the item, its serial numbers, who owns it, where it is and its
// warranty window.
// ═══════════════════════════════════════════════════════════════════
public class CustomerEquipmentCard : AuditableEntity
{
    public string  CardCode       { get; set; } = null!;   // auto: EQ-0001
    public string  ItemCode       { get; set; } = null!;
    public string? ItemName       { get; set; }
    public string? ManufacturerSN { get; set; }            // manufacturer serial no
    public string? InternalSN     { get; set; }            // internal serial no

    public string  CustomerCode   { get; set; } = null!;   // owning BP
    public string? CustomerName   { get; set; }

    public string  Status         { get; set; } = "Active"; // Active | Inactive | Terminated
    public DateTime? DeliveryDate { get; set; }
    public DateTime? WarrantyStart { get; set; }
    public DateTime? WarrantyEnd  { get; set; }
    public string? Location       { get; set; }
    public string? Remarks        { get; set; }
}

public class CustomerEquipmentCardConfig : IEntityTypeConfiguration<CustomerEquipmentCard>
{
    public void Configure(EntityTypeBuilder<CustomerEquipmentCard> b)
    {
        b.ToTable("KEQP");
        b.HasKey(x => x.Id);
        b.Property(x => x.CardCode).HasMaxLength(30).IsRequired();
        b.HasIndex(x => x.CardCode).IsUnique();
        b.Property(x => x.ItemCode).HasMaxLength(50).IsRequired();
        b.Property(x => x.ItemName).HasMaxLength(200);
        b.Property(x => x.ManufacturerSN).HasMaxLength(100);
        b.Property(x => x.InternalSN).HasMaxLength(100);
        b.Property(x => x.CustomerCode).HasMaxLength(50).IsRequired();
        b.Property(x => x.CustomerName).HasMaxLength(200);
        b.Property(x => x.Status).HasMaxLength(15);
        b.Property(x => x.Location).HasMaxLength(200);
        b.Property(x => x.Remarks).HasMaxLength(500);
        b.HasIndex(x => x.CustomerCode);
    }
}
