using FarmingApi.Core;

namespace FarmingApi.Modules.Administration.TaxCode;

public interface ITaxCodeRepository : IRepository<TaxCodeEntry> { }
public class TaxCodeRepository : Repository<TaxCodeEntry>, ITaxCodeRepository
{
    public TaxCodeRepository(MyDbContext context) : base(context) { }
}
