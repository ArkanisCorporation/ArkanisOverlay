using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Arkanis.Overlay.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class ExtendLocalCacheEntries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "ContentSizeBytes",
                table: "ExternalSourceDataCache",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "ExternalSourceDataCache",
                type: "TEXT",
                nullable: false,
                defaultValue: "Additional information not available.");

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "ExternalSourceDataCache",
                type: "TEXT",
                nullable: false,
                defaultValue: "Further unspecified local cache entry");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContentSizeBytes",
                table: "ExternalSourceDataCache");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "ExternalSourceDataCache");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "ExternalSourceDataCache");
        }
    }
}
