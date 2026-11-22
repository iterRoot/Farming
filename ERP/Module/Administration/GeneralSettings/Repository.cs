using KamnttraApi.Core;

namespace KamnttraApi.Modules.Administration.GeneralSettings;

public interface IGeneralSettingRepository : IRepository<GeneralSetting>
{
}

public class GeneralSettingRepository : Repository<GeneralSetting>, IGeneralSettingRepository
{
    public GeneralSettingRepository(MyDbContext context) : base(context)
    {
    }
}
