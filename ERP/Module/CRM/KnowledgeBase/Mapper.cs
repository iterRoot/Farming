using AutoMapper;

namespace KamnttraApi.Modules.CRM.KnowledgeBase;

public class KnowledgeBaseArticleMapper : Profile
{
    public KnowledgeBaseArticleMapper()
    {
        CreateMap<KnowledgeBaseArticle, KnowledgeBaseArticleResponse>();
    }
}
