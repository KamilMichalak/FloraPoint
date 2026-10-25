namespace FloraPointAPI.DTOs
{
    public class RadiusSearchDto
    {
        public double CenterLat { get; set; }
        public double CenterLng { get; set; }
        public double Radius { get; set; }
        public List<string>? PlantIds { get; set; }
    }
}