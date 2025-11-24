using AutoMapper;

namespace FarmingApi.Modules.Production;

public class ProductionMapper : Profile
{
	public ProductionMapper()
	{
		CreateMap<CreateProduction, Production>();
		CreateMap<ProductionResponse, Production>();

	}
}