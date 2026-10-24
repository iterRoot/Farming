using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Farming.Migrations
{
    /// <inheritdoc />
    public partial class AddBuyerToGoodsReturnRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BuyerCode",
                table: "KPGQ",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BuyerCode",
                table: "KPGQ");
        }
    }
}
