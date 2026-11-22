using KamnttraApi.Core;

namespace KamnttraApi.Modules.CRM.ServiceCall;

public interface IServiceCallRepository : IRepository<ServiceCall> { }

public class ServiceCallRepository : Repository<ServiceCall>, IServiceCallRepository
{
    public ServiceCallRepository(MyDbContext context) : base(context) { }
}
