

// ═══════════════════════════════════════════════════════════════
// Repository.cs
// ═══════════════════════════════════════════════════════════════
using FarmingApi.Core;

namespace FarmingApi.Modules.Inventory.LengthWidth;

public interface ILengthWidthUomRepository : IRepository<LengthWidthUom> { }

public class LengthWidthUomRepository
    : Repository<LengthWidthUom>, ILengthWidthUomRepository
{
    public LengthWidthUomRepository(MyDbContext context) : base(context) { }
}

