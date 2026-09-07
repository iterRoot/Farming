using FarmingApi.Core;

namespace FarmingApi.Modules.Administration.PostingPeriods;

public interface IPostingPeriodRepository : IRepository<PostingPeriod> { }

public class PostingPeriodRepository : Repository<PostingPeriod>, IPostingPeriodRepository
{
    public PostingPeriodRepository(MyDbContext context) : base(context) { }
}

public interface IPostingPeriodSettingRepository : IRepository<PostingPeriodSetting> { }

public class PostingPeriodSettingRepository : Repository<PostingPeriodSetting>, IPostingPeriodSettingRepository
{
    public PostingPeriodSettingRepository(MyDbContext context) : base(context) { }
}
