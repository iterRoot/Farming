using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FarmingApi.Core;

namespace FarmingApi.Modules.InventoryManagement.PickList;

// Warehouse pick list — a picking sheet raised against open sales documents.
// Header KPKL, lines PKL1. This module records the pick; it does not yet move
// stock or close the source order lines.
public class PickList : AuditableEntity
{
    public string    PickNumber { get; set; } = null!;   // PICK-2026-00001
    public DateTime?  PickDate   { get; set; }
    public string?    User       { get; set; }
    public string?    Picker     { get; set; }
    public string     Status     { get; set; } = "Released";   // Released | Closed | Cancelled
    public string?    Remarks    { get; set; }

    public ICollection<PickListLine> Lines { get; set; } = new List<PickListLine>();
}

public class PickListLine : AuditableEntity
{
    public int      PickListId      { get; set; }
    public int      LineNum         { get; set; }
    public string?  TransType       { get; set; }   // "Sales Order"
    public string?  DocNo           { get; set; }
    public string?  BpCode          { get; set; }
    public DateTime? DeliveryDueDate { get; set; }
    public string?  ItemNo          { get; set; }
    public string?  Description     { get; set; }
    public string?  UomCode         { get; set; }
    public string?  UomName         { get; set; }
    public string?  WhsCode         { get; set; }
    public decimal  OrderQty        { get; set; }
    public decimal  OpenQty         { get; set; }
    public decimal  PickedQty       { get; set; }

    public PickList PickList { get; set; } = null!;
}

public class PickListConfig : IEntityTypeConfiguration<PickList>
{
    public void Configure(EntityTypeBuilder<PickList> b)
    {
        b.ToTable("KPCK");
        b.HasKey(x => x.Id);
        b.Property(m => m.PickNumber).HasMaxLength(30).IsRequired();
        b.HasIndex(m => m.PickNumber).IsUnique();
        b.Property(m => m.User).HasMaxLength(60);
        b.Property(m => m.Picker).HasMaxLength(60);
        b.Property(m => m.Status).HasMaxLength(15).HasDefaultValue("Released");
        b.Property(m => m.Remarks).HasMaxLength(500);
        b.HasIndex(m => m.Status);
    }
}

public class PickListLineConfig : IEntityTypeConfiguration<PickListLine>
{
    public void Configure(EntityTypeBuilder<PickListLine> b)
    {
        b.ToTable("PCK1");
        b.HasKey(x => x.Id);
        b.Property(m => m.TransType).HasMaxLength(40);
        b.Property(m => m.DocNo).HasMaxLength(40);
        b.Property(m => m.BpCode).HasMaxLength(40);
        b.Property(m => m.ItemNo).HasMaxLength(60);
        b.Property(m => m.Description).HasMaxLength(200);
        b.Property(m => m.UomCode).HasMaxLength(20);
        b.Property(m => m.UomName).HasMaxLength(40);
        b.Property(m => m.WhsCode).HasMaxLength(20);
        b.Property(m => m.OrderQty).HasPrecision(18, 4);
        b.Property(m => m.OpenQty).HasPrecision(18, 4);
        b.Property(m => m.PickedQty).HasPrecision(18, 4);

        b.HasOne(m => m.PickList)
            .WithMany(p => p.Lines)
            .HasForeignKey(m => m.PickListId)
            .OnDelete(DeleteBehavior.Cascade);
        b.HasIndex(m => m.PickListId);
    }
}
