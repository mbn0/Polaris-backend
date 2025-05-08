using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class SeedRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "128f3e8f-2f52-4664-a1e5-0ffcd1aa4c68", null, "Instructor", "INSTRUCTOR" },
                    { "7ed98d4c-51bc-4d1a-8707-35f514043c2b", null, "Student", "STUDENT" },
                    { "b92be5f8-bf71-47c7-a215-600736a1dd8b", null, "Admin", "ADMIN" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "128f3e8f-2f52-4664-a1e5-0ffcd1aa4c68");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "7ed98d4c-51bc-4d1a-8707-35f514043c2b");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "b92be5f8-bf71-47c7-a215-600736a1dd8b");
        }
    }
}
