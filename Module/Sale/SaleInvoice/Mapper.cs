using AutoMapper;

namespace FarmingApi.Modules.Sale.SaleInvoice;

public class SaleInvoiceMapper : Profile
{
	public SaleInvoiceMapper()
	{
		CreateMap<SaleInvoice, SaleInvoiceListResponse>();
        CreateMap<SaleInvoiceListResponse, SaleInvoice>();

		CreateMap<SaleInvoiceListRequest, SaleInvoice>();
		CreateMap<SaleInvoice, SaleInvoiceListRequest>();
		// SaleInvoiceUpdateRequest

		CreateMap<SaleInvoiceUpdateRequest, SaleInvoice>();


	}
}