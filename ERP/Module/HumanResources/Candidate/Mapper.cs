using AutoMapper;

namespace FarmingApi.Modules.HumanResources.Candidate;

public class CandidateMapper : Profile
{
    public CandidateMapper()
    {
        CreateMap<Candidate, CandidateResponse>();
    }
}
