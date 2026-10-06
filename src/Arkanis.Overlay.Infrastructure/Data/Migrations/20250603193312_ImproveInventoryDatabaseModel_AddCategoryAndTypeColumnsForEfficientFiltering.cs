using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1707
namespace Arkanis.Overlay.Infrastructure.Data.Migrations
{
    using Domain.Enums;
    using Domain.Models.Inventory;

    /// <inheritdoc />
    public partial class ImproveInventoryDatabaseModel_AddCategoryAndTypeColumnsForEfficientFiltering : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EntryType",
                table: "InventoryEntries",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            foreach (var entryType in Enum.GetValues<InventoryEntryBase.EntryType>())
            {
                migrationBuilder.Sql(
                    $"""
                     UPDATE InventoryEntries
                     SET EntryType = {(int)entryType}
                     WHERE Discriminator LIKE "{entryType.ToString("G")}%"
                     """
                );
            }

            migrationBuilder.AddColumn<int>(
                name: "GameEntityCategory",
                table: "InventoryEntries",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            foreach (var entityCategory in Enum.GetValues<GameEntityCategory>())
            {
                migrationBuilder.Sql(
                    $"""
                     UPDATE InventoryEntries
                     SET GameEntityCategory = {(int)entityCategory}
                     WHERE Discriminator LIKE "%{entityCategory.ToString("G")}"
                     """
                );
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EntryType",
                table: "InventoryEntries");

            migrationBuilder.DropColumn(
                name: "GameEntityCategory",
                table: "InventoryEntries");
        }
    }
}
#pragma warning restore CA1707
