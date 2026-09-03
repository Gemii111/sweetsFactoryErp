using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FactoryX.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Phase11_PackagingManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Unit",
                table: "PackagingItems",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<decimal>(
                name: "QuantityRequired",
                table: "PackagingItems",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<int>(
                name: "PackagingBOMId",
                table: "PackagingItems",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<bool>(
                name: "IsOptional",
                table: "PackagingItems",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "PackagingItems",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PackagingBOMVersionId",
                table: "PackagingItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Sequence",
                table: "PackagingItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Unit",
                table: "PackagingBOMs",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<decimal>(
                name: "OutputProductQuantity",
                table: "PackagingBOMs",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "PackagingBOMs",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "PackagingBOMs",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "PackagingBOMs",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "PackSize",
                table: "PackagingBOMs",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PackSizeKg",
                table: "PackagingBOMs",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "PackUnit",
                table: "PackagingBOMs",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsPackagingMaterial",
                table: "materials",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "PackagingType",
                table: "materials",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CategoryType",
                table: "MaterialCategories",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "PackagingBOMVersions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PackagingBOMId = table.Column<int>(type: "int", nullable: false),
                    VersionNumber = table.Column<int>(type: "int", nullable: false),
                    VersionName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EffectiveFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EffectiveTo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackagingBOMVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PackagingBOMVersions_PackagingBOMs_PackagingBOMId",
                        column: x => x.PackagingBOMId,
                        principalTable: "PackagingBOMs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PackagingOrders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ProductionBatchId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    PackagingBOMId = table.Column<int>(type: "int", nullable: false),
                    PackagingBOMVersionId = table.Column<int>(type: "int", nullable: true),
                    PlannedQuantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    ActualQuantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TheoreticalMaxPacks = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OperatorId = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CancellationReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PackagingMaterialCost = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: true),
                    CompletedByUserId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackagingOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PackagingOrders_PackagingBOMVersions_PackagingBOMVersionId",
                        column: x => x.PackagingBOMVersionId,
                        principalTable: "PackagingBOMVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PackagingOrders_PackagingBOMs_PackagingBOMId",
                        column: x => x.PackagingBOMId,
                        principalTable: "PackagingBOMs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PackagingOrders_operators_OperatorId",
                        column: x => x.OperatorId,
                        principalTable: "operators",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PackagingOrders_production_batches_ProductionBatchId",
                        column: x => x.ProductionBatchId,
                        principalTable: "production_batches",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PackagingOrders_products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PackagingOrders_users_CompletedByUserId",
                        column: x => x.CompletedByUserId,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PackagingOrders_users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PackagingConsumptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PackagingOrderId = table.Column<int>(type: "int", nullable: false),
                    MaterialId = table.Column<int>(type: "int", nullable: false),
                    PlannedQuantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    ActualQuantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    WarehouseId = table.Column<int>(type: "int", nullable: false),
                    LocationId = table.Column<int>(type: "int", nullable: true),
                    BatchNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UnitCost = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TotalCost = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    InventoryTransactionId = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackagingConsumptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PackagingConsumptions_InventoryTransactions_InventoryTransactionId",
                        column: x => x.InventoryTransactionId,
                        principalTable: "InventoryTransactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PackagingConsumptions_PackagingOrders_PackagingOrderId",
                        column: x => x.PackagingOrderId,
                        principalTable: "PackagingOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PackagingConsumptions_WarehouseLocations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "WarehouseLocations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PackagingConsumptions_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PackagingConsumptions_materials_MaterialId",
                        column: x => x.MaterialId,
                        principalTable: "materials",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PackagingItems_PackagingBOMVersionId",
                table: "PackagingItems",
                column: "PackagingBOMVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_PackagingBOMs_Code",
                table: "PackagingBOMs",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PackagingBOMVersions_PackagingBOMId",
                table: "PackagingBOMVersions",
                column: "PackagingBOMId");

            migrationBuilder.CreateIndex(
                name: "IX_PackagingConsumptions_InventoryTransactionId",
                table: "PackagingConsumptions",
                column: "InventoryTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_PackagingConsumptions_LocationId",
                table: "PackagingConsumptions",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_PackagingConsumptions_MaterialId",
                table: "PackagingConsumptions",
                column: "MaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_PackagingConsumptions_PackagingOrderId",
                table: "PackagingConsumptions",
                column: "PackagingOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_PackagingConsumptions_WarehouseId",
                table: "PackagingConsumptions",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_PackagingOrders_CompletedByUserId",
                table: "PackagingOrders",
                column: "CompletedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PackagingOrders_CreatedByUserId",
                table: "PackagingOrders",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PackagingOrders_OperatorId",
                table: "PackagingOrders",
                column: "OperatorId");

            migrationBuilder.CreateIndex(
                name: "IX_PackagingOrders_OrderNumber",
                table: "PackagingOrders",
                column: "OrderNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PackagingOrders_PackagingBOMId",
                table: "PackagingOrders",
                column: "PackagingBOMId");

            migrationBuilder.CreateIndex(
                name: "IX_PackagingOrders_PackagingBOMVersionId",
                table: "PackagingOrders",
                column: "PackagingBOMVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_PackagingOrders_ProductId",
                table: "PackagingOrders",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_PackagingOrders_ProductionBatchId",
                table: "PackagingOrders",
                column: "ProductionBatchId");

            migrationBuilder.AddForeignKey(
                name: "FK_PackagingItems_PackagingBOMVersions_PackagingBOMVersionId",
                table: "PackagingItems",
                column: "PackagingBOMVersionId",
                principalTable: "PackagingBOMVersions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PackagingItems_PackagingBOMVersions_PackagingBOMVersionId",
                table: "PackagingItems");

            migrationBuilder.DropTable(
                name: "PackagingConsumptions");

            migrationBuilder.DropTable(
                name: "PackagingOrders");

            migrationBuilder.DropTable(
                name: "PackagingBOMVersions");

            migrationBuilder.DropIndex(
                name: "IX_PackagingItems_PackagingBOMVersionId",
                table: "PackagingItems");

            migrationBuilder.DropIndex(
                name: "IX_PackagingBOMs_Code",
                table: "PackagingBOMs");

            migrationBuilder.DropColumn(
                name: "IsOptional",
                table: "PackagingItems");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "PackagingItems");

            migrationBuilder.DropColumn(
                name: "PackagingBOMVersionId",
                table: "PackagingItems");

            migrationBuilder.DropColumn(
                name: "Sequence",
                table: "PackagingItems");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "PackagingBOMs");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "PackagingBOMs");

            migrationBuilder.DropColumn(
                name: "PackSize",
                table: "PackagingBOMs");

            migrationBuilder.DropColumn(
                name: "PackSizeKg",
                table: "PackagingBOMs");

            migrationBuilder.DropColumn(
                name: "PackUnit",
                table: "PackagingBOMs");

            migrationBuilder.DropColumn(
                name: "IsPackagingMaterial",
                table: "materials");

            migrationBuilder.DropColumn(
                name: "PackagingType",
                table: "materials");

            migrationBuilder.DropColumn(
                name: "CategoryType",
                table: "MaterialCategories");

            migrationBuilder.AlterColumn<string>(
                name: "Unit",
                table: "PackagingItems",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<decimal>(
                name: "QuantityRequired",
                table: "PackagingItems",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldPrecision: 18,
                oldScale: 4);

            migrationBuilder.AlterColumn<int>(
                name: "PackagingBOMId",
                table: "PackagingItems",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Unit",
                table: "PackagingBOMs",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<decimal>(
                name: "OutputProductQuantity",
                table: "PackagingBOMs",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldPrecision: 18,
                oldScale: 4);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "PackagingBOMs",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);
        }
    }
}
