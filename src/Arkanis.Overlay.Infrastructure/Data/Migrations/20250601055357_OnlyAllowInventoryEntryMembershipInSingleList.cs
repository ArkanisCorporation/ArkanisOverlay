using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Arkanis.Overlay.Infrastructure.Data.Migrations
{
    using System.Diagnostics.CodeAnalysis;

    /// <inheritdoc />
    public partial class OnlyAllowInventoryEntryMembershipInSingleList : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InventoryListItems");

            migrationBuilder.AddColumn<Guid>(
                name: "ListId",
                table: "InventoryEntries",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryEntries_ListId",
                table: "InventoryEntries",
                column: "ListId");

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryEntries_InventoryLists_ListId",
                table: "InventoryEntries",
                column: "ListId",
                principalTable: "InventoryLists",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        [SuppressMessage("Performance", "CA1861:Avoid constant arrays as arguments")]
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryEntries_InventoryLists_ListId",
                table: "InventoryEntries");

            migrationBuilder.DropIndex(
                name: "IX_InventoryEntries_ListId",
                table: "InventoryEntries");

            migrationBuilder.DropColumn(
                name: "ListId",
                table: "InventoryEntries");

            migrationBuilder.CreateTable(
                name: "InventoryListItems",
                columns: table => new
                {
                    EntryId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ListId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryListItems", x => new { x.EntryId, x.ListId });
                    table.ForeignKey(
                        name: "FK_InventoryListItems_InventoryEntries_EntryId",
                        column: x => x.EntryId,
                        principalTable: "InventoryEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InventoryListItems_InventoryLists_ListId",
                        column: x => x.ListId,
                        principalTable: "InventoryLists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryListItems_ListId_EntryId",
                table: "InventoryListItems",
                columns: new[] { "ListId", "EntryId" },
                unique: true);
        }
    }
}
