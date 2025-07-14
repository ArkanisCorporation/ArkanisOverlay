using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Arkanis.Overlay.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddIndicesForImprovedFiltering : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_TradeRunStages_CargoTransferType",
                table: "TradeRunStages",
                column: "CargoTransferType");

            migrationBuilder.CreateIndex(
                name: "IX_TradeRunStages_TerminalId",
                table: "TradeRunStages",
                column: "TerminalId");

            migrationBuilder.CreateIndex(
                name: "IX_TradeRuns_VehicleId",
                table: "TradeRuns",
                column: "VehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_TradeRuns_Version",
                table: "TradeRuns",
                column: "Version");

            migrationBuilder.CreateIndex(
                name: "IX_OwnableEntityReferences_EntityId",
                table: "OwnableEntityReferences",
                column: "EntityId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryEntries_LocationId",
                table: "InventoryEntries",
                column: "LocationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TradeRunStages_CargoTransferType",
                table: "TradeRunStages");

            migrationBuilder.DropIndex(
                name: "IX_TradeRunStages_TerminalId",
                table: "TradeRunStages");

            migrationBuilder.DropIndex(
                name: "IX_TradeRuns_VehicleId",
                table: "TradeRuns");

            migrationBuilder.DropIndex(
                name: "IX_TradeRuns_Version",
                table: "TradeRuns");

            migrationBuilder.DropIndex(
                name: "IX_OwnableEntityReferences_EntityId",
                table: "OwnableEntityReferences");

            migrationBuilder.DropIndex(
                name: "IX_InventoryEntries_LocationId",
                table: "InventoryEntries");
        }
    }
}
