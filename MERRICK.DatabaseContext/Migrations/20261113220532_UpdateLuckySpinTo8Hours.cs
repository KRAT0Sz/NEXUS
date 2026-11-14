using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MERRICK.DatabaseContext.Migrations;

/// <inheritdoc />
public partial class _20261113220532_UpdateLuckySpinTo8Hours : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "LastFreeSpinDate",
            schema: "economy",
            table: "AccountDailyCheckIns");

        migrationBuilder.AddColumn<DateTimeOffset>(
            name: "LastFreeSpinTimestamp",
            schema: "economy",
            table: "AccountDailyCheckIns",
            type: "datetimeoffset",
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "LastFreeSpinTimestamp",
            schema: "economy",
            table: "AccountDailyCheckIns");

        migrationBuilder.AddColumn<DateOnly>(
            name: "LastFreeSpinDate",
            schema: "economy",
            table: "AccountDailyCheckIns",
            type: "date",
            nullable: true);
    }
}
