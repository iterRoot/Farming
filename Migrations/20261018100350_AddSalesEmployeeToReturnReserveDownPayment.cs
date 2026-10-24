using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Farming.Migrations
{
    /// <inheritdoc />
    public partial class AddSalesEmployeeToReturnReserveDownPayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SalesEmployeeCode",
                table: "KARV",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SalesEmployeeCode",
                table: "KARE",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SalesEmployeeCode",
                table: "KADR",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SalesEmployeeCode",
                table: "KADI",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SalesEmployeeCode",
                table: "KARV");

            migrationBuilder.DropColumn(
                name: "SalesEmployeeCode",
                table: "KARE");

            migrationBuilder.DropColumn(
                name: "SalesEmployeeCode",
                table: "KADR");

            migrationBuilder.DropColumn(
                name: "SalesEmployeeCode",
                table: "KADI");
        }
    }
}
