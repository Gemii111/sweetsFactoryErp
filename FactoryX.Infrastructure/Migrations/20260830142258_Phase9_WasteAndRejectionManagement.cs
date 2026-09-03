using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FactoryX.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Phase9_WasteAndRejectionManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Unit",
                table: "Wastes",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "ReasonDescription",
                table: "Wastes",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Quantity",
                table: "Wastes",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<string>(
                name: "ApprovalStatus",
                table: "Wastes",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "ApprovalNotes",
                table: "Wastes",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAt",
                table: "Wastes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ApprovedByUserId",
                table: "Wastes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedByUserId",
                table: "Wastes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "InventoryTransactionId",
                table: "Wastes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LocationId",
                table: "Wastes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Wastes",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProductionBatchId1",
                table: "Wastes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RawMaterialBatchNumber",
                table: "Wastes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Wastes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WarehouseId",
                table: "Wastes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WasteNumber",
                table: "Wastes",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "WasteType",
                table: "Wastes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WorkOrderId",
                table: "Wastes",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Reason",
                table: "WasteReasons",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "WasteReasons",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "WasteReasons",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "WasteReasons",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.CreateIndex(
                name: "IX_Wastes_ApprovedByUserId",
                table: "Wastes",
                column: "ApprovedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Wastes_CreatedByUserId",
                table: "Wastes",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Wastes_InventoryTransactionId",
                table: "Wastes",
                column: "InventoryTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_Wastes_LocationId",
                table: "Wastes",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_Wastes_ProductionBatchId1",
                table: "Wastes",
                column: "ProductionBatchId1");

            migrationBuilder.CreateIndex(
                name: "IX_Wastes_WarehouseId",
                table: "Wastes",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_Wastes_WasteNumber",
                table: "Wastes",
                column: "WasteNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Wastes_WorkOrderId",
                table: "Wastes",
                column: "WorkOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_WasteReasons_Code",
                table: "WasteReasons",
                column: "Code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Wastes_InventoryTransactions_InventoryTransactionId",
                table: "Wastes",
                column: "InventoryTransactionId",
                principalTable: "InventoryTransactions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Wastes_WarehouseLocations_LocationId",
                table: "Wastes",
                column: "LocationId",
                principalTable: "WarehouseLocations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Wastes_Warehouses_WarehouseId",
                table: "Wastes",
                column: "WarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Wastes_production_batches_ProductionBatchId1",
                table: "Wastes",
                column: "ProductionBatchId1",
                principalTable: "production_batches",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Wastes_users_ApprovedByUserId",
                table: "Wastes",
                column: "ApprovedByUserId",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Wastes_users_CreatedByUserId",
                table: "Wastes",
                column: "CreatedByUserId",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Wastes_work_orders_WorkOrderId",
                table: "Wastes",
                column: "WorkOrderId",
                principalTable: "work_orders",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Wastes_InventoryTransactions_InventoryTransactionId",
                table: "Wastes");

            migrationBuilder.DropForeignKey(
                name: "FK_Wastes_WarehouseLocations_LocationId",
                table: "Wastes");

            migrationBuilder.DropForeignKey(
                name: "FK_Wastes_Warehouses_WarehouseId",
                table: "Wastes");

            migrationBuilder.DropForeignKey(
                name: "FK_Wastes_production_batches_ProductionBatchId1",
                table: "Wastes");

            migrationBuilder.DropForeignKey(
                name: "FK_Wastes_users_ApprovedByUserId",
                table: "Wastes");

            migrationBuilder.DropForeignKey(
                name: "FK_Wastes_users_CreatedByUserId",
                table: "Wastes");

            migrationBuilder.DropForeignKey(
                name: "FK_Wastes_work_orders_WorkOrderId",
                table: "Wastes");

            migrationBuilder.DropIndex(
                name: "IX_Wastes_ApprovedByUserId",
                table: "Wastes");

            migrationBuilder.DropIndex(
                name: "IX_Wastes_CreatedByUserId",
                table: "Wastes");

            migrationBuilder.DropIndex(
                name: "IX_Wastes_InventoryTransactionId",
                table: "Wastes");

            migrationBuilder.DropIndex(
                name: "IX_Wastes_LocationId",
                table: "Wastes");

            migrationBuilder.DropIndex(
                name: "IX_Wastes_ProductionBatchId1",
                table: "Wastes");

            migrationBuilder.DropIndex(
                name: "IX_Wastes_WarehouseId",
                table: "Wastes");

            migrationBuilder.DropIndex(
                name: "IX_Wastes_WasteNumber",
                table: "Wastes");

            migrationBuilder.DropIndex(
                name: "IX_Wastes_WorkOrderId",
                table: "Wastes");

            migrationBuilder.DropIndex(
                name: "IX_WasteReasons_Code",
                table: "WasteReasons");

            migrationBuilder.DropColumn(
                name: "ApprovalNotes",
                table: "Wastes");

            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                table: "Wastes");

            migrationBuilder.DropColumn(
                name: "ApprovedByUserId",
                table: "Wastes");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Wastes");

            migrationBuilder.DropColumn(
                name: "InventoryTransactionId",
                table: "Wastes");

            migrationBuilder.DropColumn(
                name: "LocationId",
                table: "Wastes");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Wastes");

            migrationBuilder.DropColumn(
                name: "ProductionBatchId1",
                table: "Wastes");

            migrationBuilder.DropColumn(
                name: "RawMaterialBatchNumber",
                table: "Wastes");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Wastes");

            migrationBuilder.DropColumn(
                name: "WarehouseId",
                table: "Wastes");

            migrationBuilder.DropColumn(
                name: "WasteNumber",
                table: "Wastes");

            migrationBuilder.DropColumn(
                name: "WasteType",
                table: "Wastes");

            migrationBuilder.DropColumn(
                name: "WorkOrderId",
                table: "Wastes");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "WasteReasons");

            migrationBuilder.AlterColumn<string>(
                name: "Unit",
                table: "Wastes",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "ReasonDescription",
                table: "Wastes",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<decimal>(
                name: "Quantity",
                table: "Wastes",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldPrecision: 18,
                oldScale: 4);

            migrationBuilder.AlterColumn<string>(
                name: "ApprovalStatus",
                table: "Wastes",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Reason",
                table: "WasteReasons",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "WasteReasons",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "WasteReasons",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);
        }
    }
}
