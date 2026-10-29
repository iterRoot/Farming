using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Farming.Migrations
{
    /// <inheritdoc />
    public partial class AddBusinessPartnerTaxSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "SubjectToWithholding",
                table: "KOCR",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "TaxExemptionExpiry",
                table: "KOCR",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TaxExemptionNo",
                table: "KOCR",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TaxStatus",
                table: "KOCR",
                type: "character varying(1)",
                maxLength: 1,
                nullable: false,
                defaultValue: "Y");

            migrationBuilder.AddColumn<string>(
                name: "WithholdingTaxCode",
                table: "KOCR",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SubjectToWithholding",
                table: "KOCR");

            migrationBuilder.DropColumn(
                name: "TaxExemptionExpiry",
                table: "KOCR");

            migrationBuilder.DropColumn(
                name: "TaxExemptionNo",
                table: "KOCR");

            migrationBuilder.DropColumn(
                name: "TaxStatus",
                table: "KOCR");

            migrationBuilder.DropColumn(
                name: "WithholdingTaxCode",
                table: "KOCR");
        }
    }
}
