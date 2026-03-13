using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoX.Gara.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateNewStructs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Invoice_Customer_CustomerId",
                table: "Invoice");

            migrationBuilder.DropForeignKey(
                name: "FK_Invoice_Employee_CreatedById",
                table: "Invoice");

            migrationBuilder.DropForeignKey(
                name: "FK_Invoice_Employee_ModifiedById",
                table: "Invoice");

            migrationBuilder.DropForeignKey(
                name: "FK_RepairOrder_Customer_CustomerId",
                table: "RepairOrder");

            migrationBuilder.DropForeignKey(
                name: "FK_RepairOrder_Customer_CustomerId1",
                table: "RepairOrder");

            migrationBuilder.DropForeignKey(
                name: "FK_RepairOrder_Invoice_InvoiceId",
                table: "RepairOrder");

            migrationBuilder.DropForeignKey(
                name: "FK_RepairOrder_Invoice_InvoiceId1",
                table: "RepairOrder");

            migrationBuilder.DropForeignKey(
                name: "FK_RepairOrder_Vehicle_VehicleId",
                table: "RepairOrder");

            migrationBuilder.DropForeignKey(
                name: "FK_RepairOrder_Vehicle_VehicleId1",
                table: "RepairOrder");

            migrationBuilder.DropForeignKey(
                name: "FK_RepairOrderItem_Part_PartId",
                table: "RepairOrderItem");

            migrationBuilder.DropForeignKey(
                name: "FK_Transaction_Invoice_InvoiceId",
                table: "Transaction");

            migrationBuilder.DropForeignKey(
                name: "FK_Transaction_Invoice_InvoiceId1",
                table: "Transaction");

            migrationBuilder.DropIndex(
                name: "IX_Transaction_CreatedBy",
                table: "Transaction");

            migrationBuilder.DropIndex(
                name: "IX_Transaction_InvoiceId1",
                table: "Transaction");

            migrationBuilder.DropIndex(
                name: "IX_RepairOrder_CustomerId1",
                table: "RepairOrder");

            migrationBuilder.DropIndex(
                name: "IX_RepairOrder_InvoiceId1",
                table: "RepairOrder");

            migrationBuilder.DropIndex(
                name: "IX_RepairOrder_VehicleId1",
                table: "RepairOrder");

            migrationBuilder.DropIndex(
                name: "IX_Invoice_CreatedById",
                table: "Invoice");

            migrationBuilder.DropIndex(
                name: "IX_Invoice_ModifiedById",
                table: "Invoice");

            migrationBuilder.DropIndex(
                name: "IX_Employee_PhoneNumber",
                table: "Employee");

            migrationBuilder.DropColumn(
                name: "InvoiceId1",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "CustomerId1",
                table: "RepairOrder");

            migrationBuilder.DropColumn(
                name: "InvoiceId1",
                table: "RepairOrder");

            migrationBuilder.DropColumn(
                name: "VehicleId1",
                table: "RepairOrder");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "Invoice");

            migrationBuilder.DropColumn(
                name: "ModifiedById",
                table: "Invoice");

            migrationBuilder.AlterColumn<int>(
                name: "InvoiceId",
                table: "RepairOrder",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.CreateIndex(
                name: "IX_Employee_Gender",
                table: "Employee",
                column: "Gender");

            migrationBuilder.CreateIndex(
                name: "IX_Employee_Name",
                table: "Employee",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Employee_Position",
                table: "Employee",
                column: "Position");

            migrationBuilder.CreateIndex(
                name: "IX_Employee_StartDate",
                table: "Employee",
                column: "StartDate");

            migrationBuilder.AddForeignKey(
                name: "FK_RepairOrder_Customer_CustomerId",
                table: "RepairOrder",
                column: "CustomerId",
                principalTable: "Customer",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RepairOrder_Invoice_InvoiceId",
                table: "RepairOrder",
                column: "InvoiceId",
                principalTable: "Invoice",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_RepairOrder_Vehicle_VehicleId",
                table: "RepairOrder",
                column: "VehicleId",
                principalTable: "Vehicle",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RepairOrderItem_Part_PartId",
                table: "RepairOrderItem",
                column: "PartId",
                principalTable: "Part",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Transaction_Invoice_InvoiceId",
                table: "Transaction",
                column: "InvoiceId",
                principalTable: "Invoice",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RepairOrder_Customer_CustomerId",
                table: "RepairOrder");

            migrationBuilder.DropForeignKey(
                name: "FK_RepairOrder_Invoice_InvoiceId",
                table: "RepairOrder");

            migrationBuilder.DropForeignKey(
                name: "FK_RepairOrder_Vehicle_VehicleId",
                table: "RepairOrder");

            migrationBuilder.DropForeignKey(
                name: "FK_RepairOrderItem_Part_PartId",
                table: "RepairOrderItem");

            migrationBuilder.DropForeignKey(
                name: "FK_Transaction_Invoice_InvoiceId",
                table: "Transaction");

            migrationBuilder.DropIndex(
                name: "IX_Employee_Gender",
                table: "Employee");

            migrationBuilder.DropIndex(
                name: "IX_Employee_Name",
                table: "Employee");

            migrationBuilder.DropIndex(
                name: "IX_Employee_Position",
                table: "Employee");

            migrationBuilder.DropIndex(
                name: "IX_Employee_StartDate",
                table: "Employee");

            migrationBuilder.AddColumn<int>(
                name: "InvoiceId1",
                table: "Transaction",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "InvoiceId",
                table: "RepairOrder",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CustomerId1",
                table: "RepairOrder",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "InvoiceId1",
                table: "RepairOrder",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VehicleId1",
                table: "RepairOrder",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedById",
                table: "Invoice",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ModifiedById",
                table: "Invoice",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_CreatedBy",
                table: "Transaction",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_InvoiceId1",
                table: "Transaction",
                column: "InvoiceId1");

            migrationBuilder.CreateIndex(
                name: "IX_RepairOrder_CustomerId1",
                table: "RepairOrder",
                column: "CustomerId1");

            migrationBuilder.CreateIndex(
                name: "IX_RepairOrder_InvoiceId1",
                table: "RepairOrder",
                column: "InvoiceId1");

            migrationBuilder.CreateIndex(
                name: "IX_RepairOrder_VehicleId1",
                table: "RepairOrder",
                column: "VehicleId1");

            migrationBuilder.CreateIndex(
                name: "IX_Invoice_CreatedById",
                table: "Invoice",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Invoice_ModifiedById",
                table: "Invoice",
                column: "ModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_Employee_PhoneNumber",
                table: "Employee",
                column: "PhoneNumber");

            migrationBuilder.AddForeignKey(
                name: "FK_Invoice_Customer_CustomerId",
                table: "Invoice",
                column: "CustomerId",
                principalTable: "Customer",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Invoice_Employee_CreatedById",
                table: "Invoice",
                column: "CreatedById",
                principalTable: "Employee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Invoice_Employee_ModifiedById",
                table: "Invoice",
                column: "ModifiedById",
                principalTable: "Employee",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RepairOrder_Customer_CustomerId",
                table: "RepairOrder",
                column: "CustomerId",
                principalTable: "Customer",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RepairOrder_Customer_CustomerId1",
                table: "RepairOrder",
                column: "CustomerId1",
                principalTable: "Customer",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RepairOrder_Invoice_InvoiceId",
                table: "RepairOrder",
                column: "InvoiceId",
                principalTable: "Invoice",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RepairOrder_Invoice_InvoiceId1",
                table: "RepairOrder",
                column: "InvoiceId1",
                principalTable: "Invoice",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RepairOrder_Vehicle_VehicleId",
                table: "RepairOrder",
                column: "VehicleId",
                principalTable: "Vehicle",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_RepairOrder_Vehicle_VehicleId1",
                table: "RepairOrder",
                column: "VehicleId1",
                principalTable: "Vehicle",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RepairOrderItem_Part_PartId",
                table: "RepairOrderItem",
                column: "PartId",
                principalTable: "Part",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Transaction_Invoice_InvoiceId",
                table: "Transaction",
                column: "InvoiceId",
                principalTable: "Invoice",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Transaction_Invoice_InvoiceId1",
                table: "Transaction",
                column: "InvoiceId1",
                principalTable: "Invoice",
                principalColumn: "Id");
        }
    }
}
