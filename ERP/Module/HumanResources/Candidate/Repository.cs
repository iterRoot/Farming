using FarmingApi.Core;

namespace FarmingApi.Modules.HumanResources.Candidate;

public interface ICandidateRepository : IRepository<Candidate> { }

public class CandidateRepository : Repository<Candidate>, ICandidateRepository
{
    public CandidateRepository(MyDbContext context) : base(context) { }
}
