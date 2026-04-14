using AutoMapper;

namespace FarmingApi.Modules.Company.Staff;

public class StaffMapper : Profile
{
	public StaffMapper()
	{
		CreateMap<Staff, CompanyListResponse>();
		CreateMap<Staff, CompanyDetailResponse>();
		CreateMap<StaffInsertRequest, Staff>();
		CreateMap<StaffUpdateRequest, Staff>();
        }

}