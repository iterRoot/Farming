using KamnttraApi.Core;

namespace KamnttraApi.Modules.Administration.MessagePreferences;

public interface IMessagePreferenceRepository
    : IRepository<MessagePreference> { }

public class MessagePreferenceRepository
    : Repository<MessagePreference>, IMessagePreferenceRepository
{
    public MessagePreferenceRepository(MyDbContext ctx) : base(ctx) { }
}