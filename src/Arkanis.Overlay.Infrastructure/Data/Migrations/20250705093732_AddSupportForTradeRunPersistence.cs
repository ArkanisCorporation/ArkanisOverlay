using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Arkanis.Overlay.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSupportForTradeRunPersistence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TradeRuns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    VehicleId = table.Column<string>(type: "TEXT", nullable: true),
                    Version = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    FinalizedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TradeRuns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TradeRunStages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TradeRunId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PricePerUnit = table.Column<int>(type: "INTEGER", nullable: false),
                    CargoTransferFee = table.Column<int>(type: "INTEGER", nullable: false),
                    CargoTransferType = table.Column<int>(type: "INTEGER", nullable: false),
                    IsRetry = table.Column<bool>(type: "INTEGER", nullable: false),
                    StartedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    ReachedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    TransferredAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    FinalizedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    Discriminator = table.Column<string>(type: "TEXT", maxLength: 21, nullable: false),
                    AcquiredAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    TerminalId = table.Column<string>(type: "TEXT", nullable: true),
                    UserSourcedData_MaxContainerSize = table.Column<int>(type: "INTEGER", nullable: true),
                    UserSourcedData_StockStatus = table.Column<int>(type: "INTEGER", nullable: true),
                    UserSourcedData_UserConfirmed = table.Column<bool>(type: "INTEGER", nullable: true),
                    UserSourcedData_Stock_Amount = table.Column<int>(type: "INTEGER", nullable: true),
                    UserSourcedData_Stock_Unit = table.Column<int>(type: "INTEGER", nullable: true),
                    SoldAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TradeRunStages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TradeRunStages_TradeRuns_TradeRunId",
                        column: x => x.TradeRunId,
                        principalTable: "TradeRuns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Quantities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TradeRunStageId = table.Column<int>(type: "INTEGER", nullable: true),
                    Amount = table.Column<int>(type: "INTEGER", nullable: false),
                    Unit = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Quantities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Quantities_TradeRunStages_TradeRunStageId",
                        column: x => x.TradeRunStageId,
                        principalTable: "TradeRunStages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OwnableEntityReferences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    QuantityOfId = table.Column<Guid>(type: "TEXT", nullable: true),
                    EntityId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OwnableEntityReferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OwnableEntityReferences_Quantities_QuantityOfId",
                        column: x => x.QuantityOfId,
                        principalTable: "Quantities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OwnableEntityReferences_QuantityOfId",
                table: "OwnableEntityReferences",
                column: "QuantityOfId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Quantities_TradeRunStageId",
                table: "Quantities",
                column: "TradeRunStageId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TradeRunStages_TradeRunId",
                table: "TradeRunStages",
                column: "TradeRunId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OwnableEntityReferences");

            migrationBuilder.DropTable(
                name: "Quantities");

            migrationBuilder.DropTable(
                name: "TradeRunStages");

            migrationBuilder.DropTable(
                name: "TradeRuns");
        }
    }
}
