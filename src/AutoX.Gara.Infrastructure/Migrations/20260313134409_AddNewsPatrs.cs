using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoX.Gara.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNewsPatrs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RepairOrderItem_SparePart_SparePartId",
                table: "RepairOrderItem");

            migrationBuilder.DropTable(
                name: "ReplacementPart");

            migrationBuilder.DropTable(
                name: "SparePart");

            migrationBuilder.RenameColumn(
                name: "SparePartId",
                table: "RepairOrderItem",
                newName: "PartId");

            migrationBuilder.RenameIndex(
                name: "IX_RepairOrderItem_SparePartId",
                table: "RepairOrderItem",
                newName: "IX_RepairOrderItem_PartId");

            migrationBuilder.CreateTable(
                name: "Part",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SupplierId = table.Column<int>(type: "INTEGER", nullable: false),
                    PartCode = table.Column<string>(type: "TEXT", maxLength: 12, nullable: false),
                    PartName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Manufacturer = table.Column<string>(type: "TEXT", maxLength: 75, nullable: false),
                    PartCategory = table.Column<byte>(type: "INTEGER", nullable: false),
                    InventoryQuantity = table.Column<int>(type: "INTEGER", nullable: false),
                    PurchasePrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    SellingPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    IsDefective = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsDiscontinued = table.Column<bool>(type: "INTEGER", nullable: false),
                    DateAdded = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Part", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Part_Supplier_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Supplier",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Part_IsDiscontinued_IsDefective_InventoryQuantity",
                table: "Part",
                columns: new[] { "IsDiscontinued", "IsDefective", "InventoryQuantity" });

            migrationBuilder.CreateIndex(
                name: "IX_Part_Manufacturer",
                table: "Part",
                column: "Manufacturer");

            migrationBuilder.CreateIndex(
                name: "IX_Part_PartCode",
                table: "Part",
                column: "PartCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Part_PartName",
                table: "Part",
                column: "PartName");

            migrationBuilder.CreateIndex(
                name: "IX_Part_SupplierId",
                table: "Part",
                column: "SupplierId");

            migrationBuilder.AddForeignKey(
                name: "FK_RepairOrderItem_Part_PartId",
                table: "RepairOrderItem",
                column: "PartId",
                principalTable: "Part",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RepairOrderItem_Part_PartId",
                table: "RepairOrderItem");

            migrationBuilder.DropTable(
                name: "Part");

            migrationBuilder.RenameColumn(
                name: "PartId",
                table: "RepairOrderItem",
                newName: "SparePartId");

            migrationBuilder.RenameIndex(
                name: "IX_RepairOrderItem_PartId",
                table: "RepairOrderItem",
                newName: "IX_RepairOrderItem_SparePartId");

            migrationBuilder.CreateTable(
                name: "ReplacementPart",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DateAdded = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDefective = table.Column<bool>(type: "INTEGER", nullable: false),
                    Manufacturer = table.Column<string>(type: "TEXT", maxLength: 75, nullable: false),
                    PartCode = table.Column<string>(type: "TEXT", maxLength: 12, nullable: false),
                    PartName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Quantity = table.Column<int>(type: "INTEGER", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReplacementPart", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SparePart",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SupplierId = table.Column<int>(type: "INTEGER", nullable: false),
                    InventoryQuantity = table.Column<int>(type: "INTEGER", nullable: false),
                    IsDiscontinued = table.Column<bool>(type: "INTEGER", nullable: false),
                    PartCategory = table.Column<byte>(type: "INTEGER", nullable: false),
                    PartName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    PurchasePrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    SellingPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SparePart", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SparePart_Supplier_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Supplier",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReplacementPart_Manufacturer",
                table: "ReplacementPart",
                column: "Manufacturer");

            migrationBuilder.CreateIndex(
                name: "IX_ReplacementPart_PartCode",
                table: "ReplacementPart",
                column: "PartCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SparePart_PartName",
                table: "SparePart",
                column: "PartName");

            migrationBuilder.CreateIndex(
                name: "IX_SparePart_SupplierId_PartName",
                table: "SparePart",
                columns: new[] { "SupplierId", "PartName" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_RepairOrderItem_SparePart_SparePartId",
                table: "RepairOrderItem",
                column: "SparePartId",
                principalTable: "SparePart",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
