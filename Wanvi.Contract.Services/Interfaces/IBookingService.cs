using Wanvi.ModelViews.BookingModelViews;

namespace Wanvi.Contract.Services.Interfaces
{
    public interface IBookingService
    {
        Task<string> CreateBookingHaft(CreateBookingModel model);
        Task<string> CreateBookingAll(CreateBookingModel model);
        Task<List<GetBookingUserModel>> GetBookingUser(
            string? searchNote = null,
            string? sortBy = null,
            bool isAscending = false,
            string? status = null);
        Task<List<GetBookingUserDetailModel>> GetBookingsByTourGuide(
            string? rentalDate = null,
            string? status = null,
            string? scheduleId = null,
            int? minTravelers = null,
            int? maxTravelers = null);
        Task<List<GetBookingUserModel>> GetBookingAdmin(
            string? searchNote = null,
            string? sortBy = null,
            bool isAscending = false,
            string? status = null);
        Task<GetBookingGuideModel> GetBookingSummaryBySchedule(
    string scheduleId,
    string rentalDate,
    string? status = null,
    int? minPrice = null,
    int? maxPrice = null,
    string sortBy = "CustomerName",
    bool ascending = true);
        Task<GetBookingGuideScreen3Model> GetBookingDetailsById(string bookingId);
        Task<string> WithdrawMoneyFromBooking(WithdrawMoneyFromBookingModel model);
        Task<string> ChangeBookingToUser(ChangeBookingToUserModel model);
        Task<string> CancelBookingForGuide(CancelBookingForGuideModel model);
        Task<string> CancelBookingForCustomer(CancelBookingForCustomerModel model);
        Task<string> CancelBookingForAdmin(CancelBookingForAdminModel model);
        Task<BookingStatisticsModel> BookingStatistics(string? day, string? month, int? year);
        Task<TotalRevenueModel> TotalRevenue(string? day, string? month, int? year);
    }

}
