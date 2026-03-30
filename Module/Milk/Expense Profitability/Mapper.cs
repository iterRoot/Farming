using AutoMapper;

namespace FarmingApi.Modules.Expense ;

public class ExpenseMapper : Profile
{
	public ExpenseMapper()
	{
		CreateMap<Expense , ExpenseListResponse>();
        CreateMap<ExpenseListResponse, Expense >();

		CreateMap<ExpenseListRequest, Expense >();
		CreateMap<Expense , ExpenseListRequest>();

	}
}