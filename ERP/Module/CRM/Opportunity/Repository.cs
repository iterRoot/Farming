using KamnttraApi.Core;

namespace KamnttraApi.Modules.CRM.Opportunity;

public interface IOpportunityRepository : IRepository<Opportunity> { }

public class OpportunityRepository : Repository<Opportunity>, IOpportunityRepository
{
    public OpportunityRepository(MyDbContext ctx) : base(ctx) { }
}