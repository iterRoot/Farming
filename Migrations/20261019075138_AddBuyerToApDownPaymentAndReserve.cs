using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Farming.Migrations
{
    /// <inheritdoc />
    public partial class AddBuyerToApDownPaymentAndReserve : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BuyerCode",
                table: "KPDR",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BuyerCode",
                table: "KPDI",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BuyerCode",
                table: "KPAR",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BuyerCode",
                table: "KPDR");

            migrationBuilder.DropColumn(
                name: "BuyerCode",
                table: "KPDI");

            migrationBuilder.DropColumn(
                name: "BuyerCode",
                table: "KPAR");
        }
    }
}
