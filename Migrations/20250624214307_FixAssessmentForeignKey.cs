using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class FixAssessmentForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "38b68739-fac5-40dc-b487-6907166ab920");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "9c0e0215-4419-4bef-a4e0-0ae0c05cd260");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "9e7f6c03-01d6-43a3-99ea-e2c05bd1b08a");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "25d96b1e-2234-4ca7-9010-089c7a6e8837", null, "Instructor", "INSTRUCTOR" },
                    { "6464ab44-a2f7-46d1-bfad-f4c84ea84d51", null, "Admin", "ADMIN" },
                    { "84615b0e-0358-467e-b0aa-d9489e8b605d", null, "Student", "STUDENT" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "25d96b1e-2234-4ca7-9010-089c7a6e8837");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "6464ab44-a2f7-46d1-bfad-f4c84ea84d51");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "84615b0e-0358-467e-b0aa-d9489e8b605d");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "38b68739-fac5-40dc-b487-6907166ab920", null, "Admin", "ADMIN" },
                    { "9c0e0215-4419-4bef-a4e0-0ae0c05cd260", null, "Instructor", "INSTRUCTOR" },
                    { "9e7f6c03-01d6-43a3-99ea-e2c05bd1b08a", null, "Student", "STUDENT" }
                });
        }
    }
}
