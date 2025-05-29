using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Arkanis.Overlay.Infrastructure.Data.Migrations
{
    using System.Diagnostics.CodeAnalysis;

    /// <inheritdoc />
    public partial class AddSupportForItemInventoryPersistence : Migration
    {
        /// <inheritdoc />
        [SuppressMessage("Performance", "CA1861:Avoid constant arrays as arguments")]
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InventoryEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    GameEntityId = table.Column<string>(type: "TEXT", nullable: false),
                    Discriminator = table.Column<string>(type: "TEXT", maxLength: 34, nullable: false),
                    Quantity_Amount = table.Column<long>(type: "INTEGER", nullable: false),
                    Quantity_Unit = table.Column<int>(type: "INTEGER", nullable: false),
                    LocationId = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InventoryLists",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 60, nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 10000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryLists", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InventoryListItems",
                columns: table => new
                {
                    ListId = table.Column<Guid>(type: "TEXT", nullable: false),
                    EntryId = table.Column<Guid>(type: "TEXT", nullable: false)
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
                name: "IX_InventoryEntries_GameEntityId",
                table: "InventoryEntries",
                column: "GameEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryListItems_ListId_EntryId",
                table: "InventoryListItems",
                columns: new[] { "ListId", "EntryId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InventoryListItems");

            migrationBuilder.DropTable(
                name: "InventoryEntries");

            migrationBuilder.DropTable(
                name: "InventoryLists");
        }
    }
}
