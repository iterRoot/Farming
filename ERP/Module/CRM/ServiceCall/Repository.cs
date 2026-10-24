using FarmingApi.Core;

namespace FarmingApi.Modules.CRM.ServiceCall;

public interface IServiceCallRepository : IRepository<ServiceCall> { }

public class ServiceCallRepository : Repository<ServiceCall>, IServiceCallRepository
{
    public ServiceCallRepository(MyDbContext context) : base(context) { }
}
