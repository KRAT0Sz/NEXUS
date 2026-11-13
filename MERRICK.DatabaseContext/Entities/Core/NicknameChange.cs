namespace MERRICK.DatabaseContext.Entities.Core;

public class NicknameChange
{
    [Key]
    public int ID { get; set; }

    public int AccountID { get; set; }

    public required string CurrentUserName { get; set; }

    public required string PastUserNames { get; set; }

    public required string NicknameChangeDates { get; set; }
}

