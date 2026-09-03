using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FactoryX.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Phase8_ProductionExecution : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FinishedGoodsStocks_ProductionBatches_ProductionBatchId",
                table: "FinishedGoodsStocks");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionBatches_RecipeVersions_RecipeVersionId",
                table: "ProductionBatches");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionBatches_Warehouses_TargetWarehouseId",
                table: "ProductionBatches");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionBatches_machines_MachineId",
                table: "ProductionBatches");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionBatches_operators_OperatorId",
                table: "ProductionBatches");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionBatches_products_ProductId",
                table: "ProductionBatches");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionBatches_shifts_ShiftId",
                table: "ProductionBatches");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionBatches_work_orders_WorkOrderId",
                table: "ProductionBatches");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionConsumptions_ProductionBatches_ProductionBatchId",
                table: "ProductionConsumptions");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionConsumptions_Warehouses_WarehouseId",
                table: "ProductionConsumptions");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionConsumptions_materials_MaterialId",
                table: "ProductionConsumptions");

            migrationBuilder.DropForeignKey(
                name: "FK_QualityInspections_ProductionBatches_ProductionBatchId",
                table: "QualityInspections");

            migrationBuilder.DropForeignKey(
                name: "FK_Wastes_ProductionBatches_ProductionBatchId",
                table: "Wastes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductionConsumptions",
                table: "ProductionConsumptions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductionBatches",
                table: "ProductionBatches");

            migrationBuilder.RenameTable(
                name: "ProductionConsumptions",
                newName: "production_consumptions");

            migrationBuilder.RenameTable(
                name: "ProductionBatches",
                newName: "production_batches");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "production_consumptions",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "production_consumptions",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "production_consumptions",
                newName: "created_at");

            migrationBuilder.RenameIndex(
                name: "IX_ProductionConsumptions_WarehouseId",
                table: "production_consumptions",
                newName: "IX_production_consumptions_WarehouseId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductionConsumptions_ProductionBatchId",
                table: "production_consumptions",
                newName: "IX_production_consumptions_ProductionBatchId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductionConsumptions_MaterialId",
                table: "production_consumptions",
                newName: "IX_production_consumptions_MaterialId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "production_batches",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "production_batches",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "production_batches",
                newName: "created_at");

            migrationBuilder.RenameIndex(
                name: "IX_ProductionBatches_WorkOrderId",
                table: "production_batches",
                newName: "IX_production_batches_WorkOrderId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductionBatches_TargetWarehouseId",
                table: "production_batches",
                newName: "IX_production_batches_TargetWarehouseId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductionBatches_ShiftId",
                table: "production_batches",
                newName: "IX_production_batches_ShiftId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductionBatches_RecipeVersionId",
                table: "production_batches",
                newName: "IX_production_batches_RecipeVersionId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductionBatches_ProductId",
                table: "production_batches",
                newName: "IX_production_batches_ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductionBatches_OperatorId",
                table: "production_batches",
                newName: "IX_production_batches_OperatorId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductionBatches_MachineId",
                table: "production_batches",
                newName: "IX_production_batches_MachineId");

            migrationBuilder.AddColumn<int>(
                name: "ProductionBatchId",
                table: "production_records",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "PlannedQuantity",
                table: "production_consumptions",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_at",
                table: "production_consumptions",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "production_consumptions",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<decimal>(
                name: "ActualQuantity",
                table: "production_consumptions",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiryDate",
                table: "production_consumptions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "InventoryTransactionId",
                table: "production_consumptions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LocationId",
                table: "production_consumptions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "production_consumptions",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RawMaterialBatchNumber",
                table: "production_consumptions",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Unit",
                table: "production_consumptions",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "KG");

            migrationBuilder.AddColumn<decimal>(
                name: "Variance",
                table: "production_consumptions",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<string>(
                name: "QualityStatus",
                table: "production_batches",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Pending",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ExpiryDate",
                table: "production_batches",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "BatchNumber",
                table: "production_batches",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_at",
                table: "production_batches",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "production_batches",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<decimal>(
                name: "ActualOutputQuantity",
                table: "production_batches",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "CancellationReason",
                table: "production_batches",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EndTime",
                table: "production_batches",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "production_batches",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OutputUnit",
                table: "production_batches",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "KG");

            migrationBuilder.AddColumn<DateTime>(
                name: "PauseTime",
                table: "production_batches",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PlannedQuantity",
                table: "production_batches",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "ProductionLineId",
                table: "production_batches",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartTime",
                table: "production_batches",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "production_batches",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WorkCenterId",
                table: "production_batches",
                type: "int",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_production_consumptions",
                table: "production_consumptions",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_production_batches",
                table: "production_batches",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "IX_production_records_ProductionBatchId",
                table: "production_records",
                column: "ProductionBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_production_consumptions_InventoryTransactionId",
                table: "production_consumptions",
                column: "InventoryTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_production_consumptions_LocationId",
                table: "production_consumptions",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_production_batches_BatchNumber",
                table: "production_batches",
                column: "BatchNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_production_batches_ProductionLineId",
                table: "production_batches",
                column: "ProductionLineId");

            migrationBuilder.CreateIndex(
                name: "IX_production_batches_WorkCenterId",
                table: "production_batches",
                column: "WorkCenterId");

            migrationBuilder.AddForeignKey(
                name: "FK_FinishedGoodsStocks_production_batches_ProductionBatchId",
                table: "FinishedGoodsStocks",
                column: "ProductionBatchId",
                principalTable: "production_batches",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_production_batches_ProductionLines_ProductionLineId",
                table: "production_batches",
                column: "ProductionLineId",
                principalTable: "ProductionLines",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_production_batches_RecipeVersions_RecipeVersionId",
                table: "production_batches",
                column: "RecipeVersionId",
                principalTable: "RecipeVersions",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_production_batches_Warehouses_TargetWarehouseId",
                table: "production_batches",
                column: "TargetWarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_production_batches_WorkCenters_WorkCenterId",
                table: "production_batches",
                column: "WorkCenterId",
                principalTable: "WorkCenters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_production_batches_machines_MachineId",
                table: "production_batches",
                column: "MachineId",
                principalTable: "machines",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_production_batches_operators_OperatorId",
                table: "production_batches",
                column: "OperatorId",
                principalTable: "operators",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_production_batches_products_ProductId",
                table: "production_batches",
                column: "ProductId",
                principalTable: "products",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_production_batches_shifts_ShiftId",
                table: "production_batches",
                column: "ShiftId",
                principalTable: "shifts",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_production_batches_work_orders_WorkOrderId",
                table: "production_batches",
                column: "WorkOrderId",
                principalTable: "work_orders",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_production_consumptions_InventoryTransactions_InventoryTransactionId",
                table: "production_consumptions",
                column: "InventoryTransactionId",
                principalTable: "InventoryTransactions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_production_consumptions_WarehouseLocations_LocationId",
                table: "production_consumptions",
                column: "LocationId",
                principalTable: "WarehouseLocations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_production_consumptions_Warehouses_WarehouseId",
                table: "production_consumptions",
                column: "WarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_production_consumptions_materials_MaterialId",
                table: "production_consumptions",
                column: "MaterialId",
                principalTable: "materials",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_production_consumptions_production_batches_ProductionBatchId",
                table: "production_consumptions",
                column: "ProductionBatchId",
                principalTable: "production_batches",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_production_records_production_batches_ProductionBatchId",
                table: "production_records",
                column: "ProductionBatchId",
                principalTable: "production_batches",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_QualityInspections_production_batches_ProductionBatchId",
                table: "QualityInspections",
                column: "ProductionBatchId",
                principalTable: "production_batches",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Wastes_production_batches_ProductionBatchId",
                table: "Wastes",
                column: "ProductionBatchId",
                principalTable: "production_batches",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FinishedGoodsStocks_production_batches_ProductionBatchId",
                table: "FinishedGoodsStocks");

            migrationBuilder.DropForeignKey(
                name: "FK_production_batches_ProductionLines_ProductionLineId",
                table: "production_batches");

            migrationBuilder.DropForeignKey(
                name: "FK_production_batches_RecipeVersions_RecipeVersionId",
                table: "production_batches");

            migrationBuilder.DropForeignKey(
                name: "FK_production_batches_Warehouses_TargetWarehouseId",
                table: "production_batches");

            migrationBuilder.DropForeignKey(
                name: "FK_production_batches_WorkCenters_WorkCenterId",
                table: "production_batches");

            migrationBuilder.DropForeignKey(
                name: "FK_production_batches_machines_MachineId",
                table: "production_batches");

            migrationBuilder.DropForeignKey(
                name: "FK_production_batches_operators_OperatorId",
                table: "production_batches");

            migrationBuilder.DropForeignKey(
                name: "FK_production_batches_products_ProductId",
                table: "production_batches");

            migrationBuilder.DropForeignKey(
                name: "FK_production_batches_shifts_ShiftId",
                table: "production_batches");

            migrationBuilder.DropForeignKey(
                name: "FK_production_batches_work_orders_WorkOrderId",
                table: "production_batches");

            migrationBuilder.DropForeignKey(
                name: "FK_production_consumptions_InventoryTransactions_InventoryTransactionId",
                table: "production_consumptions");

            migrationBuilder.DropForeignKey(
                name: "FK_production_consumptions_WarehouseLocations_LocationId",
                table: "production_consumptions");

            migrationBuilder.DropForeignKey(
                name: "FK_production_consumptions_Warehouses_WarehouseId",
                table: "production_consumptions");

            migrationBuilder.DropForeignKey(
                name: "FK_production_consumptions_materials_MaterialId",
                table: "production_consumptions");

            migrationBuilder.DropForeignKey(
                name: "FK_production_consumptions_production_batches_ProductionBatchId",
                table: "production_consumptions");

            migrationBuilder.DropForeignKey(
                name: "FK_production_records_production_batches_ProductionBatchId",
                table: "production_records");

            migrationBuilder.DropForeignKey(
                name: "FK_QualityInspections_production_batches_ProductionBatchId",
                table: "QualityInspections");

            migrationBuilder.DropForeignKey(
                name: "FK_Wastes_production_batches_ProductionBatchId",
                table: "Wastes");

            migrationBuilder.DropIndex(
                name: "IX_production_records_ProductionBatchId",
                table: "production_records");

            migrationBuilder.DropPrimaryKey(
                name: "PK_production_consumptions",
                table: "production_consumptions");

            migrationBuilder.DropIndex(
                name: "IX_production_consumptions_InventoryTransactionId",
                table: "production_consumptions");

            migrationBuilder.DropIndex(
                name: "IX_production_consumptions_LocationId",
                table: "production_consumptions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_production_batches",
                table: "production_batches");

            migrationBuilder.DropIndex(
                name: "IX_production_batches_BatchNumber",
                table: "production_batches");

            migrationBuilder.DropIndex(
                name: "IX_production_batches_ProductionLineId",
                table: "production_batches");

            migrationBuilder.DropIndex(
                name: "IX_production_batches_WorkCenterId",
                table: "production_batches");

            migrationBuilder.DropColumn(
                name: "ProductionBatchId",
                table: "production_records");

            migrationBuilder.DropColumn(
                name: "ActualQuantity",
                table: "production_consumptions");

            migrationBuilder.DropColumn(
                name: "ExpiryDate",
                table: "production_consumptions");

            migrationBuilder.DropColumn(
                name: "InventoryTransactionId",
                table: "production_consumptions");

            migrationBuilder.DropColumn(
                name: "LocationId",
                table: "production_consumptions");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "production_consumptions");

            migrationBuilder.DropColumn(
                name: "RawMaterialBatchNumber",
                table: "production_consumptions");

            migrationBuilder.DropColumn(
                name: "Unit",
                table: "production_consumptions");

            migrationBuilder.DropColumn(
                name: "Variance",
                table: "production_consumptions");

            migrationBuilder.DropColumn(
                name: "ActualOutputQuantity",
                table: "production_batches");

            migrationBuilder.DropColumn(
                name: "CancellationReason",
                table: "production_batches");

            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "production_batches");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "production_batches");

            migrationBuilder.DropColumn(
                name: "OutputUnit",
                table: "production_batches");

            migrationBuilder.DropColumn(
                name: "PauseTime",
                table: "production_batches");

            migrationBuilder.DropColumn(
                name: "PlannedQuantity",
                table: "production_batches");

            migrationBuilder.DropColumn(
                name: "ProductionLineId",
                table: "production_batches");

            migrationBuilder.DropColumn(
                name: "StartTime",
                table: "production_batches");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "production_batches");

            migrationBuilder.DropColumn(
                name: "WorkCenterId",
                table: "production_batches");

            migrationBuilder.RenameTable(
                name: "production_consumptions",
                newName: "ProductionConsumptions");

            migrationBuilder.RenameTable(
                name: "production_batches",
                newName: "ProductionBatches");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "ProductionConsumptions",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "ProductionConsumptions",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "ProductionConsumptions",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_production_consumptions_WarehouseId",
                table: "ProductionConsumptions",
                newName: "IX_ProductionConsumptions_WarehouseId");

            migrationBuilder.RenameIndex(
                name: "IX_production_consumptions_ProductionBatchId",
                table: "ProductionConsumptions",
                newName: "IX_ProductionConsumptions_ProductionBatchId");

            migrationBuilder.RenameIndex(
                name: "IX_production_consumptions_MaterialId",
                table: "ProductionConsumptions",
                newName: "IX_ProductionConsumptions_MaterialId");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "ProductionBatches",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "ProductionBatches",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "ProductionBatches",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_production_batches_WorkOrderId",
                table: "ProductionBatches",
                newName: "IX_ProductionBatches_WorkOrderId");

            migrationBuilder.RenameIndex(
                name: "IX_production_batches_TargetWarehouseId",
                table: "ProductionBatches",
                newName: "IX_ProductionBatches_TargetWarehouseId");

            migrationBuilder.RenameIndex(
                name: "IX_production_batches_ShiftId",
                table: "ProductionBatches",
                newName: "IX_ProductionBatches_ShiftId");

            migrationBuilder.RenameIndex(
                name: "IX_production_batches_RecipeVersionId",
                table: "ProductionBatches",
                newName: "IX_ProductionBatches_RecipeVersionId");

            migrationBuilder.RenameIndex(
                name: "IX_production_batches_ProductId",
                table: "ProductionBatches",
                newName: "IX_ProductionBatches_ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_production_batches_OperatorId",
                table: "ProductionBatches",
                newName: "IX_ProductionBatches_OperatorId");

            migrationBuilder.RenameIndex(
                name: "IX_production_batches_MachineId",
                table: "ProductionBatches",
                newName: "IX_ProductionBatches_MachineId");

            migrationBuilder.AlterColumn<decimal>(
                name: "PlannedQuantity",
                table: "ProductionConsumptions",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldPrecision: 18,
                oldScale: 4);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "ProductionConsumptions",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "ProductionConsumptions",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<string>(
                name: "QualityStatus",
                table: "ProductionBatches",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldDefaultValue: "Pending");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ExpiryDate",
                table: "ProductionBatches",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BatchNumber",
                table: "ProductionBatches",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "ProductionBatches",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "ProductionBatches",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductionConsumptions",
                table: "ProductionConsumptions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductionBatches",
                table: "ProductionBatches",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FinishedGoodsStocks_ProductionBatches_ProductionBatchId",
                table: "FinishedGoodsStocks",
                column: "ProductionBatchId",
                principalTable: "ProductionBatches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionBatches_RecipeVersions_RecipeVersionId",
                table: "ProductionBatches",
                column: "RecipeVersionId",
                principalTable: "RecipeVersions",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionBatches_Warehouses_TargetWarehouseId",
                table: "ProductionBatches",
                column: "TargetWarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionBatches_machines_MachineId",
                table: "ProductionBatches",
                column: "MachineId",
                principalTable: "machines",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionBatches_operators_OperatorId",
                table: "ProductionBatches",
                column: "OperatorId",
                principalTable: "operators",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionBatches_products_ProductId",
                table: "ProductionBatches",
                column: "ProductId",
                principalTable: "products",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionBatches_shifts_ShiftId",
                table: "ProductionBatches",
                column: "ShiftId",
                principalTable: "shifts",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionBatches_work_orders_WorkOrderId",
                table: "ProductionBatches",
                column: "WorkOrderId",
                principalTable: "work_orders",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionConsumptions_ProductionBatches_ProductionBatchId",
                table: "ProductionConsumptions",
                column: "ProductionBatchId",
                principalTable: "ProductionBatches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionConsumptions_Warehouses_WarehouseId",
                table: "ProductionConsumptions",
                column: "WarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionConsumptions_materials_MaterialId",
                table: "ProductionConsumptions",
                column: "MaterialId",
                principalTable: "materials",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_QualityInspections_ProductionBatches_ProductionBatchId",
                table: "QualityInspections",
                column: "ProductionBatchId",
                principalTable: "ProductionBatches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Wastes_ProductionBatches_ProductionBatchId",
                table: "Wastes",
                column: "ProductionBatchId",
                principalTable: "ProductionBatches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
