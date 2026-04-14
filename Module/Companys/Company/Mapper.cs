using AutoMapper;

namespace FarmingApi.Modules.Company;

public class CompanyMapper : Profile
{
	public CompanyMapper()
	{
		CreateMap<Company, CompanyListResponse>();
		CreateMap<Company, CompanyDetailResponse>();
		CreateMap<CompanyInsertRequest, Company>();
		CreateMap<CompanyUpdateRequest, Company>()
			.ForMember(e => e.Logo, opt => opt.Ignore());
	}
}