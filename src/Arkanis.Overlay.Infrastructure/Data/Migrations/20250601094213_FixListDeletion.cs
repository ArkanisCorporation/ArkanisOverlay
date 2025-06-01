using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Arkanis.Overlay.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixListDeletion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryEntries_InventoryLists_ListId",
                table: "InventoryEntries");

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryEntries_InventoryLists_ListId",
                table: "InventoryEntries",
                column: "ListId",
                principalTable: "InventoryLists",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryEntries_InventoryLists_ListId",
                table: "InventoryEntries");

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryEntries_InventoryLists_ListId",
                table: "InventoryEntries",
                column: "ListId",
                principalTable: "InventoryLists",
                principalColumn: "Id");
        }
    }
}
