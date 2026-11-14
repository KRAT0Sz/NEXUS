using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MERRICK.DatabaseContext.Migrations;

/// <inheritdoc />
public partial class _20261114064700_AddWarEffortSystem : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "TriggerName",
            schema: "economy",
            table: "Quests",
            type: "nvarchar(max)",
            nullable: false,
            defaultValue: "achievement_value_games_won");

        migrationBuilder.AddColumn<int>(
            name: "ReputationReward",
            schema: "economy",
            table: "Quests",
            type: "int",
            nullable: false,
            defaultValue: 25);

        migrationBuilder.AddColumn<int>(
            name: "TypeID",
            schema: "economy",
            table: "Quests",
            type: "int",
            nullable: false,
            defaultValue: 1);

        migrationBuilder.AddColumn<string>(
            name: "TypeKey",
            schema: "economy",
            table: "Quests",
            type: "nvarchar(max)",
            nullable: false,
            defaultValue: "games_won");

        migrationBuilder.AddColumn<string>(
            name: "HeroProduct",
            schema: "economy",
            table: "Quests",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "HeroRoleString",
            schema: "economy",
            table: "Quests",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "AttributeCode",
            schema: "economy",
            table: "Quests",
            type: "int",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "AttackType",
            schema: "economy",
            table: "Quests",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "TeamID",
            schema: "economy",
            table: "Quests",
            type: "int",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "ProductIconPath",
            schema: "economy",
            table: "Quests",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "SlotIndex",
            schema: "economy",
            table: "AccountQuestProgresses",
            type: "int",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.CreateTable(
            name: "WarEffortBags",
            schema: "economy",
            columns: table => new
            {
                ID = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                BagNumber = table.Column<int>(type: "int", nullable: false),
                BagGroupID = table.Column<int>(type: "int", nullable: false),
                Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                RequiredReputation = table.Column<int>(type: "int", nullable: false),
                RewardType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                RewardSilver = table.Column<int>(type: "int", nullable: false),
                RewardGold = table.Column<int>(type: "int", nullable: false),
                RewardTickets = table.Column<int>(type: "int", nullable: false),
                RewardProductCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                RewardProductName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                RewardProductType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                RewardIconPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                IsFinalTier = table.Column<bool>(type: "bit", nullable: false),
                IsActive = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_WarEffortBags", x => x.ID);
            });

        migrationBuilder.CreateTable(
            name: "AccountWarEfforts",
            schema: "economy",
            columns: table => new
            {
                ID = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                AccountID = table.Column<int>(type: "int", nullable: false),
                CurrentCycle = table.Column<int>(type: "int", nullable: false),
                CurrentBagNumber = table.Column<int>(type: "int", nullable: false),
                CurrentBagReputation = table.Column<int>(type: "int", nullable: false),
                TotalCycleReputation = table.Column<int>(type: "int", nullable: false),
                DailyResetsRemaining = table.Column<int>(type: "int", nullable: false),
                LastResetDate = table.Column<DateOnly>(type: "date", nullable: true),
                CycleStartDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                CycleEndDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AccountWarEfforts", x => x.ID);
                table.ForeignKey(
                    name: "FK_AccountWarEfforts_Accounts_AccountID",
                    column: x => x.AccountID,
                    principalSchema: "core",
                    principalTable: "Accounts",
                    principalColumn: "ID",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "AccountCompletedBags",
            schema: "economy",
            columns: table => new
            {
                ID = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                AccountID = table.Column<int>(type: "int", nullable: false),
                Cycle = table.Column<int>(type: "int", nullable: false),
                BagNumber = table.Column<int>(type: "int", nullable: false),
                BagID = table.Column<int>(type: "int", nullable: false),
                IsOpened = table.Column<bool>(type: "bit", nullable: false),
                CompletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                OpenedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                GrantedSilver = table.Column<int>(type: "int", nullable: false),
                GrantedGold = table.Column<int>(type: "int", nullable: false),
                GrantedTickets = table.Column<int>(type: "int", nullable: false),
                GrantedProductCode = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AccountCompletedBags", x => x.ID);
                table.ForeignKey(
                    name: "FK_AccountCompletedBags_Accounts_AccountID",
                    column: x => x.AccountID,
                    principalSchema: "core",
                    principalTable: "Accounts",
                    principalColumn: "ID",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_AccountWarEfforts_AccountID",
            schema: "economy",
            table: "AccountWarEfforts",
            column: "AccountID",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_AccountCompletedBags_AccountID_BagNumber_Cycle",
            schema: "economy",
            table: "AccountCompletedBags",
            columns: new[] { "AccountID", "BagNumber", "Cycle" },
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "AccountCompletedBags",
            schema: "economy");

        migrationBuilder.DropTable(
            name: "AccountWarEfforts",
            schema: "economy");

        migrationBuilder.DropTable(
            name: "WarEffortBags",
            schema: "economy");

        migrationBuilder.DropColumn(
            name: "SlotIndex",
            schema: "economy",
            table: "AccountQuestProgresses");

        migrationBuilder.DropColumn(
            name: "TriggerName",
            schema: "economy",
            table: "Quests");

        migrationBuilder.DropColumn(
            name: "ReputationReward",
            schema: "economy",
            table: "Quests");

        migrationBuilder.DropColumn(
            name: "TypeID",
            schema: "economy",
            table: "Quests");

        migrationBuilder.DropColumn(
            name: "TypeKey",
            schema: "economy",
            table: "Quests");

        migrationBuilder.DropColumn(
            name: "HeroProduct",
            schema: "economy",
            table: "Quests");

        migrationBuilder.DropColumn(
            name: "HeroRoleString",
            schema: "economy",
            table: "Quests");

        migrationBuilder.DropColumn(
            name: "AttributeCode",
            schema: "economy",
            table: "Quests");

        migrationBuilder.DropColumn(
            name: "AttackType",
            schema: "economy",
            table: "Quests");

        migrationBuilder.DropColumn(
            name: "TeamID",
            schema: "economy",
            table: "Quests");

        migrationBuilder.DropColumn(
            name: "ProductIconPath",
            schema: "economy",
            table: "Quests");
    }
}

