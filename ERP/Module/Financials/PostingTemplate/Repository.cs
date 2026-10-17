using FarmingApi.Core;

namespace FarmingApi.Modules.Financials.PostingTemplate;

public interface IPostingTemplateRepository : IRepository<PostingTemplate> { }

public class PostingTemplateRepository : Repository<PostingTemplate>, IPostingTemplateRepository
{
    public PostingTemplateRepository(MyDbContext context) : base(context) { }
}
