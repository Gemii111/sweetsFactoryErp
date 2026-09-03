using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FactoryX.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Phase7_ProductionPlanning : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "work_orders",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<string>(
                name: "OrderNumber",
                table: "work_orders",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<int>(
                name: "MachineId",
                table: "work_orders",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<decimal>(
                name: "ActualQuantityDecimal",
                table: "work_orders",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "DueDate",
                table: "work_orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "work_orders",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OrderStatus",
                table: "work_orders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "OutputUnit",
                table: "work_orders",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "KG");

            migrationBuilder.AddColumn<DateTime>(
                name: "PlannedDate",
                table: "work_orders",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<decimal>(
                name: "PlannedQuantity",
                table: "work_orders",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "Priority",
                table: "work_orders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ProductionAreaId",
                table: "work_orders",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProductionLineId",
                table: "work_orders",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RecipeId",
                table: "work_orders",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "work_order_material_requirements",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkOrderId = table.Column<int>(type: "int", nullable: false),
                    MaterialId = table.Column<int>(type: "int", nullable: false),
                    MaterialCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MaterialName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    MaterialArabicName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    StockUnit = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "KG"),
                    RecipeQuantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    ExpectedOutputQuantity = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PlannedProductionQuantity = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    RequiredQuantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    AllocatedQuantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false, defaultValue: 0m),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_work_order_material_requirements", x => x.id);
                    table.ForeignKey(
                        name: "FK_work_order_material_requirements_materials_MaterialId",
                        column: x => x.MaterialId,
                        principalTable: "materials",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_work_order_material_requirements_work_orders_WorkOrderId",
                        column: x => x.WorkOrderId,
                        principalTable: "work_orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_work_orders_OrderNumber",
                table: "work_orders",
                column: "OrderNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_work_orders_ProductionAreaId",
                table: "work_orders",
                column: "ProductionAreaId");

            migrationBuilder.CreateIndex(
                name: "IX_work_orders_ProductionLineId",
                table: "work_orders",
                column: "ProductionLineId");

            migrationBuilder.CreateIndex(
                name: "IX_work_orders_RecipeId",
                table: "work_orders",
                column: "RecipeId");

            migrationBuilder.CreateIndex(
                name: "IX_work_order_material_requirements_MaterialId",
                table: "work_order_material_requirements",
                column: "MaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_work_order_material_requirements_WorkOrderId",
                table: "work_order_material_requirements",
                column: "WorkOrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_work_orders_ProductionAreas_ProductionAreaId",
                table: "work_orders",
                column: "ProductionAreaId",
                principalTable: "ProductionAreas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_work_orders_ProductionLines_ProductionLineId",
                table: "work_orders",
                column: "ProductionLineId",
                principalTable: "ProductionLines",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_work_orders_Recipes_RecipeId",
                table: "work_orders",
                column: "RecipeId",
                principalTable: "Recipes",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_work_orders_ProductionAreas_ProductionAreaId",
                table: "work_orders");

            migrationBuilder.DropForeignKey(
                name: "FK_work_orders_ProductionLines_ProductionLineId",
                table: "work_orders");

            migrationBuilder.DropForeignKey(
                name: "FK_work_orders_Recipes_RecipeId",
                table: "work_orders");

            migrationBuilder.DropTable(
                name: "work_order_material_requirements");

            migrationBuilder.DropIndex(
                name: "IX_work_orders_OrderNumber",
                table: "work_orders");

            migrationBuilder.DropIndex(
                name: "IX_work_orders_ProductionAreaId",
                table: "work_orders");

            migrationBuilder.DropIndex(
                name: "IX_work_orders_ProductionLineId",
                table: "work_orders");

            migrationBuilder.DropIndex(
                name: "IX_work_orders_RecipeId",
                table: "work_orders");

            migrationBuilder.DropColumn(
                name: "ActualQuantityDecimal",
                table: "work_orders");

            migrationBuilder.DropColumn(
                name: "DueDate",
                table: "work_orders");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "work_orders");

            migrationBuilder.DropColumn(
                name: "OrderStatus",
                table: "work_orders");

            migrationBuilder.DropColumn(
                name: "OutputUnit",
                table: "work_orders");

            migrationBuilder.DropColumn(
                name: "PlannedDate",
                table: "work_orders");

            migrationBuilder.DropColumn(
                name: "PlannedQuantity",
                table: "work_orders");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "work_orders");

            migrationBuilder.DropColumn(
                name: "ProductionAreaId",
                table: "work_orders");

            migrationBuilder.DropColumn(
                name: "ProductionLineId",
                table: "work_orders");

            migrationBuilder.DropColumn(
                name: "RecipeId",
                table: "work_orders");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "work_orders",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "OrderNumber",
                table: "work_orders",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<int>(
                name: "MachineId",
                table: "work_orders",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
