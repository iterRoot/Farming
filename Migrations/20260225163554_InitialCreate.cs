using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Farming.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ItemsMaster",
                columns: table => new
                {
                    ItemsCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ItemsName = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    UomName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    UomCode = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Price = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    PriceList = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Types = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    StatusItemsCode = table.Column<string>(type: "character varying(100)", nullable: false),
                    GuidId = table.Column<Guid>(type: "uuid", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
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
                    table.PrimaryKey("PK_ItemsMaster", x => x.ItemsCode);
                    table.ForeignKey(
                        name: "FK_ItemsMaster_ItemsMaster_StatusItemsCode",
                        column: x => x.StatusItemsCode,
                        principalTable: "ItemsMaster",
                        principalColumn: "ItemsCode",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ItemsMaster_StatusItemsCode",
                table: "ItemsMaster",
                column: "StatusItemsCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ItemsMaster");
        }
    }
}
