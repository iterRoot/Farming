using AutoMapper;

namespace FarmingApi.Modules.CRM.KnowledgeBase;

public class KnowledgeBaseArticleMapper : Profile
{
    public KnowledgeBaseArticleMapper()
    {
        CreateMap<KnowledgeBaseArticle, KnowledgeBaseArticleResponse>();
    }
}
