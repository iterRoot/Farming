using FarmingApi.Core;

namespace FarmingApi.Modules.Administration.Attachment;

public interface IAttachmentRepository : IRepository<Attachment> { }

public class AttachmentRepository : Repository<Attachment>, IAttachmentRepository
{
    public AttachmentRepository(MyDbContext context) : base(context) { }
}
