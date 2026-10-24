using AutoMapper;

namespace FarmingApi.Modules.HumanResources.JobPosting;

public class JobPostingMapper : Profile
{
    public JobPostingMapper()
    {
        CreateMap<JobPosting, JobPostingResponse>();
    }
}
