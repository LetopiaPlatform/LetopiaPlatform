using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LetopiaPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addinterestsandskllscolumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<List<string>>(
                name: "interests",
                table: "AspNetUsers",
                type: "text[]",
                nullable: false,
                defaultValueSql: "'{}'::text[]");

            migrationBuilder.AddColumn<List<string>>(
                name: "skills",
                table: "AspNetUsers",
                type: "text[]",
                nullable: false,
                defaultValueSql: "'{}'::text[]");
            migrationBuilder.Sql(@"
    CREATE OR REPLACE FUNCTION array_has_duplicates(arr text[])
    RETURNS boolean
    LANGUAGE sql
    IMMUTABLE
    AS $$
        SELECT array_length(arr, 1) > array_length(ARRAY(SELECT DISTINCT unnest(arr)), 1);
    $$;
");
            migrationBuilder.AddCheckConstraint(
                name: "CK_Users_Interests_Unique",
                table: "AspNetUsers",
                sql: "NOT array_has_duplicates(interests)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Users_Skills_Unique",
                table: "AspNetUsers",
                sql: "NOT array_has_duplicates(skills)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS array_has_duplicates(text[]);");
            migrationBuilder.DropCheckConstraint(
                name: "CK_Users_Interests_Unique",
                table: "AspNetUsers");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Users_Skills_Unique",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "interests",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "skills",
                table: "AspNetUsers");
        }
    }
}
