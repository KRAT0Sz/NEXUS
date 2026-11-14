namespace MERRICK.DatabaseContext.Entities.Economy;

[Index(nameof(AccountID), IsUnique = true)]
public class AccountDailyCheckIn
{
    [Key]
    public int ID { get; set; }

    public int AccountID { get; set; }

    [ForeignKey(nameof(AccountID))]
    public Account? Account { get; set; }

    public int CurrentStreak { get; set; } = 0;

    public DateOnly? LastCheckInDate { get; set; } = null;

    public DateTimeOffset? LastFreeSpinTimestamp { get; set; } = null;

    public int TotalSpinsPlayed { get; set; } = 0;
}

