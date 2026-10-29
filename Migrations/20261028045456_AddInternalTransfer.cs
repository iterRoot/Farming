using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Farming.Migrations
{
    /// <inheritdoc />
    public partial class AddInternalTransfer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "KBIT",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DocNum = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PostingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FromAccount = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    FromAccountName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, defaultValue: ""),
                    ToAccount = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ToAccountName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, defaultValue: ""),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    BankCharge = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ChargeAccount = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "USD"),
                    Reference = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Memo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Status = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false, defaultValue: "P"),
                    JournalEntryId = table.Column<int>(type: "integer", nullable: true),
                    JournalNo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    UserSign = table.Column<int>(type: "integer", nullable: true),
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
                    table.PrimaryKey("PK_KBIT", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_KBIT_DocNum",
                table: "KBIT",
                column: "DocNum",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KBIT_FromAccount",
                table: "KBIT",
                column: "FromAccount");

            migrationBuilder.CreateIndex(
                name: "IX_KBIT_PostingDate",
                table: "KBIT",
                column: "PostingDate");

            migrationBuilder.CreateIndex(
                name: "IX_KBIT_ToAccount",
                table: "KBIT",
                column: "ToAccount");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KBIT");
        }
    }
}
