namespace MERRICK.DatabaseContext.Entities.Telemetry;

public class GeoName
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int ID { get; set; }

    public required string Name { get; set; }

    public Dictionary<string, string> Names { get; set; } = [];

    public string? Code { get; set; } = null;

    public string? ISOCode { get; set; } = null;

    public bool? IsInEuropeanUnion { get; set; } = null;
}

