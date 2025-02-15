using Wanvi.ModelViews.BookingModelViews;

namespace Wanvi.Contract.Services.Interfaces
{
    public interface IBookingService
    {
        Task<string> CreateBooking(CreateBookingModel model);
    }
}
