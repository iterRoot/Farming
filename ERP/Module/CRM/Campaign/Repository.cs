using KamnttraApi.Core;

namespace KamnttraApi.Modules.CRM.Campaign;

public interface ICampaignRepository : IRepository<Campaign> { }

public class CampaignRepository : Repository<Campaign>, ICampaignRepository
{
    public CampaignRepository(MyDbContext context) : base(context) { }
}
