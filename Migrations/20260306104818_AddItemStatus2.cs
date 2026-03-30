using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Farming.Migrations
{
    /// <inheritdoc />
    public partial class AddItemStatus2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "sale_invoices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ItemsCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ItemsName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DisPct = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DisSum = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PriceBfDis = table.Column<decimal>(type: "numeric", nullable: false),
                    PriceAtDis = table.Column<decimal>(type: "numeric", nullable: false),
                    PriceAtVat = table.Column<decimal>(type: "numeric", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    InvQty = table.Column<decimal>(type: "numeric", nullable: false),
                    LineTotalLC = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    LineTotalFC = table.Column<decimal>(type: "numeric", nullable: false),
                    LineTotalSys = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalLC = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalFC = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalSys = table.Column<decimal>(type: "numeric", nullable: false),
                    LineStatus = table.Column<decimal>(type: "numeric", nullable: false),
                    ItemsCost = table.Column<decimal>(type: "numeric", nullable: false),
                    GTotal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    NetPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    GrossPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    BaseEntry = table.Column<int>(type: "integer", nullable: false),
                    UomName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    UomCode = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Price = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    PriceList = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Types = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    FreeTxt = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    GuidId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    InActive = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sale_invoices", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "sale_invoices");
        }
    }
}
