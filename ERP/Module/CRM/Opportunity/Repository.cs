using FarmingApi.Core;

namespace FarmingApi.Modules.CRM.Opportunity;

public interface IOpportunityRepository : IRepository<Opportunity> { }

public class OpportunityRepository : Repository<Opportunity>, IOpportunityRepository
{
    public OpportunityRepository(MyDbContext ctx) : base(ctx) { }
}