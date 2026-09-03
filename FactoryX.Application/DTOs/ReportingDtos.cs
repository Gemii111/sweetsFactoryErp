using FactoryX.Domain.Entities;

namespace FactoryX.Application.DTOs;

#region Global Filter DTO
public class ReportFilterDto
{
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int? WarehouseId { get; set; }
    public int? ProductId { get; set; }
    public int? ProductCategoryId { get; set; }
    public int? MaterialId { get; set; }
    public int? MaterialCategoryId { get; set; }
    public int? CustomerId { get; set; }
    public int? SupplierId { get; set; }
    public int? ProductionOrderId { get; set; }
    public int? ProductionBatchId { get; set; }
    public string? Status { get; set; }
    public string? SearchTerm { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}
#endregion

#region Executive Dashboard DTOs
public class ManagementDashboardDto
{
    public DateTime AsOfDate { get; set; } = DateTime.UtcNow;
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }

    // 1. Sales KPIs
    public decimal TotalSalesRevenue { get; set; }
    public int TotalSalesOrdersCount { get; set; }
    public int FulfilledSalesOrdersCount { get; set; }
    public int OutstandingSalesOrdersCount { get; set; }
    public decimal TotalInvoicedSales { get; set; }
    public decimal TotalCustomerReceivables { get; set; }

    // 2. Production KPIs
    public decimal TotalPlannedProductionKg { get; set; }
    public decimal TotalActualProductionKg { get; set; }
    public int TotalProductionOrdersCount { get; set; }
    public int ActiveBatchesCount { get; set; }
    public int CompletedBatchesCount { get; set; }
    public decimal ProductionAchievementRate => TotalPlannedProductionKg > 0 
        ? Math.Round((TotalActualProductionKg / TotalPlannedProductionKg) * 100, 1) 
        : 0;

    // 3. Inventory KPIs
    public decimal RawMaterialInventoryValue { get; set; }
    public decimal PackagingInventoryValue { get; set; }
    public decimal FinishedGoodsInventoryValue { get; set; }
    public decimal TotalInventoryValue => RawMaterialInventoryValue + PackagingInventoryValue + FinishedGoodsInventoryValue;
    public int LowStockItemsCount { get; set; }
    public int ExpiringLotsCount { get; set; }
    public int TotalInventoryTransactionsCount { get; set; }

    // 4. Waste KPIs
    public decimal TotalWasteQuantityKg { get; set; }
    public decimal TotalWasteCost { get; set; }
    public List<WasteByTypeSummaryDto> WasteByType { get; set; } = new();
    public List<WasteByReasonSummaryDto> WasteByReason { get; set; } = new();

    // 5. Quality KPIs
    public int TotalQCInspectionsCount { get; set; }
    public int QCApprovedCount { get; set; }
    public int QCRejectedCount { get; set; }
    public int QCOnHoldCount { get; set; }
    public int QCReinspectionCount { get; set; }
    public decimal QCPassRate => TotalQCInspectionsCount > 0 
        ? Math.Round(((decimal)QCApprovedCount / TotalQCInspectionsCount) * 100, 1) 
        : 0;

    // 6. Purchasing KPIs
    public int TotalPurchaseOrdersCount { get; set; }
    public int ReceivedPurchaseOrdersCount { get; set; }
    public int OutstandingPurchaseOrdersCount { get; set; }
    public decimal TotalSupplierPayables { get; set; }

    // 7. Finance & Accounting KPIs (Derived directly from Phase 16 Accounting)
    public decimal AccountingRevenue { get; set; }
    public decimal AccountingCOGS { get; set; }
    public decimal AccountingGrossProfit => AccountingRevenue - AccountingCOGS;
    public decimal AccountingOperatingExpenses { get; set; }
    public decimal AccountingNetProfit => AccountingGrossProfit - AccountingOperatingExpenses;
    public decimal TotalCashBalance { get; set; }
    public decimal TotalBankBalance { get; set; }
    public decimal TotalLiquidFunds => TotalCashBalance + TotalBankBalance;

