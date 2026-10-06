using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Arkanis.Overlay.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemovePreviousEntityReferenceManagementDataFromInventoryEntries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_InventoryEntries_GameEntityId",
                table: "InventoryEntries");

            migrationBuilder.DropColumn(
                name: "GameEntityCategory",
                table: "InventoryEntries");

            migrationBuilder.DropColumn(
                name: "GameEntityId",
                table: "InventoryEntries");

            migrationBuilder.DropColumn(
                name: "Quantity_Amount",
                table: "InventoryEntries");

            migrationBuilder.DropColumn(
                name: "Quantity_Unit",
                table: "InventoryEntries");

            migrationBuilder.Sql(
                """
                -- Convert 'Physical_{EntityType}' discriminators to 'Location'
                -- Convert 'Virtual_{EntityType}' discriminators to 'Virtual'
                UPDATE InventoryEntries
                SET Discriminator = CASE
                    WHEN Discriminator LIKE 'Virtual_%'
                        THEN 'Virtual'
                    WHEN Discriminator LIKE 'Physical_%'
                        THEN 'Location'
                    ELSE 'Undefined'
                END;
                """
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GameEntityCategory",
                table: "InventoryEntries",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "GameEntityId",
                table: "InventoryEntries",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Quantity_Amount",
                table: "InventoryEntries",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Quantity_Unit",
                table: "InventoryEntries",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryEntries_GameEntityId",
                table: "InventoryEntries",
                column: "GameEntityId");
        }
    }
}
