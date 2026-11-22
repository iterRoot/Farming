using KamnttraApi.Core;

namespace KamnttraApi.Modules.Administration.EmailSettings;

public interface IEmailAccountRepository : IRepository<EmailAccount> { }
public class EmailAccountRepository : Repository<EmailAccount>, IEmailAccountRepository
{
    public EmailAccountRepository(MyDbContext context) : base(context) { }
}
