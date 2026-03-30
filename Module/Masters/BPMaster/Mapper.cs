using AutoMapper;
namespace FarmingApi.Modules.Master.UserMaster;
public class UserMasterMapper : Profile 
{
    public UserMasterMapper()
    {
        CreateMap<UserMaster, UserMasterInsertrequest>();
        CreateMap<UserMasterInsertrequest, UserMaster>();

    }
}