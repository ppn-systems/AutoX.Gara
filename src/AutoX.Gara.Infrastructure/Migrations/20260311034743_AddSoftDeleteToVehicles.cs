using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoX.Gara.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftDeleteToVehicles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Vehicle",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Vehicle");
        }
    }
}
