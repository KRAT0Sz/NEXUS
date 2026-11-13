namespace MERRICK.DatabaseContext.Entities.Administration;

public class StaffMemberAction
{
    [Key]
    public int ID { get; set; }

    public Guid SourceID { get; set; }

    public required string TargetID { get; set; }

    public required string Action { get; set; }

    public string Details { get; set; } = string.Empty;

    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
}

public static class StaffMemberActionType
{
    public const string PromoteGameMaster = "Promote Game Master";
    public const string DemoteGameMaster = "Demote Game Master";
    public const string PromoteSeniorGameMaster = "Promote Senior Game Master";
    public const string DemoteSeniorGameMaster = "Demote Senior Game Master";
    public const string ReassignHWID = "Reassign HWID";
    public const string RemoveHWID = "Remove HWID";
    public const string RemoveIP = "Remove IP Address";
    public const string RemoveMAC = "Remove MAC Address";
    public const string RemoveSystemSpecification = "Remove System Specification";
    public const string OverrideSuspension = "Override Suspension";
    public const string SetNewSuspensionDuration = "Set New Suspension Duration";
    public const string IssueGold = "Issue Gold";
    public const string IssueSilver = "Issue Silver";
    public const string IssueTickets = "Issue Tickets";
    public const string IssueRedeemableCodes = "Issue Redeemable Codes";
    public const string GrantUpgrade = "Grant Upgrade";
    public const string RevokeUpgrade = "Revoke Upgrade";
    public const string GrantAllUpgrades = "Grant All Upgrades";
    public const string RevokeAllUpgrades = "Revoke All Upgrades";
    public const string AddConsumable = "Add Consumable";
    public const string RemoveConsumable = "Remove Consumable";
    public const string EnableAccount = "Enable Account";
    public const string DisableAccount = "Disable Account";
    public const string RenameAccount = "Rename Account";
    public const string AccountLookUp = "Account Lookup";
    public const string PromoteAccountToHostOfRankedMatches = "Promote Account To Host Of Ranked Matches";
    public const string PromoteAccountToHostOfUnrankedMatches = "Promote Account To Host Of Unranked Matches";
    public const string DemoteHost = "Demote Host";
}

