using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarketFeedMonitor.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "feed_statuses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Source = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    LastAttemptAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastSuccessfulFetchAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ConsecutiveFailures = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_feed_statuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "instrument_definitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Symbol = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    AssetType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    PrimarySource = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    IsTracked = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_instrument_definitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "alert_records",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InstrumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    AlertType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Severity = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Message = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_alert_records", x => x.Id);
                    table.ForeignKey(
                        name: "FK_alert_records_instrument_definitions_InstrumentId",
                        column: x => x.InstrumentId,
                        principalTable: "instrument_definitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "market_snapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InstrumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: false),
                    Change1hPercent = table.Column<decimal>(type: "numeric(8,4)", precision: 8, scale: 4, nullable: true),
                    Source = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SourceTimestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ReceivedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_market_snapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_market_snapshots_instrument_definitions_InstrumentId",
                        column: x => x.InstrumentId,
                        principalTable: "instrument_definitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_alert_records_CreatedAt",
                table: "alert_records",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_alert_records_InstrumentId_IsActive",
                table: "alert_records",
                columns: new[] { "InstrumentId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_feed_statuses_Source",
                table: "feed_statuses",
                column: "Source",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_instrument_definitions_Symbol",
                table: "instrument_definitions",
                column: "Symbol",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_market_snapshots_InstrumentId_ReceivedAt",
                table: "market_snapshots",
                columns: new[] { "InstrumentId", "ReceivedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_market_snapshots_Source_SourceTimestamp",
                table: "market_snapshots",
                columns: new[] { "Source", "SourceTimestamp" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "alert_records");

            migrationBuilder.DropTable(
                name: "feed_statuses");

            migrationBuilder.DropTable(
                name: "market_snapshots");

            migrationBuilder.DropTable(
                name: "instrument_definitions");
        }
    }
}
