namespace Wanvi.Core.Utils
{
    public static class GeoHelper
    {
        private const double EarthRadiusKm = 6371.0088; // Bán kính trung bình của Trái Đất theo ellipsoid WGS-84

        /// <summary>
        /// Tính khoảng cách giữa hai tọa độ (vĩ độ và kinh độ) sử dụng công thức Vincenty.
        /// </summary>
        public static double GetDistance(double lat1, double lon1, double lat2, double lon2)
        {
            // Chuyển đổi độ sang radian
            double lat1Rad = ToRadians(lat1);
            double lon1Rad = ToRadians(lon1);
            double lat2Rad = ToRadians(lat2);
            double lon2Rad = ToRadians(lon2);

            double deltaLon = lon2Rad - lon1Rad;

            double U1 = Math.Atan((1 - 0.08181919) * Math.Tan(lat1Rad));
            double U2 = Math.Atan((1 - 0.08181919) * Math.Tan(lat2Rad));

            double sinU1 = Math.Sin(U1), cosU1 = Math.Cos(U1);
            double sinU2 = Math.Sin(U2), cosU2 = Math.Cos(U2);

            double lambda = deltaLon, lambdaP;
            double sinLambda, cosLambda, sinSigma, cosSigma, sigma, sinAlpha, cos2Alpha, cos2SigmaM, C;
            int iterLimit = 100;

            do
            {
                sinLambda = Math.Sin(lambda);
                cosLambda = Math.Cos(lambda);
                sinSigma = Math.Sqrt((cosU2 * sinLambda) * (cosU2 * sinLambda) +
                                     (cosU1 * sinU2 - sinU1 * cosU2 * cosLambda) * (cosU1 * sinU2 - sinU1 * cosU2 * cosLambda));
                if (sinSigma == 0) return 0; // Trùng tọa độ
                cosSigma = sinU1 * sinU2 + cosU1 * cosU2 * cosLambda;
                sigma = Math.Atan2(sinSigma, cosSigma);
                sinAlpha = cosU1 * cosU2 * sinLambda / sinSigma;
                cos2Alpha = 1 - sinAlpha * sinAlpha;
                cos2SigmaM = (cos2Alpha != 0) ? (cosSigma - 2 * sinU1 * sinU2 / cos2Alpha) : 0;
                C = 0.0033528107 / 16 * cos2Alpha * (4 + 0.0033528107 * (4 - 3 * cos2Alpha));
                lambdaP = lambda;
                lambda = deltaLon + (1 - C) * 0.0033528107 * sinAlpha *
                         (sigma + C * sinSigma * (cos2SigmaM + C * cosSigma * (-1 + 2 * cos2SigmaM * cos2SigmaM)));
            } while (Math.Abs(lambda - lambdaP) > 1e-12 && --iterLimit > 0);

            if (iterLimit == 0) return GetDistanceHaversine(lat1, lon1, lat2, lon2); // Dự phòng nếu không hội tụ

            double uSq = cos2Alpha * (6378137.0 * 6378137.0 - 6356752.314245 * 6356752.314245) / (6356752.314245 * 6356752.314245);
            double A = 1 + uSq / 16384 * (4096 + uSq * (-768 + uSq * (320 - 175 * uSq)));
            double B = uSq / 1024 * (256 + uSq * (-128 + uSq * (74 - 47 * uSq)));

            double deltaSigma = B * sinSigma * (cos2SigmaM + B / 4 * (cosSigma * (-1 + 2 * cos2SigmaM * cos2SigmaM) -
                                   B / 6 * cos2SigmaM * (-3 + 4 * sinSigma * sinSigma) * (-3 + 4 * cos2SigmaM * cos2SigmaM)));

            return 6356752.314245 * A * (sigma - deltaSigma) / 1000.0; // Kết quả trả về theo km
        }

        /// <summary>
        /// Dự phòng: Tính khoảng cách giữa hai tọa độ sử dụng công thức Haversine.
        /// </summary>
        public static double GetDistanceHaversine(double lat1, double lon1, double lat2, double lon2)
        {
            double lat1Rad = ToRadians(lat1);
            double lon1Rad = ToRadians(lon1);
            double lat2Rad = ToRadians(lat2);
            double lon2Rad = ToRadians(lon2);

            double deltaLat = lat2Rad - lat1Rad;
            double deltaLon = lon2Rad - lon1Rad;

            double a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
                       Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                       Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return EarthRadiusKm * c;
        }

        /// <summary>
        /// Chuyển đổi độ sang radian.
        /// </summary>
        private static double ToRadians(double degree)
        {
            return degree * (Math.PI / 180.0);
        }
    }
}
