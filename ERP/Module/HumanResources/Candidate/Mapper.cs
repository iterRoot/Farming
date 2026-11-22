using AutoMapper;

namespace KamnttraApi.Modules.HumanResources.Candidate;

public class CandidateMapper : Profile
{
    public CandidateMapper()
    {
        CreateMap<Candidate, CandidateResponse>();
    }
}
