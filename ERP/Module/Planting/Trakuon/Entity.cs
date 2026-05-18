// using Microsoft.EntityFrameworkCore;
// using Microsoft.EntityFrameworkCore.Metadata.Builders;
// using FarmingApi.Core;


// namespace FarmingApi.Modules.Trakuons;

// public class Trakuons : AuditableEntity
// {
//     // public DateTime PlantingDate { get; set; }
//     // public int? Age { get; set; }
//     // public int CirclePlanting { get; set; }
//     // public DateTime EndDate { get; set; }
//     // public bool Finish {get;set;}

//     public string CropName { get; set; }
//     public DateTime StartDate { get; set; }
//     public int AgeDays { get; set; }
//     public int CirclePlanting { get; set; }
//     public DateTime EndDate { get; set; }
//     public string? Desc { get; set; }
// // 
//     // public int? HouseId { get; set; }        // optional FK
//     // public House.House? House { get; set; }        // optional navigation


// }


// public class TrakuonsConfig : IEntityTypeConfiguration<Trakuons>
// {
//     public void Configure(EntityTypeBuilder<Trakuons> builder)
//     {
//         builder.ToTable("Trakuons");
//         builder.HasKey(x => x.Id);

//         builder.Property(m => m.StartDate).HasMaxLength(100).IsRequired();
//         builder.Property(m => m.AgeDays).HasMaxLength(20);
//         builder.Property(m => m.CirclePlanting).HasMaxLength(255);
//         builder.Property(m => m.EndDate).HasMaxLength(200);
//                 builder.Property(m => m.CropName).HasMaxLength(10);

//         builder.Property(m => m.Desc).HasMaxLength(200);


//         // builder.HasOne(s => s.House)
//         //     .WithMany(h => h.Trakuons)
//         //     .HasForeignKey(s => s.HouseId)
//         //     .OnDelete(DeleteBehavior.Restrict);
//     }
// }

