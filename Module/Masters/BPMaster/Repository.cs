using FarmingApi.Core;

namespace FarmingApi.Modules.Master.BusinessPartner;
public interface IBusinessPartnerRepository : IRepository<BusinessPartner>
{

}
public class BusinessPartnerRepository : Repository<BusinessPartner>,IBusinessPartnerRepository
{
    public BusinessPartnerRepository(MyDbContext context) : base(context)
    {
        
    }
}