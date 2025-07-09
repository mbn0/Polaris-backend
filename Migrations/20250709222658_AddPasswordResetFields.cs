using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddPasswordResetFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AddColumn<string>(
                name: "PasswordResetOtp",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PasswordResetOtpExpiry",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "10592fbb-bde4-4c4f-b98e-a8e7d33ca84d", null, "Admin", "ADMIN" },
                    { "ddb27d9e-9b97-4d09-98e8-ae326c9a0906", null, "Instructor", "INSTRUCTOR" },
                    { "dedc437e-81b3-4e78-a016-6ee5673edb65", null, "Student", "STUDENT" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "10592fbb-bde4-4c4f-b98e-a8e7d33ca84d");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "ddb27d9e-9b97-4d09-98e8-ae326c9a0906");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "dedc437e-81b3-4e78-a016-6ee5673edb65");

            migrationBuilder.DropColumn(
                name: "PasswordResetOtp",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "PasswordResetOtpExpiry",
                table: "AspNetUsers");

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
    }
}
