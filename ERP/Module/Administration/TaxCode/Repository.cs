using KamnttraApi.Core;

namespace KamnttraApi.Modules.Administration.TaxCode;

public interface ITaxCodeRepository : IRepository<TaxCodeEntry> { }
public class TaxCodeRepository : Repository<TaxCodeEntry>, ITaxCodeRepository
{
    public TaxCodeRepository(MyDbContext context) : base(context) { }
}
