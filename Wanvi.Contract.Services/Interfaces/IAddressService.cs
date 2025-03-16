using Wanvi.Contract.Repositories.Entities;
using Wanvi.ModelViews.VietMapModelViews;

namespace Wanvi.Contract.Services.Interfaces
{
    public interface IAddressService
    {
        Task<Address> GetOrCreateAddressAsync(double latitude, double longitude);
        Task<string> GetStreetFromCoordinatesAsync(double latitude, double longitude);
        Task<IEnumerable<ResponseAutocompleteModel>> SearchAsync(string query);
        Task<ResponsePlaceModel> GetOrCreateAddressByRefIdAsync(string refId);
    }
}
