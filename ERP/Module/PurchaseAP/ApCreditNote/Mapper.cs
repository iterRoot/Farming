using AutoMapper;

namespace FarmingApi.Modules.PurchaseAP.APCreditNote;

public class APCreditNoteMapper : Profile
{
    public APCreditNoteMapper()
    {
        CreateMap<APCreditNoteRequest, APCreditNote>()
            .ForMember(dest => dest.Vendor, opt => opt.Ignore())
            .ForMember(dest => dest.Items,  opt => opt.MapFrom(src => src.Items));

        CreateMap<APCreditNoteLineRequest, APCreditNoteLine>()
            .ForMember(dest => dest.Item, opt => opt.Ignore())
            .ForMember(dest => dest.ItemCode,     opt => opt.Ignore())
            .ForMember(dest => dest.ItemName,     opt => opt.Ignore())
            .ForMember(dest => dest.BaseInvoice,  opt => opt.Ignore());

        CreateMap<APCreditNote, APCreditNoteResponse>()
            .ForMember(dest => dest.VendorCode, opt => opt.MapFrom(src => src.Vendor.Code))
                   .ForMember(dest => dest.VendorName, opt => opt.MapFrom(src =>src.Vendor.CardName))

            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));

        CreateMap<APCreditNoteLine, APCreditNoteLineResponse>();

        CreateMap<APCreditNoteUpdateRequest, APCreditNote>()
            .ForMember(dest => dest.Id,        opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Vendor,    opt => opt.Ignore())
            .ForMember(dest => dest.Items,     opt => opt.MapFrom(src => src.Items));
    }
}