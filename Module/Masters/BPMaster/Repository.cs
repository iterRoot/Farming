using FarmingApi.Core;

namespace FarmingApi.Modules.Master.UserMaster;
public interface IUserMasterRepository : IRepository<UserMaster>
{

}
public class UserMasterRepository : Repository<UserMaster>,IUserMasterRepository
{
    public UserMasterRepository(MyDbContext context) : base(context)
    {
        
    }
}