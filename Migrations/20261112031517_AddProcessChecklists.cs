using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Farming.Migrations
{
    /// <inheritdoc />
    public partial class AddProcessChecklists : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "KCLR",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RunNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ChecklistTemplateId = table.Column<int>(type: "integer", nullable: false),
                    TemplateCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ContextRef = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    StartedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RequireSignOff = table.Column<bool>(type: "boolean", nullable: false),
                    SignOffRole = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SignedOffBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SignedOffAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_KCLR", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KCLT",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Category = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Module = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    SubCategory = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    TriggerType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    TriggerEvent = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LinkedDocType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    AssignedRole = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    AssignedTo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    RequireSignOff = table.Column<bool>(type: "boolean", nullable: false),
                    SignOffRole = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Tags = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    IconName = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Version = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    Remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_KCLT", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CLR1",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ChecklistRunId = table.Column<int>(type: "integer", nullable: false),
                    StepNo = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    StepType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    GroupName = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false),
                    DependsOnStepNo = table.Column<int>(type: "integer", nullable: true),
                    LinkedRoute = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    StepStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CompletedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CLR1", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CLR1_KCLR_ChecklistRunId",
                        column: x => x.ChecklistRunId,
                        principalTable: "KCLR",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CLT1",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ChecklistTemplateId = table.Column<int>(type: "integer", nullable: false),
                    StepNo = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    StepType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    GroupName = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    AssignedRole = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    AssignedTo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    EstimatedMinutes = table.Column<int>(type: "integer", nullable: false),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false),
                    RequiresAttachment = table.Column<bool>(type: "boolean", nullable: false),
                    RequiresNote = table.Column<bool>(type: "boolean", nullable: false),
                    DependsOnStepNo = table.Column<int>(type: "integer", nullable: true),
                    LinkedRoute = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Instructions = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    AcceptanceCriteria = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CLT1", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CLT1_KCLT_ChecklistTemplateId",
                        column: x => x.ChecklistTemplateId,
                        principalTable: "KCLT",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CLR1_ChecklistRunId",
                table: "CLR1",
                column: "ChecklistRunId");

            migrationBuilder.CreateIndex(
                name: "IX_CLT1_ChecklistTemplateId",
                table: "CLT1",
                column: "ChecklistTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_KCLR_ChecklistTemplateId",
                table: "KCLR",
                column: "ChecklistTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_KCLR_RunNo",
                table: "KCLR",
                column: "RunNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KCLT_Code",
                table: "KCLT",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CLR1");

            migrationBuilder.DropTable(
                name: "CLT1");

            migrationBuilder.DropTable(
                name: "KCLR");

            migrationBuilder.DropTable(
                name: "KCLT");
        }
    }
}
