using FarmingApi.Core;

namespace FarmingApi.Modules.Administration.ElectronicDocuments;

public interface IElectronicDocumentRepository
    : IRepository<ElectronicDocument> { }

public class ElectronicDocumentRepository
    : Repository<ElectronicDocument>, IElectronicDocumentRepository
{
    public ElectronicDocumentRepository(MyDbContext ctx) : base(ctx) { }
}