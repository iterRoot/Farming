using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Farming.Migrations
{
    /// <inheritdoc />
    public partial class AddItemStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItemsMaster_ItemsMaster_StatusItemsCode",
                table: "ItemsMaster");

            migrationBuilder.DropIndex(
                name: "IX_ItemsMaster_StatusItemsCode",
                table: "ItemsMaster");

            migrationBuilder.DropColumn(
                name: "StatusItemsCode",
                table: "ItemsMaster");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "ItemsMaster",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "ItemsMaster");

            migrationBuilder.AddColumn<string>(
                name: "StatusItemsCode",
                table: "ItemsMaster",
                type: "character varying(100)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_ItemsMaster_StatusItemsCode",
                table: "ItemsMaster",
                column: "StatusItemsCode");

            migrationBuilder.AddForeignKey(
                name: "FK_ItemsMaster_ItemsMaster_StatusItemsCode",
                table: "ItemsMaster",
                column: "StatusItemsCode",
                principalTable: "ItemsMaster",
                principalColumn: "ItemsCode",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
