using Wanvi.ModelViews.BookingModelViews;

namespace Wanvi.Contract.Services.Interfaces
{
    public interface IBookingService
    {
        Task<string> CreateBookingHaft(CreateBookingModel model);
        Task<string> CreateBookingAll(CreateBookingModel model);
    }
}
