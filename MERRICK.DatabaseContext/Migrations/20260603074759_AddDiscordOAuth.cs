using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MERRICK.DatabaseContext.Migrations
{
    /// <inheritdoc />
    public partial class AddDiscordOAuth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DiscordAvatarHash",
                schema: "core",
                table: "Users",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DiscordId",
                schema: "core",
                table: "Users",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DiscordLinkedAt",
                schema: "core",
                table: "Users",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DiscordUsername",
                schema: "core",
                table: "Users",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiscordAvatarHash",
                schema: "core",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DiscordId",
                schema: "core",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DiscordLinkedAt",
                schema: "core",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DiscordUsername",
                schema: "core",
                table: "Users");
        }
    }
}
