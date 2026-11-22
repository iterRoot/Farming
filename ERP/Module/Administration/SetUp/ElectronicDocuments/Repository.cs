using KamnttraApi.Core;

namespace KamnttraApi.Modules.Administration.ElectronicDocuments;

public interface IElectronicDocumentRepository
    : IRepository<ElectronicDocument> { }

public class ElectronicDocumentRepository
    : Repository<ElectronicDocument>, IElectronicDocumentRepository
{
    public ElectronicDocumentRepository(MyDbContext ctx) : base(ctx) { }
}