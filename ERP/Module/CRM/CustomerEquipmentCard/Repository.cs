using KamnttraApi.Core;

namespace KamnttraApi.Modules.CRM.CustomerEquipmentCard;

public interface ICustomerEquipmentCardRepository : IRepository<CustomerEquipmentCard> { }

public class CustomerEquipmentCardRepository : Repository<CustomerEquipmentCard>, ICustomerEquipmentCardRepository
{
    public CustomerEquipmentCardRepository(MyDbContext context) : base(context) { }
}
