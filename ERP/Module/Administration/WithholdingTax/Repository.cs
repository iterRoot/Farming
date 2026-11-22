using KamnttraApi.Core;

namespace KamnttraApi.Modules.Administration.WithholdingTax;

public interface IWithholdingTaxCodeRepository : IRepository<WithholdingTaxCode> { }
public class WithholdingTaxCodeRepository : Repository<WithholdingTaxCode>, IWithholdingTaxCodeRepository
{
    public WithholdingTaxCodeRepository(MyDbContext context) : base(context) { }
}
