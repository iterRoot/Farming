using KamnttraApi.Core;

namespace KamnttraApi.Modules.CRM.ServiceContract;

public interface IServiceContractRepository : IRepository<ServiceContract> { }

public class ServiceContractRepository : Repository<ServiceContract>, IServiceContractRepository
{
    public ServiceContractRepository(MyDbContext context) : base(context) { }
}
