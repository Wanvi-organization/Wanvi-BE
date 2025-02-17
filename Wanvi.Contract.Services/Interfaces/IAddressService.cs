using Wanvi.Contract.Repositories.Entities;

namespace Wanvi.Contract.Services.Interfaces
{
    public interface IAddressService
    {
        Task<Address> GetOrCreateAddressAsync(double latitude, double longitude);
        Task<string> GetStreetFromCoordinatesAsync(double latitude, double longitude);
    }
}
