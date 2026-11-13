namespace MERRICK.DatabaseContext.Entities.Economy;

public class DailySpecial
{
    [Key]
    public int ID { get; set; }

    public int PanelIndex { get; set; }

    public required string Name { get; set; }

    public int DiscountOff { get; set; }

    public int DiscountSilver { get; set; }

    public string TagTitle { get; set; } = string.Empty;

    public string TagDateLimit { get; set; } = string.Empty;
}

