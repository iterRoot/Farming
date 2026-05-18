// using Microsoft.EntityFrameworkCore;
// using Microsoft.EntityFrameworkCore.Metadata.Builders;
// using FarmingApi.Core;
// using System.ComponentModel.DataAnnotations.Schema;
// using FarmingApi.Modules.Inventory.ItemsMaster;

// namespace FarmingApi.Modules.Sale.StockMovement;

// public class StockMovement : AuditableEntity
// {
//     public string ItemsCode { get; set; } = null!;
//     public string WhsCode { get; set; }
//     public string TransType { get; set; }
//     public decimal Qty { get; set; }
//     public DateTime TransDate { get; set; }
//     public string? RefNo { get; set; }
//     public string FromWhs { get; set; }
//     public string ToWhs { get; set; }
//     [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
//     public int TransId { get; set; }

// }

// public class StockMovementConfig : IEntityTypeConfiguration<StockMovement>
// {
//     public void Configure(EntityTypeBuilder<StockMovement> builder)
//     {
//         builder.ToTable("StockMovement");

//         builder.HasKey(x => x.Id);
//         builder.HasKey(x => x.TransId);
//         builder.Property(m => m.ItemsCode)
//             .HasMaxLength(100)
//             .IsRequired();

//         builder.Property(m => m.TransType)
//             .HasMaxLength(255);

//         builder.Property(m => m.Qty)
//             .HasMaxLength(200);

//         builder.Property(m => m.TransDate)
//             .HasMaxLength(50);

//         builder.Property(m => m.RefNo)
//             .HasMaxLength(200);

//         builder.Property(m => m.ToWhs)
//             .HasMaxLength(200);

//         builder.Property(m => m.FromWhs)
//             .HasMaxLength(500);

//     }
// }