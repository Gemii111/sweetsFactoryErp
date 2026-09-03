using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FactoryX.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Phase10_QualityControl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "QualityInspections",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "RejectionReason",
                table: "QualityInspections",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "ApprovalNotes",
                table: "QualityInspections",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedAt",
                table: "QualityInspections",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CompletedByUserId",
                table: "QualityInspections",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedByUserId",
                table: "QualityInspections",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DecisionAt",
                table: "QualityInspections",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DecisionByUserId",
                table: "QualityInspections",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FinalDecision",
                table: "QualityInspections",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "HoldReason",
                table: "QualityInspections",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InspectionNumber",
                table: "QualityInspections",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "QualityInspections",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PreviousInspectionId",
                table: "QualityInspections",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "QualityTemplateId",
                table: "QualityInspections",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RecommendedDecision",
                table: "QualityInspections",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ReinspectionReason",
                table: "QualityInspections",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SubmittedAt",
                table: "QualityInspections",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SubmittedByUserId",
                table: "QualityInspections",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WorkOrderId",
                table: "QualityInspections",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "QualityTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ProductCategoryId = table.Column<int>(type: "int", nullable: true),
                    ProductId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QualityTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QualityTemplates_ProductCategories_ProductCategoryId",
                        column: x => x.ProductCategoryId,
                        principalTable: "ProductCategories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QualityTemplates_products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "QualityTemplateItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QualityTemplateId = table.Column<int>(type: "int", nullable: false),
                    SpecificationName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    DataType = table.Column<int>(type: "int", nullable: false),
                    MinValue = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    MaxValue = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    TargetValue = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    AllowedTextValues = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Unit = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QualityTemplateItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QualityTemplateItems_QualityTemplates_QualityTemplateId",
                        column: x => x.QualityTemplateId,
                        principalTable: "QualityTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "QualityInspectionItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QualityInspectionId = table.Column<int>(type: "int", nullable: false),
                    QualityTemplateItemId = table.Column<int>(type: "int", nullable: true),
                    SpecificationName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    DataType = table.Column<int>(type: "int", nullable: false),
                    MinValue = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    MaxValue = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    TargetValue = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    AllowedTextValues = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Unit = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ActualTextValue = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ActualNumericValue = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    ActualBooleanValue = table.Column<bool>(type: "bit", nullable: true),
                    ActualPassFailValue = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Result = table.Column<int>(type: "int", nullable: false),
                    InspectorNotes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QualityInspectionItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QualityInspectionItems_QualityInspections_QualityInspectionId",
                        column: x => x.QualityInspectionId,
                        principalTable: "QualityInspections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QualityInspectionItems_QualityTemplateItems_QualityTemplateItemId",
                        column: x => x.QualityTemplateItemId,
                        principalTable: "QualityTemplateItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QualityInspections_CompletedByUserId",
                table: "QualityInspections",
                column: "CompletedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_QualityInspections_CreatedByUserId",
                table: "QualityInspections",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_QualityInspections_DecisionByUserId",
                table: "QualityInspections",
                column: "DecisionByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_QualityInspections_InspectionNumber",
                table: "QualityInspections",
                column: "InspectionNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QualityInspections_PreviousInspectionId",
                table: "QualityInspections",
                column: "PreviousInspectionId");

            migrationBuilder.CreateIndex(
                name: "IX_QualityInspections_QualityTemplateId",
                table: "QualityInspections",
                column: "QualityTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_QualityInspections_SubmittedByUserId",
                table: "QualityInspections",
                column: "SubmittedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_QualityInspections_WorkOrderId",
                table: "QualityInspections",
                column: "WorkOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_QualityInspectionItems_QualityInspectionId",
                table: "QualityInspectionItems",
                column: "QualityInspectionId");

            migrationBuilder.CreateIndex(
                name: "IX_QualityInspectionItems_QualityTemplateItemId",
                table: "QualityInspectionItems",
                column: "QualityTemplateItemId");

            migrationBuilder.CreateIndex(
                name: "IX_QualityTemplateItems_QualityTemplateId",
                table: "QualityTemplateItems",
                column: "QualityTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_QualityTemplates_Code",
                table: "QualityTemplates",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QualityTemplates_ProductCategoryId",
                table: "QualityTemplates",
                column: "ProductCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_QualityTemplates_ProductId",
                table: "QualityTemplates",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_QualityInspections_QualityInspections_PreviousInspectionId",
                table: "QualityInspections",
                column: "PreviousInspectionId",
                principalTable: "QualityInspections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_QualityInspections_QualityTemplates_QualityTemplateId",
                table: "QualityInspections",
                column: "QualityTemplateId",
                principalTable: "QualityTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_QualityInspections_users_CompletedByUserId",
                table: "QualityInspections",
                column: "CompletedByUserId",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_QualityInspections_users_CreatedByUserId",
                table: "QualityInspections",
                column: "CreatedByUserId",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_QualityInspections_users_DecisionByUserId",
                table: "QualityInspections",
                column: "DecisionByUserId",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_QualityInspections_users_SubmittedByUserId",
                table: "QualityInspections",
                column: "SubmittedByUserId",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_QualityInspections_work_orders_WorkOrderId",
                table: "QualityInspections",
                column: "WorkOrderId",
                principalTable: "work_orders",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QualityInspections_QualityInspections_PreviousInspectionId",
                table: "QualityInspections");

            migrationBuilder.DropForeignKey(
                name: "FK_QualityInspections_QualityTemplates_QualityTemplateId",
                table: "QualityInspections");

            migrationBuilder.DropForeignKey(
                name: "FK_QualityInspections_users_CompletedByUserId",
                table: "QualityInspections");

            migrationBuilder.DropForeignKey(
                name: "FK_QualityInspections_users_CreatedByUserId",
                table: "QualityInspections");

            migrationBuilder.DropForeignKey(
                name: "FK_QualityInspections_users_DecisionByUserId",
                table: "QualityInspections");

            migrationBuilder.DropForeignKey(
                name: "FK_QualityInspections_users_SubmittedByUserId",
                table: "QualityInspections");

            migrationBuilder.DropForeignKey(
                name: "FK_QualityInspections_work_orders_WorkOrderId",
                table: "QualityInspections");

            migrationBuilder.DropTable(
                name: "QualityInspectionItems");

            migrationBuilder.DropTable(
                name: "QualityTemplateItems");

            migrationBuilder.DropTable(
                name: "QualityTemplates");

            migrationBuilder.DropIndex(
                name: "IX_QualityInspections_CompletedByUserId",
                table: "QualityInspections");

            migrationBuilder.DropIndex(
                name: "IX_QualityInspections_CreatedByUserId",
                table: "QualityInspections");

            migrationBuilder.DropIndex(
                name: "IX_QualityInspections_DecisionByUserId",
                table: "QualityInspections");

            migrationBuilder.DropIndex(
                name: "IX_QualityInspections_InspectionNumber",
                table: "QualityInspections");

            migrationBuilder.DropIndex(
                name: "IX_QualityInspections_PreviousInspectionId",
                table: "QualityInspections");

            migrationBuilder.DropIndex(
                name: "IX_QualityInspections_QualityTemplateId",
                table: "QualityInspections");

            migrationBuilder.DropIndex(
                name: "IX_QualityInspections_SubmittedByUserId",
                table: "QualityInspections");

            migrationBuilder.DropIndex(
                name: "IX_QualityInspections_WorkOrderId",
                table: "QualityInspections");

            migrationBuilder.DropColumn(
                name: "ApprovalNotes",
                table: "QualityInspections");

            migrationBuilder.DropColumn(
                name: "CompletedAt",
                table: "QualityInspections");

            migrationBuilder.DropColumn(
                name: "CompletedByUserId",
                table: "QualityInspections");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "QualityInspections");

            migrationBuilder.DropColumn(
                name: "DecisionAt",
                table: "QualityInspections");

            migrationBuilder.DropColumn(
                name: "DecisionByUserId",
                table: "QualityInspections");

            migrationBuilder.DropColumn(
                name: "FinalDecision",
                table: "QualityInspections");

            migrationBuilder.DropColumn(
                name: "HoldReason",
                table: "QualityInspections");

            migrationBuilder.DropColumn(
                name: "InspectionNumber",
                table: "QualityInspections");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "QualityInspections");

            migrationBuilder.DropColumn(
                name: "PreviousInspectionId",
                table: "QualityInspections");

            migrationBuilder.DropColumn(
                name: "QualityTemplateId",
                table: "QualityInspections");

            migrationBuilder.DropColumn(
                name: "RecommendedDecision",
                table: "QualityInspections");

            migrationBuilder.DropColumn(
                name: "ReinspectionReason",
                table: "QualityInspections");

            migrationBuilder.DropColumn(
                name: "SubmittedAt",
                table: "QualityInspections");

            migrationBuilder.DropColumn(
                name: "SubmittedByUserId",
                table: "QualityInspections");

            migrationBuilder.DropColumn(
                name: "WorkOrderId",
                table: "QualityInspections");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "QualityInspections",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "RejectionReason",
                table: "QualityInspections",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000);
        }
    }
}