    // 8. Trends for Visualizations
    public List<SalesTrendDto> MonthlySalesTrend { get; set; } = new();
    public List<ProductionTrendDto> MonthlyProductionTrend { get; set; } = new();
    public List<TopSellingProductDto> TopProducts { get; set; } = new();
}

public class WasteByTypeSummaryDto
{
    public string TypeName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal Cost { get; set; }
}

public class WasteByReasonSummaryDto
{
    public string ReasonDescription { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal Quantity { get; set; }
    public decimal Cost { get; set; }
}

public class SalesTrendDto
{
    public string PeriodName { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public decimal Invoiced { get; set; }
}

public class ProductionTrendDto
{
    public string PeriodName { get; set; } = string.Empty;
    public decimal PlannedQuantity { get; set; }
    public decimal ActualQuantity { get; set; }
}

public class TopSellingProductDto
{
    public string ProductName { get; set; } = string.Empty;
    public decimal QuantitySold { get; set; }
    public decimal Revenue { get; set; }
}
#endregion

#region Sales Reports DTOs
public class SalesSummaryReportDto
{
    public ReportFilterDto Filter { get; set; } = new();
    public int TotalOrders { get; set; }
    public decimal TotalOrderedQuantity { get; set; }
    public decimal TotalFulfilledQuantity { get; set; }
    public decimal TotalOrderValue { get; set; }
    public decimal TotalInvoicedAmount { get; set; }
    public decimal TotalPaidAmount { get; set; }
    public decimal TotalOutstandingReceivable { get; set; }
    public List<SalesSummaryItemDto> Items { get; set; } = new();
    public List<CustomerSalesItemDto> SalesByCustomer { get; set; } = new();
    public List<ProductSalesItemDto> SalesByProduct { get; set; } = new();
}

public class SalesSummaryItemDto
{
    public DateTime OrderDate { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public int OrderId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerCode { get; set; } = string.Empty;
    public decimal OrderedQuantity { get; set; }
    public decimal FulfilledQuantity { get; set; }
    public decimal OrderAmount { get; set; }
    public decimal InvoicedAmount { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class SalesOrderStatusReportDto
{
    public ReportFilterDto Filter { get; set; } = new();
    public int DraftCount { get; set; }
    public int ConfirmedCount { get; set; }
    public int PartiallyFulfilledCount { get; set; }
    public int FullyFulfilledCount { get; set; }
    public int ClosedCount { get; set; }
    public List<SalesOrderStatusItemDto> Orders { get; set; } = new();
}

public class SalesOrderStatusItemDto
{
    public int OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public DateTime? RequiredDeliveryDate { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public decimal TotalQuantity { get; set; }
    public decimal FulfilledQuantity { get; set; }
    public decimal RemainingQuantity => Math.Max(0, TotalQuantity - FulfilledQuantity);
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class CustomerSalesReportDto
{
    public ReportFilterDto Filter { get; set; } = new();
    public List<CustomerSalesItemDto> Customers { get; set; } = new();
}

public class CustomerSalesItemDto
{
    public int CustomerId { get; set; }
    public string CustomerCode { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerType { get; set; } = string.Empty;
    public int TotalOrders { get; set; }
    public decimal OrderedValue { get; set; }
    public decimal FulfilledValue { get; set; }
    public decimal InvoicedValue { get; set; }
    public decimal PaidValue { get; set; }
    public decimal OutstandingReceivable { get; set; }
}

public class ProductSalesReportDto
{
    public ReportFilterDto Filter { get; set; } = new();
    public List<ProductSalesItemDto> Products { get; set; } = new();
}

public class ProductSalesItemDto
{
    public int ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public decimal QuantitySold { get; set; }
    public string Unit { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public decimal CostOfGoodsSold { get; set; }
    public decimal GrossProfit => Revenue - CostOfGoodsSold;
    public decimal GrossMarginPercent => Revenue > 0 ? Math.Round((GrossProfit / Revenue) * 100, 1) : 0;
}
#endregion

#region Purchasing Reports DTOs
public class PurchaseSummaryReportDto
{
    public ReportFilterDto Filter { get; set; } = new();
    public int TotalRequestsCount { get; set; }
    public int TotalOrdersCount { get; set; }
    public int TotalReceiptsCount { get; set; }
    public decimal TotalOrderedQuantity { get; set; }
    public decimal TotalReceivedQuantity { get; set; }
    public decimal TotalRejectedQuantity { get; set; }
    public decimal TotalPurchaseValue { get; set; }
    public List<SupplierPurchaseItemDto> SupplierSummaries { get; set; } = new();
}

public class SupplierPurchaseReportDto
{
    public ReportFilterDto Filter { get; set; } = new();
    public List<SupplierPurchaseItemDto> Suppliers { get; set; } = new();
}

public class SupplierPurchaseItemDto
{
    public int SupplierId { get; set; }
    public string SupplierCode { get; set; } = string.Empty;
    public string SupplierName { get; set; } = string.Empty;
    public int OrdersCount { get; set; }
    public decimal OrderedValue { get; set; }
    public decimal ReceivedValue { get; set; }
    public decimal PaidValue { get; set; }
    public decimal OutstandingPayable { get; set; }
}

public class PurchaseOrderStatusReportDto
{
    public ReportFilterDto Filter { get; set; } = new();
    public List<PurchaseOrderItemStatusDto> Orders { get; set; } = new();
}

public class PurchaseOrderItemStatusDto
{
    public int OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public decimal TotalOrderedQuantity { get; set; }
    public decimal TotalReceivedQuantity { get; set; }
    public decimal RemainingQuantity => Math.Max(0, TotalOrderedQuantity - TotalReceivedQuantity);
    public decimal TotalCost { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class SupplierPriceHistoryReportDto
{
    public ReportFilterDto Filter { get; set; } = new();
    public List<SupplierPriceHistoryItemDto> PriceHistories { get; set; } = new();
}

public class SupplierPriceHistoryItemDto
{
    public int Id { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public string MaterialName { get; set; } = string.Empty;
    public decimal PreviousPrice { get; set; }
    public decimal CurrentPrice { get; set; }
    public decimal PriceChangePercent => PreviousPrice > 0 
        ? Math.Round(((CurrentPrice - PreviousPrice) / PreviousPrice) * 100, 1) 
        : 0;
    public DateTime EffectiveDate { get; set; }
    public string? Notes { get; set; }
}
#endregion

#region Inventory Reports DTOs
public class InventoryValuationReportDto
{
    public ReportFilterDto Filter { get; set; } = new();
    public decimal TotalRawMaterialValuation { get; set; }
    public decimal TotalPackagingValuation { get; set; }
    public decimal TotalFinishedGoodsValuation { get; set; }
    public decimal TotalInventoryValuation => TotalRawMaterialValuation + TotalPackagingValuation + TotalFinishedGoodsValuation;
    public List<InventoryValuationItemDto> Items { get; set; } = new();
}

public class InventoryValuationItemDto
{
    public string ItemType { get; set; } = string.Empty; // Raw, Packaging, Finished
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string WarehouseName { get; set; } = string.Empty;
    public string? BatchNumber { get; set; }
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public decimal UnitCost { get; set; }
    public decimal TotalValue => Math.Round(Quantity * UnitCost, 2);
}

public class StockBalanceReportDto
{
    public ReportFilterDto Filter { get; set; } = new();
    public List<StockBalanceItemDto> Stocks { get; set; } = new();
}

public class StockBalanceItemDto
{
    public string WarehouseName { get; set; } = string.Empty;
    public string? LocationCode { get; set; }
    public string ItemType { get; set; } = string.Empty;
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string? BatchNumber { get; set; }
    public decimal Quantity { get; set; }
    public decimal ReservedQuantity { get; set; }
    public decimal AvailableQuantity => Math.Max(0, Quantity - ReservedQuantity);
    public string Unit { get; set; } = string.Empty;
    public decimal MinimumStock { get; set; }
    public decimal MaximumStock { get; set; }
    public string StockStatus => Quantity <= 0 ? "OUT_OF_STOCK"
        : (MinimumStock > 0 && Quantity < MinimumStock) ? "LOW_STOCK"
        : (MaximumStock > 0 && Quantity > MaximumStock) ? "OVER_STOCK"
        : "NORMAL";
}

public class InventoryMovementReportDto
{
    public ReportFilterDto Filter { get; set; } = new();
    public List<InventoryMovementItemDto> Movements { get; set; } = new();
}

public class InventoryMovementItemDto
{
    public DateTime Date { get; set; }
    public string TransactionNumber { get; set; } = string.Empty;
    public string TransactionType { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string WarehouseName { get; set; } = string.Empty;
    public string? BatchNumber { get; set; }
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public string Direction { get; set; } = string.Empty; // IN / OUT
    public string? ReferenceDocument { get; set; }
    public string? Notes { get; set; }
}

public class LowStockReportDto
{
    public ReportFilterDto Filter { get; set; } = new();
    public List<LowStockItemDto> Items { get; set; } = new();
}

public class LowStockItemDto
{
    public string ItemType { get; set; } = string.Empty;
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string WarehouseName { get; set; } = string.Empty;
    public decimal CurrentQuantity { get; set; }
    public decimal MinimumQuantity { get; set; }
    public decimal Deficit => Math.Max(0, MinimumQuantity - CurrentQuantity);
    public string Unit { get; set; } = string.Empty;
}

public class ExpiryReportDto
{
    public ReportFilterDto Filter { get; set; } = new();
    public List<ExpiryItemDto> Lots { get; set; } = new();
}

public class ExpiryItemDto
{
    public string ItemType { get; set; } = string.Empty;
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string BatchNumber { get; set; } = string.Empty;
    public string WarehouseName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public DateTime? ProductionDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public int DaysRemaining => (ExpiryDate.Date - DateTime.UtcNow.Date).Days;
    public bool IsExpired => DaysRemaining < 0;
}
#endregion

#region Production Reports DTOs
public class ProductionSummaryReportDto
{
    public ReportFilterDto Filter { get; set; } = new();
    public int TotalProductionOrders { get; set; }
    public decimal TotalPlannedOutputKg { get; set; }
    public decimal TotalActualOutputKg { get; set; }
    public decimal AchievementRate => TotalPlannedOutputKg > 0 
        ? Math.Round((TotalActualOutputKg / TotalPlannedOutputKg) * 100, 1) 
        : 0;
    public int CompletedBatchesCount { get; set; }
    public int ActiveBatchesCount { get; set; }
    public int CancelledBatchesCount { get; set; }
    public List<ProductionOrderPerformanceItemDto> Orders { get; set; } = new();
}

public class ProductionOrderPerformanceReportDto
{
    public ReportFilterDto Filter { get; set; } = new();
    public List<ProductionOrderPerformanceItemDto> Orders { get; set; } = new();
}

public class ProductionOrderPerformanceItemDto
{
    public int OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal PlannedQuantity { get; set; }
    public decimal ActualQuantity { get; set; }
    public decimal Variance => ActualQuantity - PlannedQuantity;
    public decimal AchievementPercent => PlannedQuantity > 0 
        ? Math.Round((ActualQuantity / PlannedQuantity) * 100, 1) 
        : 0;
    public DateTime PlannedDate { get; set; }
    public DateTime? CompletionDate { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class ProductionBatchReportDto
{
    public ReportFilterDto Filter { get; set; } = new();
    public List<ProductionBatchItemDto> Batches { get; set; } = new();
}

public class ProductionBatchItemDto
{
    public int BatchId { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public string WorkOrderNumber { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal PlannedQuantity { get; set; }
    public decimal ActualQuantity { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class MaterialConsumptionVarianceReportDto
{
    public ReportFilterDto Filter { get; set; } = new();
    public List<MaterialConsumptionVarianceItemDto> Variances { get; set; } = new();
}

public class MaterialConsumptionVarianceItemDto
{
    public string WorkOrderNumber { get; set; } = string.Empty;
    public string BatchNumber { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string MaterialName { get; set; } = string.Empty;
    public decimal PlannedRequirementQuantity { get; set; }
    public decimal ActualConsumedQuantity { get; set; }
    public decimal VarianceQuantity => ActualConsumedQuantity - PlannedRequirementQuantity;
    public decimal VariancePercent => PlannedRequirementQuantity > 0 
        ? Math.Round(((ActualConsumedQuantity - PlannedRequirementQuantity) / PlannedRequirementQuantity) * 100, 1) 
        : 0;
    public string Unit { get; set; } = string.Empty;
}

public class ProductionCostSummaryReportDto
{
    public ReportFilterDto Filter { get; set; } = new();
    public List<ProductionCostSummaryItemDto> CostSummaries { get; set; } = new();
}

public class ProductionCostSummaryItemDto
{
    public string BatchNumber { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal OutputQuantity { get; set; }
    public decimal MaterialCost { get; set; }
    public decimal LaborCost { get; set; }
    public decimal MachineCost { get; set; }
    public decimal OverheadCost { get; set; }
    public decimal WasteCost { get; set; }
    public decimal TotalCost => MaterialCost + LaborCost + MachineCost + OverheadCost + WasteCost;
    public decimal CostPerUnit => OutputQuantity > 0 ? Math.Round(TotalCost / OutputQuantity, 2) : 0;
}
#endregion

#region Waste Reports DTOs
public class WasteSummaryReportDto
{
    public ReportFilterDto Filter { get; set; } = new();
    public decimal TotalWasteQuantity { get; set; }
    public decimal TotalWasteCost { get; set; }
    public List<WasteByTypeSummaryDto> WasteByType { get; set; } = new();
    public List<WasteByReasonSummaryDto> WasteByReason { get; set; } = new();
    public List<WasteRecordReportItemDto> WasteRecords { get; set; } = new();
}

public class WasteTypeReportDto
{
    public ReportFilterDto Filter { get; set; } = new();
    public List<WasteByTypeSummaryDto> WasteTypes { get; set; } = new();
}

public class WasteReasonReportDto
{
    public ReportFilterDto Filter { get; set; } = new();
    public List<WasteByReasonSummaryDto> WasteReasons { get; set; } = new();
}

public class WasteRecordReportItemDto
{
    public string WasteNumber { get; set; } = string.Empty;
    public DateTime WasteDate { get; set; }
    public string WasteType { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public decimal TotalCost { get; set; }
    public string ReasonDescription { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}
#endregion

#region Quality Reports DTOs
public class QualitySummaryReportDto
{
    public ReportFilterDto Filter { get; set; } = new();
    public int TotalInspectionsCount { get; set; }
    public int ApprovedCount { get; set; }
    public int RejectedCount { get; set; }
    public int HoldCount { get; set; }
    public int CancelledCount { get; set; }
    public int ReinspectionCount { get; set; }
    public decimal PassRate => TotalInspectionsCount > 0 
        ? Math.Round(((decimal)ApprovedCount / TotalInspectionsCount) * 100, 1) 
        : 0;
    public List<ProductQualityItemDto> ProductQualitySummaries { get; set; } = new();
}

public class ProductQualityReportDto
{
    public ReportFilterDto Filter { get; set; } = new();
    public List<ProductQualityItemDto> Products { get; set; } = new();
}

public class ProductQualityItemDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int TotalInspections { get; set; }
    public int ApprovedCount { get; set; }
    public int RejectedCount { get; set; }
    public int HoldCount { get; set; }
    public decimal ApprovedPercent => TotalInspections > 0 ? Math.Round(((decimal)ApprovedCount / TotalInspections) * 100, 1) : 0;
    public decimal RejectedPercent => TotalInspections > 0 ? Math.Round(((decimal)RejectedCount / TotalInspections) * 100, 1) : 0;
}

public class QualityRejectionReportDto
{
    public ReportFilterDto Filter { get; set; } = new();
    public List<QualityRejectionItemDto> Rejections { get; set; } = new();
}

public class QualityRejectionItemDto
{
    public string InspectionNumber { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string? BatchNumber { get; set; }
    public DateTime InspectionDate { get; set; }
    public string Decision { get; set; } = string.Empty;
    public string? FailedParameters { get; set; }
    public string? Comments { get; set; }
}
#endregion

#region Packaging Reports DTOs
public class PackagingSummaryReportDto
{
    public ReportFilterDto Filter { get; set; } = new();
    public int TotalPackagingOrders { get; set; }
    public int CompletedPackagingOrders { get; set; }
    public decimal TotalPlannedQuantity { get; set; }
    public decimal TotalCompletedQuantity { get; set; }
    public decimal TotalPackagingCost { get; set; }
    public List<PackagingConsumptionItemDto> Consumptions { get; set; } = new();
}

public class PackagingConsumptionReportDto
{
    public ReportFilterDto Filter { get; set; } = new();
    public List<PackagingConsumptionItemDto> Items { get; set; } = new();
}

public class PackagingConsumptionItemDto
{
    public string PackagingOrderNumber { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string PackagingMaterialName { get; set; } = string.Empty;
    public decimal PlannedQuantity { get; set; }
    public decimal ActualQuantity { get; set; }
    public decimal Variance => ActualQuantity - PlannedQuantity;
    public string Unit { get; set; } = string.Empty;
}
#endregion

#region Finished Goods Reports DTOs
public class FinishedGoodsStockReportDto
{
    public ReportFilterDto Filter { get; set; } = new();
    public decimal TotalQuantity { get; set; }
    public decimal TotalValuation { get; set; }
    public List<FinishedGoodsStockItemDto> Items { get; set; } = new();
}

public class FinishedGoodsStockItemDto
{
    public string ProductName { get; set; } = string.Empty;
    public string ProductCode { get; set; } = string.Empty;
    public string WarehouseName { get; set; } = string.Empty;
    public string BatchNumber { get; set; } = string.Empty;
    public DateTime ExpiryDate { get; set; }
    public decimal CurrentQuantity { get; set; }
    public decimal ReservedQuantity { get; set; }
    public decimal AvailableQuantity => Math.Max(0, CurrentQuantity - ReservedQuantity);
    public string Unit { get; set; } = string.Empty;
    public decimal UnitCost { get; set; }
    public decimal TotalValue => Math.Round(CurrentQuantity * UnitCost, 2);
}

public class FinishedGoodsReleaseReportDto
{
    public ReportFilterDto Filter { get; set; } = new();
    public List<FinishedGoodsReleaseItemDto> Releases { get; set; } = new();
}

public class FinishedGoodsReleaseItemDto
{
    public string ReleaseNumber { get; set; } = string.Empty;
    public DateTime ReleasedAt { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string BatchNumber { get; set; } = string.Empty;
    public decimal ReleasedQuantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public decimal TotalCost { get; set; }
    public string? QCInspectionNumber { get; set; }
    public string? PackagingOrderNumber { get; set; }
}

public class FinishedGoodsTraceabilityReportDto
{
    public string QuerySearch { get; set; } = string.Empty;
    public bool Found { get; set; }
    public FinishedGoodsTraceabilityTreeDto? TraceTree { get; set; }
}

public class FinishedGoodsTraceabilityTreeDto
{
    public string BatchNumber { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string ProductCode { get; set; } = string.Empty;
    public DateTime? ProductionDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? ProductionOrderNumber { get; set; }
    public string? RecipeCode { get; set; }
    public string? QCInspectionNumber { get; set; }
    public string? QCDecision { get; set; }
    public string? PackagingOrderNumber { get; set; }
    public string? ReleaseNumber { get; set; }
    public DateTime? ReleasedAt { get; set; }
    
    // Upstream Raw Materials
    public List<TraceabilityRawMaterialItemDto> RawMaterials { get; set; } = new();
    // Downstream Sales Fulfillments & Customers
    public List<TraceabilitySalesItemDto> SalesDeliveries { get; set; } = new();
}

public class TraceabilityRawMaterialItemDto
{
    public string MaterialName { get; set; } = string.Empty;
    public string? RawMaterialBatchNumber { get; set; }
    public decimal ConsumedQuantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public string? PurchaseReceiptNumber { get; set; }
    public string? SupplierName { get; set; }
}

public class TraceabilitySalesItemDto
{
    public string SalesOrderNumber { get; set; } = string.Empty;
    public string FulfillmentNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public DateTime ShippedDate { get; set; }
    public decimal ShippedQuantity { get; set; }
    public string Unit { get; set; } = string.Empty;
}
#endregion

#region Accounting Reports DTOs (Phase 16 Source-of-Truth)
public class ProfitAndLossReportDto
{
    public ReportFilterDto Filter { get; set; } = new();
    
    // Revenue
    public decimal SalesRevenue { get; set; }
    public decimal OtherRevenue { get; set; }
    public decimal TotalRevenue => SalesRevenue + OtherRevenue;

    // Cost of Goods Sold
    public decimal CostOfGoodsSold { get; set; }
    
    // Gross Profit
    public decimal GrossProfit => TotalRevenue - CostOfGoodsSold;
    public decimal GrossProfitMarginPercent => TotalRevenue > 0 ? Math.Round((GrossProfit / TotalRevenue) * 100, 1) : 0;

    // Operating Expenses
    public decimal WasteExpense { get; set; }
    public decimal GeneralAndAdminExpenses { get; set; }
    public decimal TotalOperatingExpenses => WasteExpense + GeneralAndAdminExpenses;

    // Net Profit
    public decimal NetOperatingProfit => GrossProfit - TotalOperatingExpenses;
    public decimal NetProfitMarginPercent => TotalRevenue > 0 ? Math.Round((NetOperatingProfit / TotalRevenue) * 100, 1) : 0;

    public List<ProfitAndLossDetailLineDto> RevenueLines { get; set; } = new();
    public List<ProfitAndLossDetailLineDto> ExpenseLines { get; set; } = new();
}

public class ProfitAndLossDetailLineDto
{
    public string AccountCode { get; set; } = string.Empty;
    public string AccountNameAr { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class BalanceSheetReportDto
{
    public ReportFilterDto Filter { get; set; } = new();
    public DateTime AsOfDate { get; set; }

    // Assets
    public decimal CashAndBankBalances { get; set; }
    public decimal AccountsReceivable { get; set; }
    public decimal RawMaterialInventory { get; set; }
    public decimal PackagingInventory { get; set; }
    public decimal FinishedGoodsInventory { get; set; }
    public decimal TotalInventory => RawMaterialInventory + PackagingInventory + FinishedGoodsInventory;
    public decimal TotalCurrentAssets => CashAndBankBalances + AccountsReceivable + TotalInventory;
    public decimal TotalAssets => TotalCurrentAssets;

    // Liabilities
    public decimal AccountsPayable { get; set; }
    public decimal OutputVatLiability { get; set; }
    public decimal TotalCurrentLiabilities => AccountsPayable + OutputVatLiability;
    public decimal TotalLiabilities => TotalCurrentLiabilities;

    // Equity
    public decimal PaidInCapital { get; set; }
    public decimal RetainedEarnings { get; set; }
    public decimal CurrentYearNetIncome { get; set; }
    public decimal TotalEquity => PaidInCapital + RetainedEarnings + CurrentYearNetIncome;

    public decimal TotalLiabilitiesAndEquity => TotalLiabilities + TotalEquity;
    public bool IsBalanced => Math.Abs(TotalAssets - TotalLiabilitiesAndEquity) < 1.00m;

    public List<BalanceSheetAccountItemDto> AssetAccounts { get; set; } = new();
    public List<BalanceSheetAccountItemDto> LiabilityAccounts { get; set; } = new();
    public List<BalanceSheetAccountItemDto> EquityAccounts { get; set; } = new();
}

public class BalanceSheetAccountItemDto
{
    public string AccountCode { get; set; } = string.Empty;
    public string AccountNameAr { get; set; } = string.Empty;
    public decimal Balance { get; set; }
}

public class CustomerReceivablesReportDto
{
    public ReportFilterDto Filter { get; set; } = new();
    public decimal AccountingControlBalance { get; set; }
    public decimal SubledgerTotalReceivable { get; set; }
    public decimal ReconciliationDifference => SubledgerTotalReceivable - AccountingControlBalance;
    public List<CustomerReceivableItemDto> Customers { get; set; } = new();
}

public class CustomerReceivableItemDto
{
    public int CustomerId { get; set; }
    public string CustomerCode { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public decimal InvoicedAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal OutstandingReceivable { get; set; }
    public decimal CurrentBalanceInDb { get; set; }
}

public class SupplierPayablesReportDto
{
    public ReportFilterDto Filter { get; set; } = new();
    public decimal AccountingControlBalance { get; set; }
    public decimal SubledgerTotalPayable { get; set; }
    public decimal ReconciliationDifference => SubledgerTotalPayable - AccountingControlBalance;
    public List<SupplierPayableItemDto> Suppliers { get; set; } = new();
}

public class SupplierPayableItemDto
{
    public int SupplierId { get; set; }
    public string SupplierCode { get; set; } = string.Empty;
    public string SupplierName { get; set; } = string.Empty;
    public decimal TotalPurchases { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal OutstandingPayable { get; set; }
}

public class VatReportDto
{
    public ReportFilterDto Filter { get; set; } = new();
    public decimal OutputVatTotal { get; set; }
    public decimal InputVatTotal { get; set; }
    public decimal NetVatPayable => OutputVatTotal - InputVatTotal;
    public List<VatTransactionItemDto> Transactions { get; set; } = new();
}

public class VatTransactionItemDto
{
    public DateTime Date { get; set; }
    public string JournalNumber { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // Output VAT / Input VAT
    public string DocumentNumber { get; set; } = string.Empty;
    public string PartnerName { get; set; } = string.Empty;
    public decimal TaxableAmount { get; set; }
    public decimal VatAmount { get; set; }
}
#endregion

#region Management Profitability DTOs
public class ManagementProfitabilityReportDto
{
    public ReportFilterDto Filter { get; set; } = new();
    public decimal TotalRevenue { get; set; }
    public decimal TotalCOGS { get; set; }
    public decimal TotalGrossProfit => TotalRevenue - TotalCOGS;
    public decimal OverallGrossMarginPercent => TotalRevenue > 0 ? Math.Round((TotalGrossProfit / TotalRevenue) * 100, 1) : 0;
    public List<ProductProfitabilityItemDto> ProductProfitability { get; set; } = new();
    public List<CustomerProfitabilityItemDto> CustomerProfitability { get; set; } = new();
}

public class ProductProfitabilityItemDto
{
    public int ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public decimal QuantitySold { get; set; }
    public decimal Revenue { get; set; }
    public decimal CostOfGoodsSold { get; set; }
    public decimal GrossProfit => Revenue - CostOfGoodsSold;
    public decimal GrossMarginPercent => Revenue > 0 ? Math.Round((GrossProfit / Revenue) * 100, 1) : 0;
}

public class CustomerProfitabilityItemDto
{
    public int CustomerId { get; set; }
    public string CustomerCode { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public decimal CostOfGoodsSold { get; set; }
    public decimal GrossProfit => Revenue - CostOfGoodsSold;
    public decimal GrossMarginPercent => Revenue > 0 ? Math.Round((GrossProfit / Revenue) * 100, 1) : 0;
}
#endregion
