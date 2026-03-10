using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoX.Gara.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftDeleteToCustomer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Customer",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "Gender",
                table: "Customer",
                type: "INTEGER",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Customer",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Customer");
        }
    }
}
