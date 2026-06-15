using FarmingApi.Core;

namespace FarmingApi.Modules.Administration.AddressFormats;

public interface IAddressFormatRepository : IRepository<AddressFormat> { }

public class AddressFormatRepository
    : Repository<AddressFormat>, IAddressFormatRepository
{
    public AddressFormatRepository(MyDbContext ctx) : base(ctx) { }
}