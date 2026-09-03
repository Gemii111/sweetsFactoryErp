using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FactoryX.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Phase12_FinishedGoodsInventory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Unit",
                table: "FinishedGoodsStocks",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Quantity",
                table: "FinishedGoodsStocks",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AddColumn<string>(
                name: "BatchNumber",
                table: "FinishedGoodsStocks",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "PackagingOrderId",
                table: "FinishedGoodsStocks",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "QCInspectionId",
                table: "FinishedGoodsStocks",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalCost",
                table: "FinishedGoodsStocks",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "UnitCost",
                table: "FinishedGoodsStocks",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "FinishedGoodsReleases",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReleaseNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    ProductionBatchId = table.Column<int>(type: "int", nullable: false),
                    PackagingOrderId = table.Column<int>(type: "int", nullable: true),
                    QCInspectionId = table.Column<int>(type: "int", nullable: true),
                    WarehouseId = table.Column<int>(type: "int", nullable: false),
                    LocationId = table.Column<int>(type: "int", nullable: true),
                    BatchNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    UnitCost = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TotalCost = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    ProductionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReleasedByUserId = table.Column<int>(type: "int", nullable: false),
                    ReleasedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    InventoryTransactionId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinishedGoodsReleases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinishedGoodsReleases_InventoryTransactions_InventoryTransactionId",
                        column: x => x.InventoryTransactionId,
                        principalTable: "InventoryTransactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FinishedGoodsReleases_PackagingOrders_PackagingOrderId",
                        column: x => x.PackagingOrderId,
                        principalTable: "PackagingOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FinishedGoodsReleases_QualityInspections_QCInspectionId",
                        column: x => x.QCInspectionId,
                        principalTable: "QualityInspections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FinishedGoodsReleases_WarehouseLocations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "WarehouseLocations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FinishedGoodsReleases_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FinishedGoodsReleases_production_batches_ProductionBatchId",
                        column: x => x.ProductionBatchId,
                        principalTable: "production_batches",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FinishedGoodsReleases_products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FinishedGoodsReleases_users_ReleasedByUserId",
                        column: x => x.ReleasedByUserId,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FinishedGoodsStocks_BatchNumber",
                table: "FinishedGoodsStocks",
                column: "BatchNumber");

            migrationBuilder.CreateIndex(
                name: "IX_FinishedGoodsStocks_PackagingOrderId",
                table: "FinishedGoodsStocks",
                column: "PackagingOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_FinishedGoodsStocks_QCInspectionId",
                table: "FinishedGoodsStocks",
                column: "QCInspectionId");

            migrationBuilder.CreateIndex(
                name: "IX_FinishedGoodsReleases_BatchNumber",
                table: "FinishedGoodsReleases",
                column: "BatchNumber");

            migrationBuilder.CreateIndex(
                name: "IX_FinishedGoodsReleases_InventoryTransactionId",
                table: "FinishedGoodsReleases",
                column: "InventoryTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_FinishedGoodsReleases_LocationId",
                table: "FinishedGoodsReleases",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_FinishedGoodsReleases_PackagingOrderId",
                table: "FinishedGoodsReleases",
                column: "PackagingOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_FinishedGoodsReleases_ProductId",
                table: "FinishedGoodsReleases",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_FinishedGoodsReleases_ProductionBatchId",
                table: "FinishedGoodsReleases",
                column: "ProductionBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_FinishedGoodsReleases_QCInspectionId",
                table: "FinishedGoodsReleases",
                column: "QCInspectionId");

            migrationBuilder.CreateIndex(
                name: "IX_FinishedGoodsReleases_ReleasedByUserId",
                table: "FinishedGoodsReleases",
                column: "ReleasedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_FinishedGoodsReleases_ReleaseNumber",
                table: "FinishedGoodsReleases",
                column: "ReleaseNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FinishedGoodsReleases_WarehouseId",
                table: "FinishedGoodsReleases",
                column: "WarehouseId");

            migrationBuilder.AddForeignKey(
                name: "FK_FinishedGoodsStocks_PackagingOrders_PackagingOrderId",
                table: "FinishedGoodsStocks",
                column: "PackagingOrderId",
                principalTable: "PackagingOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FinishedGoodsStocks_QualityInspections_QCInspectionId",
                table: "FinishedGoodsStocks",
                column: "QCInspectionId",
                principalTable: "QualityInspections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FinishedGoodsStocks_PackagingOrders_PackagingOrderId",
                table: "FinishedGoodsStocks");

            migrationBuilder.DropForeignKey(
                name: "FK_FinishedGoodsStocks_QualityInspections_QCInspectionId",
                table: "FinishedGoodsStocks");

            migrationBuilder.DropTable(
                name: "FinishedGoodsReleases");

            migrationBuilder.DropIndex(
                name: "IX_FinishedGoodsStocks_BatchNumber",
                table: "FinishedGoodsStocks");

            migrationBuilder.DropIndex(
                name: "IX_FinishedGoodsStocks_PackagingOrderId",
                table: "FinishedGoodsStocks");

            migrationBuilder.DropIndex(
                name: "IX_FinishedGoodsStocks_QCInspectionId",
                table: "FinishedGoodsStocks");

            migrationBuilder.DropColumn(
                name: "BatchNumber",
                table: "FinishedGoodsStocks");

            migrationBuilder.DropColumn(
                name: "PackagingOrderId",
                table: "FinishedGoodsStocks");

            migrationBuilder.DropColumn(
                name: "QCInspectionId",
                table: "FinishedGoodsStocks");

            migrationBuilder.DropColumn(
                name: "TotalCost",
                table: "FinishedGoodsStocks");

            migrationBuilder.DropColumn(
                name: "UnitCost",
                table: "FinishedGoodsStocks");

            migrationBuilder.AlterColumn<string>(
                name: "Unit",
                table: "FinishedGoodsStocks",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<decimal>(
                name: "Quantity",
                table: "FinishedGoodsStocks",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldPrecision: 18,
                oldScale: 4);
        }
    }
}
