namespace MERRICK.DatabaseContext.Entities.Economy;

/// <summary>
///     Tracks a player account's current progress in The War Effort cycle.
/// </summary>
[Index(nameof(AccountID), IsUnique = true)]
public class AccountWarEffort
{
    [Key]
    public int ID { get; set; }

    public int AccountID { get; set; }

    [ForeignKey(nameof(AccountID))]
    public Account? Account { get; set; }

    public int CurrentCycle { get; set; } = 1;

    public int CurrentBagNumber { get; set; } = 1;

    public int CurrentBagReputation { get; set; } = 0;

    public int TotalCycleReputation { get; set; } = 0;

    public int DailyResetsRemaining { get; set; } = 1;

    public DateOnly? LastResetDate { get; set; } = null;

    public DateTimeOffset CycleStartDate { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset CycleEndDate { get; set; } = DateTimeOffset.UtcNow.AddDays(28);
}

