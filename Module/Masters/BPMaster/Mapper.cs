using AutoMapper;
namespace FarmingApi.Modules.Master.BusinessPartner;
public class BusinessPartnerMapper : Profile 
{
    public BusinessPartnerMapper()
    {
        CreateMap<BusinessPartner, BusinessPartnerResponse>();
        CreateMap<BusinessPartnerCreateRequest, BusinessPartner>();
        CreateMap<BusinessPartnerUpdateRequest, BusinessPartner>();
    }
}