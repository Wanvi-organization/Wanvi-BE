namespace Wanvi.Core.Utils
{
    public static class GeoHelper
    {
        private const double EarthRadiusKm = 6371.0; // Bán kính Trái đất tính bằng km

        /// <summary>
        /// Tính khoảng cách giữa hai tọa độ (vĩ độ và kinh độ) sử dụng công thức Haversine.
        /// </summary>
        /// <param name="lat1">Vĩ độ của điểm đầu tiên</param>
        /// <param name="lon1">Kinh độ của điểm đầu tiên</param>
        /// <param name="lat2">Vĩ độ của điểm thứ hai</param>
        /// <param name="lon2">Kinh độ của điểm thứ hai</param>
        /// <returns>Khoảng cách giữa hai điểm tính bằng km</returns>
        public static double GetDistance(double lat1, double lon1, double lat2, double lon2)
        {
            // Chuyển đổi từ độ sang radian
            double lat1Rad = ToRadians(lat1);
            double lon1Rad = ToRadians(lon1);
            double lat2Rad = ToRadians(lat2);
            double lon2Rad = ToRadians(lon2);

            // Công thức Haversine
            double deltaLat = lat2Rad - lat1Rad;
            double deltaLon = lon2Rad - lon1Rad;

            double a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
                       Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                       Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            // Khoảng cách cuối cùng
            return EarthRadiusKm * c;
        }

        /// <summary>
        /// Chuyển đổi độ sang radian.
        /// </summary>
        /// <param name="degree">Giá trị độ</param>
        /// <returns>Giá trị radian</returns>
        private static double ToRadians(double degree)
        {
            return degree * (Math.PI / 180.0);
        }
    }

}
