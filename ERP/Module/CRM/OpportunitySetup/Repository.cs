using FarmingApi.Core;

namespace FarmingApi.Modules.CRM.OpportunitySetup;

public interface IOpportunityStageSetupRepository : IRepository<OpportunityStageSetup> { }
public class OpportunityStageSetupRepository : Repository<OpportunityStageSetup>, IOpportunityStageSetupRepository
{
    public OpportunityStageSetupRepository(MyDbContext ctx) : base(ctx) { }
}

public interface IOpportunityReasonSetupRepository : IRepository<OpportunityReasonSetup> { }
public class OpportunityReasonSetupRepository : Repository<OpportunityReasonSetup>, IOpportunityReasonSetupRepository
{
    public OpportunityReasonSetupRepository(MyDbContext ctx) : base(ctx) { }
}

public interface ICompetitorMasterRepository : IRepository<CompetitorMaster> { }
public class CompetitorMasterRepository : Repository<CompetitorMaster>, ICompetitorMasterRepository
{
    public CompetitorMasterRepository(MyDbContext ctx) : base(ctx) { }
}
