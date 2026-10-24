// ═══════════════════════════════════════════════════════════════
// FILE: Repository.cs
// ═══════════════════════════════════════════════════════════════
using FarmingApi.Core;

namespace FarmingApi.Modules.Financials.GLAccountDetermination;

public interface IGLAccountDeterminationRepository
    : IRepository<GLAccountDetermination> { }

public class GLAccountDeterminationRepository
    : Repository<GLAccountDetermination>, IGLAccountDeterminationRepository
{
    public GLAccountDeterminationRepository(MyDbContext context) : base(context) { }
}
