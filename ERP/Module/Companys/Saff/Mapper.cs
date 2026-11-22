using AutoMapper;

namespace KamnttraApi.Modules.Company.Staff;

public class StaffMapper : Profile
{
	public StaffMapper()
	{
		CreateMap<Staff, StaffListResponse>();
		CreateMap<StaffInsertRequest, Staff>();
		CreateMap<StaffUpdateRequest, Staff>();
        }

}