using AutoMapper;

namespace KamnttraApi.Modules.HumanResources.JobPosting;

public class JobPostingMapper : Profile
{
    public JobPostingMapper()
    {
        CreateMap<JobPosting, JobPostingResponse>();
    }
}
