using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Arkanis.Overlay.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSupportForHangarAndVehicleInventoryEntries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "HangarEntryId",
                table: "InventoryEntries",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPledged",
                table: "InventoryEntries",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameTag",
                table: "InventoryEntries",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UexReferenceId",
                table: "InventoryEntries",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryEntries_HangarEntryId",
                table: "InventoryEntries",
                column: "HangarEntryId");

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryEntries_InventoryEntries_HangarEntryId",
                table: "InventoryEntries",
                column: "HangarEntryId",
                principalTable: "InventoryEntries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryEntries_InventoryEntries_HangarEntryId",
                table: "InventoryEntries");

            migrationBuilder.DropIndex(
                name: "IX_InventoryEntries_HangarEntryId",
                table: "InventoryEntries");

            migrationBuilder.DropColumn(
                name: "HangarEntryId",
                table: "InventoryEntries");

            migrationBuilder.DropColumn(
                name: "IsPledged",
                table: "InventoryEntries");

            migrationBuilder.DropColumn(
                name: "NameTag",
                table: "InventoryEntries");

            migrationBuilder.DropColumn(
                name: "UexReferenceId",
                table: "InventoryEntries");
        }
    }
}
