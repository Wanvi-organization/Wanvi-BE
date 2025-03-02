using Wanvi.ModelViews.BookingModelViews;

namespace Wanvi.Contract.Services.Interfaces
{
    public interface IBookingService
    {
        Task<string> CreateBookingHaft(CreateBookingModel model);
        Task<string> CreateBookingAll(CreateBookingModel model);
        Task<List<GetBookingUsermodel>> GetBookingUser(
            string? searchNote = null,
            string? sortBy = null,
            bool isAscending = false,
            string? status = null);
        Task<List<GetBookingGuideModel>> GetBookingsByTourGuide(
            string? rentalDate = null,
            string? status = null,
            string? scheduleId = null,
            int? minTravelers = null,
            int? maxTravelers = null,
            string? sortBy = "RentalDate",
            bool ascending = false);
        Task<List<GetBookingUsermodel>> GetBookingAdmin(
            string? searchNote = null,
            string? sortBy = null,
            bool isAscending = false,
            string? status = null);
        Task<string> WithdrawMoneyFromBooking(WithdrawMoneyFromBookingModel model);
        Task<string> ChangeBookingToUser(ChangeBookingToUserModel model);
        Task<string> CancelBookingForGuide(CancelBookingForGuideModel model);
        Task<string> CancelBookingForCustomer(CancelBookingForCustomerModel model);

    }

}
