using FarmingApi.Core;

namespace FarmingApi.Modules.CRM.Campaign;

public interface ICampaignRepository : IRepository<Campaign> { }

public class CampaignRepository : Repository<Campaign>, ICampaignRepository
{
    public CampaignRepository(MyDbContext context) : base(context) { }
}
