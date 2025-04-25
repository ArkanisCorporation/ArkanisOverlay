using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Arkanis.Overlay.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExternalSourceDataCache",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Content = table.Column<string>(type: "JSONB", nullable: false),
                    CachedUntil = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    DataState_UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    DataState_Version = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalSourceDataCache", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExternalSourceDataCache");
        }
    }
}
