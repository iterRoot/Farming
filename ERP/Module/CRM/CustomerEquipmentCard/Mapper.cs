using AutoMapper;

namespace FarmingApi.Modules.CRM.CustomerEquipmentCard;

public class CustomerEquipmentCardMapper : Profile
{
    public CustomerEquipmentCardMapper()
    {
        CreateMap<CustomerEquipmentCard, CustomerEquipmentCardResponse>();
    }
}
