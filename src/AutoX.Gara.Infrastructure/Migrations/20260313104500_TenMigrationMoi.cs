using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoX.Gara.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class TenMigrationMoi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ReplacementPart",
                keyColumn: "Id",
                keyValue: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "ReplacementPart",
                columns: new[] { "Id", "DateAdded", "ExpiryDate", "IsDefective", "Manufacturer", "PartCode", "PartName", "Quantity", "UnitPrice" },
                values: new object[] { 1, new DateTime(2025, 2, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2029, 2, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "OEM", "ABC123", "Brake Pad", 0, 150.50m });
        }
    }
}
