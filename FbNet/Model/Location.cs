namespace FbNet.Model
{
    public class Location
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string CountryCode { get; set; }
        public float Latitude { get; set; }
        public float Longitude { get; set; }
        public string Region { get; set; }
        public long RegionId { get; set; }
    }
}