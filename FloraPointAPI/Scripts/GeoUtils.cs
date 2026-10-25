namespace FloraPointAPI.Scripts
{
    public static class GeoUtils
    {

        //wzór Haversine'a na odleglosc miedzy punktami
        public static double CalculateDistanceMeters(double lat1, double lon1, double lat2, double lon2)
        {
            double r = 6371000; // promien  ziemi
            double dLat = ToRadians(lat2 - lat1);
            double dLon = ToRadians(lon2 - lon1);

            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return r * c;
        }

        public static double ToRadians(double val)
        {
            return (Math.PI / 180.0) * val;
        }
    }
}