using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FarmingApi.Core;

namespace FarmingApi.Modules.Administration.GeneralSettings;

// Singleton settings record for the SAP-style "General Settings" screen.
//
// The screen holds ~170 flags spread over 14 tabs and is always read and
// written as a whole — nothing filters or joins on an individual setting.
// Storing it as one jsonb document keeps the schema stable: adding a new
// setting to a tab needs no migration.
public class GeneralSetting : AuditableEntity
{
    public string Settings { get; set; } = "{}";
}

public class GeneralSettingConfig : IEntityTypeConfiguration<GeneralSetting>
{
    public void Configure(EntityTypeBuilder<GeneralSetting> builder)
    {
        builder.ToTable("KGST");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Settings).HasColumnType("jsonb").IsRequired();
    }
}
