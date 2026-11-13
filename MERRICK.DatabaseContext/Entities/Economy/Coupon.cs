namespace MERRICK.DatabaseContext.Entities.Economy;

public class Coupon
{
    [Key]
    public int ID { get; set; }

    public required string Name { get; set; }

    public string Code => $"cp.{Name}";

    public string Hero { get; set; } = string.Empty;

    public double DiscountPercentage { get; set; } = 0.25;

    public List<string> ApplicableProducts { get; set; } = [];
}

