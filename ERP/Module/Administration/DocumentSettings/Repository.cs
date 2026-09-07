using FarmingApi.Core;

namespace FarmingApi.Modules.Administration.DocumentSettings;

public interface IDocumentGeneralSettingRepository : IRepository<DocumentGeneralSetting> { }
public class DocumentGeneralSettingRepository : Repository<DocumentGeneralSetting>, IDocumentGeneralSettingRepository
{
    public DocumentGeneralSettingRepository(MyDbContext context) : base(context) { }
}

public interface IDocumentTypeSettingRepository : IRepository<DocumentTypeSetting> { }
public class DocumentTypeSettingRepository : Repository<DocumentTypeSetting>, IDocumentTypeSettingRepository
{
    public DocumentTypeSettingRepository(MyDbContext context) : base(context) { }
}
