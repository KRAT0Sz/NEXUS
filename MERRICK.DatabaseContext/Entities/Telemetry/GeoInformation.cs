namespace MERRICK.DatabaseContext.Entities.Telemetry;

public class GeoInformation
{
    [Key]
    [MaxLength(45)]
    public required string IPAddress { get; set; }

    public int? CityID { get; set; } = null;

    public GeoName? City { get; set; } = null;

    public int? ContinentID { get; set; } = null;

    public GeoName? Continent { get; set; } = null;

    public int? CountryID { get; set; } = null;

    public GeoName? Country { get; set; } = null;

    public float Latitude { get; set; } = 0.0f;

    public float Longitude { get; set; } = 0.0f;

    public string TimeZone { get; set; } = string.Empty;

    public string? PostalCode { get; set; } = null;

    public List<GeoName> Subdivisions { get; set; } = [];
}

