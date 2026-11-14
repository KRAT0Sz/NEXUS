using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MERRICK.DatabaseContext.Migrations;

/// <inheritdoc />
public partial class _20261113214727_AddGamificationQuestsAndCheckIns : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "AccountDailyCheckIns",
            schema: "economy",
            columns: table => new
            {
                ID = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                AccountID = table.Column<int>(type: "int", nullable: false),
                CurrentStreak = table.Column<int>(type: "int", nullable: false),
                LastCheckInDate = table.Column<DateOnly>(type: "date", nullable: true),
                LastFreeSpinDate = table.Column<DateOnly>(type: "date", nullable: true),
                TotalSpinsPlayed = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AccountDailyCheckIns", x => x.ID);
                table.ForeignKey(
                    name: "FK_AccountDailyCheckIns_Accounts_AccountID",
                    column: x => x.AccountID,
                    principalSchema: "core",
                    principalTable: "Accounts",
                    principalColumn: "ID",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Quests",
            schema: "economy",
            columns: table => new
            {
                ID = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                QuestKey = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Category = table.Column<int>(type: "int", nullable: false),
                TargetType = table.Column<int>(type: "int", nullable: false),
                TargetCount = table.Column<int>(type: "int", nullable: false),
                RewardSilver = table.Column<int>(type: "int", nullable: false),
                RewardGold = table.Column<int>(type: "int", nullable: false),
                RewardTickets = table.Column<int>(type: "int", nullable: false),
                RewardUpgradeCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                IsActive = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Quests", x => x.ID);
            });

        migrationBuilder.CreateTable(
            name: "AccountQuestProgresses",
            schema: "economy",
            columns: table => new
            {
                ID = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                AccountID = table.Column<int>(type: "int", nullable: false),
                QuestID = table.Column<int>(type: "int", nullable: false),
                CurrentCount = table.Column<int>(type: "int", nullable: false),
                IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                IsClaimed = table.Column<bool>(type: "bit", nullable: false),
                ClaimedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                ResetAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AccountQuestProgresses", x => x.ID);
                table.ForeignKey(
                    name: "FK_AccountQuestProgresses_Accounts_AccountID",
                    column: x => x.AccountID,
                    principalSchema: "core",
                    principalTable: "Accounts",
                    principalColumn: "ID",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_AccountQuestProgresses_Quests_QuestID",
                    column: x => x.QuestID,
                    principalSchema: "economy",
                    principalTable: "Quests",
                    principalColumn: "ID",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_AccountDailyCheckIns_AccountID",
            schema: "economy",
            table: "AccountDailyCheckIns",
            column: "AccountID",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_AccountQuestProgresses_AccountID_QuestID",
            schema: "economy",
            table: "AccountQuestProgresses",
            columns: new[] { "AccountID", "QuestID" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_AccountQuestProgresses_QuestID",
            schema: "economy",
            table: "AccountQuestProgresses",
            column: "QuestID");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "AccountDailyCheckIns",
            schema: "economy");

        migrationBuilder.DropTable(
            name: "AccountQuestProgresses",
            schema: "economy");

        migrationBuilder.DropTable(
            name: "Quests",
            schema: "economy");
    }
}
