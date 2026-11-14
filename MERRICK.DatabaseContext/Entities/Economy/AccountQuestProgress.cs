namespace MERRICK.DatabaseContext.Entities.Economy;

/// <summary>
///     Tracks an account's progress toward completing a specific quest.
/// </summary>
[Index(nameof(AccountID), nameof(QuestID), IsUnique = true)]
public class AccountQuestProgress
{
    [Key]
    public int ID { get; set; }

    public int AccountID { get; set; }

    [ForeignKey(nameof(AccountID))]
    public Account? Account { get; set; }

    public int QuestID { get; set; }

    [ForeignKey(nameof(QuestID))]
    public Quest? Quest { get; set; }

    /// <summary>
    ///     Active quest display slot in the game client (0 or 1).
    /// </summary>
    public int SlotIndex { get; set; } = 0;

    public int CurrentCount { get; set; } = 0;

    public bool IsCompleted { get; set; } = false;

    public bool IsClaimed { get; set; } = false;

    public DateTimeOffset? ClaimedAt { get; set; } = null;

    public DateTimeOffset ResetAt { get; set; } = DateTimeOffset.UtcNow;
}
