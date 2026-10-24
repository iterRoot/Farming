using FarmingApi.Core;

namespace FarmingApi.Modules.Administration.ElectronicFileManager;

public interface IElectronicFileRepository
    : IRepository<ElectronicFile> { }

public class ElectronicFileRepository
    : Repository<ElectronicFile>, IElectronicFileRepository
{
    public ElectronicFileRepository(MyDbContext ctx) : base(ctx) { }
}