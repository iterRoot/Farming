using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Farming.Migrations
{
    /// <inheritdoc />
    public partial class AddUserSessionSecurity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LockedUntil",
                table: "KUSR",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SessionsValidFrom",
                table: "KUSR",
                type: "timestamp with time zone",
                nullable: true);

            // The permission filter now denies users who have no Authorisations record (it used to allow them
            // everything). So nobody loses access on deploy, give every existing user without a record an
            // explicit Full one, which also makes them visible on the Authorisations screen to review and
            // narrow. Users created after this get a no-access record when they are created.
            migrationBuilder.Sql(@"
                INSERT INTO ""KUAU"" (""UserId"", ""FullAuthorization"", ""Permissions"", ""GuidId"", ""CreatedAt"", ""InActive"")
                SELECT u.""Id"", TRUE, '{}'::jsonb, gen_random_uuid(), NOW(), FALSE
                FROM ""KUSR"" u
                WHERE NOT EXISTS (SELECT 1 FROM ""KUAU"" a WHERE a.""UserId"" = u.""Id"");");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LockedUntil",
                table: "KUSR");

            migrationBuilder.DropColumn(
                name: "SessionsValidFrom",
                table: "KUSR");
        }
    }
}
