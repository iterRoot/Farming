using KamnttraApi.Core;

namespace KamnttraApi.Modules.CRM.KnowledgeBase;

public interface IKnowledgeBaseArticleRepository : IRepository<KnowledgeBaseArticle> { }

public class KnowledgeBaseArticleRepository : Repository<KnowledgeBaseArticle>, IKnowledgeBaseArticleRepository
{
    public KnowledgeBaseArticleRepository(MyDbContext context) : base(context) { }
}
