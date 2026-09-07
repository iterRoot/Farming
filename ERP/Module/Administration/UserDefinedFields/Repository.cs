using FarmingApi.Core;

namespace FarmingApi.Modules.Administration.UserDefinedFields;

public interface IUserDefinedFieldRepository : IRepository<UserDefinedField>
{
}

public class UserDefinedFieldRepository : Repository<UserDefinedField>, IUserDefinedFieldRepository
{
    public UserDefinedFieldRepository(MyDbContext context) : base(context)
    {
    }
}
