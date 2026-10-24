// using Microsoft.EntityFrameworkCore;
// using Microsoft.EntityFrameworkCore.Metadata.Builders;
// using FarmingApi.Core;


// namespace FarmingApi.Modules.Salad;

// public class Salad : AuditableEntity
// {
//     public string Sex { get; set; } = null!;
//     public int? Age { get; set; }
//     public string? Birthday { get; set; }
//     public string? Variety { get; set; }
//     public string? Desc { get; set; }

//     // public int? HouseId { get; set; }        
//     // public House.House? House { get; set; }        

// }


// public class SaladConfig : IEntityTypeConfiguration<Salad>
// {
//     public void Configure(EntityTypeBuilder<Salad> builder)
//     {
//         builder.ToTable("Salad");
//         builder.HasKey(x => x.Id);

//         builder.Property(m => m.Sex).HasMaxLength(100).IsRequired();
//         builder.Property(m => m.Birthday).HasMaxLength(20);
//         builder.Property(m => m.Variety).HasMaxLength(255);
//         builder.Property(m => m.Desc).HasMaxLength(200);

//         // builder.HasOne(s => s.House)
//         //     .WithMany(h => h.Salad)
//         //     .HasForeignKey(s => s.HouseId)
//         //     .OnDelete(DeleteBehavior.Restrict);
//     }
// }

