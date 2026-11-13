using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MERRICK.DatabaseContext.Migrations
{
    /// <inheritdoc />
    public partial class AddModerationAndTelemetryEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "economy");

            migrationBuilder.EnsureSchema(
                name: "telemetry");

            migrationBuilder.EnsureSchema(
                name: "admin");

            migrationBuilder.CreateTable(
                name: "Coupons",
                schema: "economy",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Hero = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DiscountPercentage = table.Column<double>(type: "float", nullable: false),
                    ApplicableProducts = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Coupons", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "DailySpecials",
                schema: "economy",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PanelIndex = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DiscountOff = table.Column<int>(type: "int", nullable: false),
                    DiscountSilver = table.Column<int>(type: "int", nullable: false),
                    TagTitle = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TagDateLimit = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailySpecials", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "GenesisRewards",
                schema: "economy",
                columns: table => new
                {
                    Code = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimedUserID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClaimedRewardsMask = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GenesisRewards", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "HardwareSnapshots",
                schema: "telemetry",
                columns: table => new
                {
                    Hash = table.Column<byte[]>(type: "varbinary(64)", maxLength: 64, nullable: false),
                    SnapshotJSONCompressed = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    TimestampFirstObserved = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    HardwareID = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    IPAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    UserID = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HardwareSnapshots", x => x.Hash);
                });

            migrationBuilder.CreateTable(
                name: "IncidentReports",
                schema: "admin",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReporterID = table.Column<int>(type: "int", nullable: true),
                    AccusedID = table.Column<int>(type: "int", nullable: true),
                    ReviewerID = table.Column<int>(type: "int", nullable: true),
                    MatchID = table.Column<int>(type: "int", nullable: false),
                    IncidentType = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OffenseOccurrenceTime = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ResolutionDetails = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TimestampCreated = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncidentReports", x => x.ID);
                    table.ForeignKey(
                        name: "FK_IncidentReports_Accounts_AccusedID",
                        column: x => x.AccusedID,
                        principalSchema: "core",
                        principalTable: "Accounts",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_IncidentReports_Accounts_ReporterID",
                        column: x => x.ReporterID,
                        principalSchema: "core",
                        principalTable: "Accounts",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_IncidentReports_Accounts_ReviewerID",
                        column: x => x.ReviewerID,
                        principalSchema: "core",
                        principalTable: "Accounts",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "NicknameChanges",
                schema: "core",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountID = table.Column<int>(type: "int", nullable: false),
                    CurrentUserName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PastUserNames = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NicknameChangeDates = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NicknameChanges", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "StaffMemberActions",
                schema: "admin",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SourceID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TargetID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Details = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Timestamp = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffMemberActions", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "StatsResets",
                schema: "economy",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountID = table.Column<int>(type: "int", nullable: false),
                    ResetType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TimestampReset = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    StatisticsSnapshotJSON = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatsResets", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Suspensions",
                schema: "admin",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OffenderAccountID = table.Column<int>(type: "int", nullable: false),
                    IssuerAccountID = table.Column<int>(type: "int", nullable: true),
                    SuspendByMask = table.Column<int>(type: "int", nullable: false),
                    TimestampStart = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    TimestampBaseEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    TimestampEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Multiplier = table.Column<float>(type: "real", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InternalDetails = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Override = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Suspensions", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Suspensions_Accounts_IssuerAccountID",
                        column: x => x.IssuerAccountID,
                        principalSchema: "core",
                        principalTable: "Accounts",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_Suspensions_Accounts_OffenderAccountID",
                        column: x => x.OffenderAccountID,
                        principalSchema: "core",
                        principalTable: "Accounts",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UniqueHardwares",
                schema: "telemetry",
                columns: table => new
                {
                    FirstSnapshotHash = table.Column<byte[]>(type: "varbinary(64)", maxLength: 64, nullable: false),
                    UserID = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    Timestamp = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    SerialisedData = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UniqueHardwares", x => x.FirstSnapshotHash);
                });

            migrationBuilder.CreateTable(
                name: "GeoInformations",
                schema: "telemetry",
                columns: table => new
                {
                    IPAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: false),
                    CityID = table.Column<int>(type: "int", nullable: true),
                    ContinentID = table.Column<int>(type: "int", nullable: true),
                    CountryID = table.Column<int>(type: "int", nullable: true),
                    Latitude = table.Column<float>(type: "real", nullable: false),
                    Longitude = table.Column<float>(type: "real", nullable: false),
                    TimeZone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PostalCode = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeoInformations", x => x.IPAddress);
                });

            migrationBuilder.CreateTable(
                name: "GeoNames",
                schema: "telemetry",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Names = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ISOCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsInEuropeanUnion = table.Column<bool>(type: "bit", nullable: true),
                    GeoInformationIPAddress = table.Column<string>(type: "nvarchar(45)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeoNames", x => x.ID);
                    table.ForeignKey(
                        name: "FK_GeoNames_GeoInformations_GeoInformationIPAddress",
                        column: x => x.GeoInformationIPAddress,
                        principalSchema: "telemetry",
                        principalTable: "GeoInformations",
                        principalColumn: "IPAddress");
                });

            migrationBuilder.CreateIndex(
                name: "IX_GeoInformations_CityID",
                schema: "telemetry",
                table: "GeoInformations",
                column: "CityID");

            migrationBuilder.CreateIndex(
                name: "IX_GeoInformations_ContinentID",
                schema: "telemetry",
                table: "GeoInformations",
                column: "ContinentID");

            migrationBuilder.CreateIndex(
                name: "IX_GeoInformations_CountryID",
                schema: "telemetry",
                table: "GeoInformations",
                column: "CountryID");

            migrationBuilder.CreateIndex(
                name: "IX_GeoNames_GeoInformationIPAddress",
                schema: "telemetry",
                table: "GeoNames",
                column: "GeoInformationIPAddress");

            migrationBuilder.CreateIndex(
                name: "IX_HardwareSnapshots_UserID",
                schema: "telemetry",
                table: "HardwareSnapshots",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentReports_AccusedID",
                schema: "admin",
                table: "IncidentReports",
                column: "AccusedID");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentReports_ReporterID",
                schema: "admin",
                table: "IncidentReports",
                column: "ReporterID");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentReports_ReviewerID",
                schema: "admin",
                table: "IncidentReports",
                column: "ReviewerID");

            migrationBuilder.CreateIndex(
                name: "IX_Suspensions_IssuerAccountID",
                schema: "admin",
                table: "Suspensions",
                column: "IssuerAccountID");

            migrationBuilder.CreateIndex(
                name: "IX_Suspensions_OffenderAccountID",
                schema: "admin",
                table: "Suspensions",
                column: "OffenderAccountID");

            migrationBuilder.AddForeignKey(
                name: "FK_GeoInformations_GeoNames_CityID",
                schema: "telemetry",
                table: "GeoInformations",
                column: "CityID",
                principalSchema: "telemetry",
                principalTable: "GeoNames",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_GeoInformations_GeoNames_ContinentID",
                schema: "telemetry",
                table: "GeoInformations",
                column: "ContinentID",
                principalSchema: "telemetry",
                principalTable: "GeoNames",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_GeoInformations_GeoNames_CountryID",
                schema: "telemetry",
                table: "GeoInformations",
                column: "CountryID",
                principalSchema: "telemetry",
                principalTable: "GeoNames",
                principalColumn: "ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GeoInformations_GeoNames_CityID",
                schema: "telemetry",
                table: "GeoInformations");

            migrationBuilder.DropForeignKey(
                name: "FK_GeoInformations_GeoNames_ContinentID",
                schema: "telemetry",
                table: "GeoInformations");

            migrationBuilder.DropForeignKey(
                name: "FK_GeoInformations_GeoNames_CountryID",
                schema: "telemetry",
                table: "GeoInformations");

            migrationBuilder.DropTable(
                name: "Coupons",
                schema: "economy");

            migrationBuilder.DropTable(
                name: "DailySpecials",
                schema: "economy");

            migrationBuilder.DropTable(
                name: "GenesisRewards",
                schema: "economy");

            migrationBuilder.DropTable(
                name: "HardwareSnapshots",
                schema: "telemetry");

            migrationBuilder.DropTable(
                name: "IncidentReports",
                schema: "admin");

            migrationBuilder.DropTable(
                name: "NicknameChanges",
                schema: "core");

            migrationBuilder.DropTable(
                name: "StaffMemberActions",
                schema: "admin");

            migrationBuilder.DropTable(
                name: "StatsResets",
                schema: "economy");

            migrationBuilder.DropTable(
                name: "Suspensions",
                schema: "admin");

            migrationBuilder.DropTable(
                name: "UniqueHardwares",
                schema: "telemetry");

            migrationBuilder.DropTable(
                name: "GeoNames",
                schema: "telemetry");

            migrationBuilder.DropTable(
                name: "GeoInformations",
                schema: "telemetry");
        }
    }
}
