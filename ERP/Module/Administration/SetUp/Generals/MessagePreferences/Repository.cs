using FarmingApi.Core;

namespace FarmingApi.Modules.Administration.MessagePreferences;

public interface IMessagePreferenceRepository
    : IRepository<MessagePreference> { }

public class MessagePreferenceRepository
    : Repository<MessagePreference>, IMessagePreferenceRepository
{
    public MessagePreferenceRepository(MyDbContext ctx) : base(ctx) { }
}