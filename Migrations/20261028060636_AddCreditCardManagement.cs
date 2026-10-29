using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Farming.Migrations
{
    /// <inheritdoc />
    public partial class AddCreditCardManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "KBCS",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ClearingAccount = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SettlementAccount = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    FeePercent = table.Column<decimal>(type: "numeric(9,4)", precision: 9, scale: 4, nullable: false),
                    FeeAccount = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
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
                    table.PrimaryKey("PK_KBCS", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KBCT",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DocNum = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CardCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PostingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    BankAccount = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ClearingAccount = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    GrossAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    FeeAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    NetAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    FeeAccount = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Reference = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false, defaultValue: "P"),
                    JournalEntryId = table.Column<int>(type: "integer", nullable: true),
                    JournalNo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
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
                    table.PrimaryKey("PK_KBCT", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BCT1",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreditCardSettlementId = table.Column<int>(type: "integer", nullable: false),
                    IncomingPaymentId = table.Column<int>(type: "integer", nullable: false),
                    PaymentDocNum = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CustomerName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
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
                    table.PrimaryKey("PK_BCT1", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BCT1_KBCT_CreditCardSettlementId",
                        column: x => x.CreditCardSettlementId,
                        principalTable: "KBCT",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BCT1_CreditCardSettlementId",
                table: "BCT1",
                column: "CreditCardSettlementId");

            migrationBuilder.CreateIndex(
                name: "IX_BCT1_IncomingPaymentId",
                table: "BCT1",
                column: "IncomingPaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_KBCS_Code",
                table: "KBCS",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KBCT_CardCode",
                table: "KBCT",
                column: "CardCode");

            migrationBuilder.CreateIndex(
                name: "IX_KBCT_DocNum",
                table: "KBCT",
                column: "DocNum",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KBCT_PostingDate",
                table: "KBCT",
                column: "PostingDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BCT1");

            migrationBuilder.DropTable(
                name: "KBCS");

            migrationBuilder.DropTable(
                name: "KBCT");
        }
    }
}
