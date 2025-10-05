#nullable disable

namespace Arkanis.Overlay.Infrastructure.Migrations;

using Microsoft.EntityFrameworkCore.Migrations;

/// <inheritdoc />
public partial class AddMissingVehicleStorageStep : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.AddColumn<DateTimeOffset>(
            name: "VehicleStoredAt",
            table: "TradeRunStages",
            type: "TEXT",
            nullable: true);

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropColumn(
            name: "VehicleStoredAt",
            table: "TradeRunStages");
}
