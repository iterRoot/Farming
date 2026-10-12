using FarmingApi.Core;

namespace FarmingApi.Modules.HumanResources.JobPosting;

public interface IJobPostingRepository : IRepository<JobPosting> { }

public class JobPostingRepository : Repository<JobPosting>, IJobPostingRepository
{
    public JobPostingRepository(MyDbContext context) : base(context) { }
}
