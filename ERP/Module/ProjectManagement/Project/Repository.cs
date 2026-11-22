using KamnttraApi.Core;

namespace KamnttraApi.Modules.ProjectManagement.Project;

public interface IProjectRepository : IRepository<Project> { }

public class ProjectRepository : Repository<Project>, IProjectRepository
{
    public ProjectRepository(MyDbContext context) : base(context) { }
}
