using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Arkanis.Overlay.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class TransformInventoryEntriesToUseOwnableEntityReferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "InventoryEntryId",
                table: "Quantities",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TradeRunId",
                table: "InventoryEntries",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Quantities_InventoryEntryId",
                table: "Quantities",
                column: "InventoryEntryId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryEntries_TradeRunId",
                table: "InventoryEntries",
                column: "TradeRunId");

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryEntries_TradeRuns_TradeRunId",
                table: "InventoryEntries",
                column: "TradeRunId",
                principalTable: "TradeRuns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Quantities_InventoryEntries_InventoryEntryId",
                table: "Quantities",
                column: "InventoryEntryId",
                principalTable: "InventoryEntries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.Sql(
                """
                -- Create a view to generate new v4 GUIDs
                CREATE TEMPORARY VIEW uuid4 AS
                SELECT upper(hex( randomblob(4))
                    || '-' || hex( randomblob(2))
                    || '-' || '4' || substr( hex( randomblob(2)), 2)
                    || '-' || substr('AB89', 1 + (abs(random()) % 4) , 1)  || substr(hex(randomblob(2)), 2)
                    || '-' || hex(randomblob(6))) AS next;

                -- Create a temporary table to migrate the current data into and transform appropriately
                CREATE TEMPORARY TABLE InventoryListEntryReferenceMigration(
                    InventoryEntryId string,
                    QuantityId string NULL,
                    ReferenceId string NULL,
                    Reference string,
                    Amount integer,
                    Unit integer
                );

                -- Pseudorandom GUID generator does not work properly as a subquery within the INSERT INTO SELECT statement
                CREATE TRIGGER CreateUuidsForInventoryListEntryReferenceMigration
                    AFTER INSERT ON InventoryListEntryReferenceMigration FOR EACH ROW WHEN NEW.QuantityId IS NULL
                BEGIN
                    UPDATE InventoryListEntryReferenceMigration
                    SET QuantityId = (SELECT next FROM uuid4),
                        ReferenceId = (SELECT next FROM uuid4)
                    WHERE ROWID = NEW.ROWID;
                END;

                -- Migrate and transform the current data
                INSERT INTO InventoryListEntryReferenceMigration
                SELECT Id AS InventoryEntryId,
                       NULL as QuantityId,
                       NULL as ReferenceId,
                       GameEntityId AS Reference,
                       Quantity_Amount AS Amount,
                       Quantity_Unit AS Unit
                FROM InventoryEntries;

                -- Migrate the transformed data into appropriate tables
                INSERT INTO Quantities (Id, InventoryEntryId, Amount, Unit)
                SELECT QuantityId, InventoryEntryId, Amount, Unit
                FROM InventoryListEntryReferenceMigration;

                INSERT INTO OwnableEntityReferences (Id, QuantityOfId, EntityId)
                SELECT ReferenceId, QuantityId, Reference
                FROM InventoryListEntryReferenceMigration;

                -- Cleanup
                DROP TRIGGER CreateUuidsForInventoryListEntryReferenceMigration;
                """
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryEntries_TradeRuns_TradeRunId",
                table: "InventoryEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_Quantities_InventoryEntries_InventoryEntryId",
                table: "Quantities");

            migrationBuilder.DropIndex(
                name: "IX_Quantities_InventoryEntryId",
                table: "Quantities");

            migrationBuilder.DropIndex(
                name: "IX_InventoryEntries_TradeRunId",
                table: "InventoryEntries");

            migrationBuilder.DropColumn(
                name: "InventoryEntryId",
                table: "Quantities");

            migrationBuilder.DropColumn(
                name: "TradeRunId",
                table: "InventoryEntries");
        }
    }
}
