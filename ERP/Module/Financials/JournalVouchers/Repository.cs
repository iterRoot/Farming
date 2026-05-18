using FarmingApi.Core;

namespace FarmingApi.Modules.Financials.JournalVoucher;

public interface IJournalVoucherRepository : IRepository<JournalVoucher>
{
}

public class JournalVoucherRepository : Repository<JournalVoucher>, IJournalVoucherRepository
{
    public JournalVoucherRepository(MyDbContext context) : base(context)
    {
    }
}