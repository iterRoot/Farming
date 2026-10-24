using FarmingApi.Core;

namespace FarmingApi.Modules.Administration.EmailSettings;

public interface IEmailAccountRepository : IRepository<EmailAccount> { }
public class EmailAccountRepository : Repository<EmailAccount>, IEmailAccountRepository
{
    public EmailAccountRepository(MyDbContext context) : base(context) { }
}
