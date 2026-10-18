namespace FloraPointAPI.DTOs
{
    public class AreaSearchDto
    {
        // granice obszaru do przeszukania
        public double MinLatitude { get; set; }
        public double MaxLatitude { get; set; }
        public double MinLongitude { get; set; }
        public double MaxLongitude { get; set; }

        // opcjonalna lista id roślin których szukać
        public List<string>? PlantIds { get; set; }
    }
}