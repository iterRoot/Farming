using AutoMapper;

namespace KamnttraApi.Modules.CRM.CustomerEquipmentCard;

public class CustomerEquipmentCardMapper : Profile
{
    public CustomerEquipmentCardMapper()
    {
        CreateMap<CustomerEquipmentCard, CustomerEquipmentCardResponse>();
    }
}
