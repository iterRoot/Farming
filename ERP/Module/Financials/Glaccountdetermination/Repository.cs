// ═══════════════════════════════════════════════════════════════
// FILE: Repository.cs
// ═══════════════════════════════════════════════════════════════
using KamnttraApi.Core;

namespace KamnttraApi.Modules.Financials.GLAccountDetermination;

public interface IGLAccountDeterminationRepository
    : IRepository<GLAccountDetermination> { }

public class GLAccountDeterminationRepository
    : Repository<GLAccountDetermination>, IGLAccountDeterminationRepository
{
    public GLAccountDeterminationRepository(MyDbContext context) : base(context) { }
}
