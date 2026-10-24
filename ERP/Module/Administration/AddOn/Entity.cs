// using FarmingApi.Core;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.EntityFrameworkCore.Metadata.Builders;
//
//
// namespace FarmingApi.Modules.Administration.AddOn;
//
// public class AddOn : AuditableEntity
// {
//     public int Id { get; set; }
//     
// }
//
// public class AddOnConfig : IEntityTypeConfiguration<AddOn>
// {
//     public void Configure(EntityTypeBuilder<AddOn> builder)
//     {
//         builder.ToTable("TTTT");
//         builder.HasKey(x => x.Id);
//
//     }
// }