namespace MERRICK.DatabaseContext.Entities.Economy;

/// <summary>
///     Tracks bags/chests that the account has achieved and can open or has opened.
/// </summary>
[Index(nameof(AccountID), nameof(BagNumber), nameof(Cycle), IsUnique = true)]
public class AccountCompletedBag
{
    [Key]
    public int ID { get; set; }

    public int AccountID { get; set; }

    [ForeignKey(nameof(AccountID))]
    public Account? Account { get; set; }

    public int Cycle { get; set; } = 1;

    public int BagNumber { get; set; }

    public int BagID { get; set; }

    public bool IsOpened { get; set; } = false;

    public DateTimeOffset CompletedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? OpenedAt { get; set; } = null;

    public int GrantedSilver { get; set; } = 0;

    public int GrantedGold { get; set; } = 0;

    public int GrantedTickets { get; set; } = 0;

    public string? GrantedProductCode { get; set; } = null;
}

