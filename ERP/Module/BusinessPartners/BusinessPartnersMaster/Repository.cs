using FarmingApi.Core;

namespace FarmingApi.Modules.BusinessPartners.BusinessPartnersMaster;

public interface IBusinessPartnersMasterRepository : IRepository<BusinessPartnersMaster>
{
}

public class BusinessPartnersMasterRepository : Repository<BusinessPartnersMaster>, IBusinessPartnersMasterRepository
{
    public BusinessPartnersMasterRepository(MyDbContext context) : base(context)
    {
    }
}