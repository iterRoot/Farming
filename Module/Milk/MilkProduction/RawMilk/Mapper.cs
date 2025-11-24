using AutoMapper;

namespace FarmingApi.Modules.RawMilk;

public class RawMilkMapper : Profile
{
	public RawMilkMapper()
	{
		// CreateMap<RawMilkResponse, RawMilk>();
        		CreateMap<RawMilk, RawMilkResponse >();


           
	}
}