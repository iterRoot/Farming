using FarmingApi.Core;

namespace FarmingApi.Modules.Sale.Test;

public interface ITestRepository : IRepository<Test>
{
}

public class TestRepository : Repository<Test>, ITestRepository
{
    public TestRepository(MyDbContext context) : base(context)
    {
    }
}