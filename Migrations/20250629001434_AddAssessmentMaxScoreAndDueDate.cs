using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddAssessmentMaxScoreAndDueDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AddColumn<DateTime>(
                name: "DueDate",
                table: "Assessments",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "MaxScore",
                table: "Assessments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "48ff61d4-3378-4940-b8d0-ed4df117164a", null, "Admin", "ADMIN" },
                    { "bc00a991-ad54-4400-a0b0-2b95fae83d5a", null, "Student", "STUDENT" },
                    { "cd055aad-6ffd-44ae-8e1c-2648446b2f86", null, "Instructor", "INSTRUCTOR" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "48ff61d4-3378-4940-b8d0-ed4df117164a");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "bc00a991-ad54-4400-a0b0-2b95fae83d5a");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "cd055aad-6ffd-44ae-8e1c-2648446b2f86");

            migrationBuilder.DropColumn(
                name: "DueDate",
                table: "Assessments");

            migrationBuilder.DropColumn(
                name: "MaxScore",
                table: "Assessments");

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
    }
}
