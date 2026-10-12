using AutoMapper;

namespace FarmingApi.Modules.ProjectManagement.Project;

public class ProjectMapper : Profile
{
    public ProjectMapper()
    {
        // Only the flat list row is a straight map; the detail response is
        // assembled in the controller because the nested grids come out of
        // the Details JSON document.
        CreateMap<Project, ProjectListResponse>();
    }
}
