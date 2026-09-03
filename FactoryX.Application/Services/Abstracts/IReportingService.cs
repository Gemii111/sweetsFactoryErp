using FactoryX.Application.DTOs;

namespace FactoryX.Application.Services.Abstracts;

public interface IReportingService
{
    // Executive / Management Dashboard
    Task<ManagementDashboardDto> GetManagementDashboardAsync(ReportFilterDto filter);

    // 1. Sales Reports
    Task<SalesSummaryReportDto> GetSalesSummaryReportAsync(ReportFilterDto filter);
    Task<SalesOrderStatusReportDto> GetSalesOrderStatusReportAsync(ReportFilterDto filter);
    Task<CustomerSalesReportDto> GetCustomerSalesReportAsync(ReportFilterDto filter);
    Task<ProductSalesReportDto> GetProductSalesReportAsync(ReportFilterDto filter);

    // 2. Purchasing Reports
    Task<PurchaseSummaryReportDto> GetPurchaseSummaryReportAsync(ReportFilterDto filter);
    Task<SupplierPurchaseReportDto> GetSupplierPurchaseReportAsync(ReportFilterDto filter);
    Task<PurchaseOrderStatusReportDto> GetPurchaseOrderStatusReportAsync(ReportFilterDto filter);
    Task<SupplierPriceHistoryReportDto> GetSupplierPriceHistoryReportAsync(ReportFilterDto filter);

    // 3. Inventory Reports
    Task<InventoryValuationReportDto> GetInventoryValuationReportAsync(ReportFilterDto filter);
    Task<StockBalanceReportDto> GetStockBalanceReportAsync(ReportFilterDto filter);
    Task<InventoryMovementReportDto> GetInventoryMovementReportAsync(ReportFilterDto filter);
    Task<LowStockReportDto> GetLowStockReportAsync(ReportFilterDto filter);
    Task<ExpiryReportDto> GetExpiryReportAsync(ReportFilterDto filter);

    // 4. Production Reports
    Task<ProductionSummaryReportDto> GetProductionSummaryReportAsync(ReportFilterDto filter);
    Task<ProductionOrderPerformanceReportDto> GetProductionOrderPerformanceReportAsync(ReportFilterDto filter);
    Task<ProductionBatchReportDto> GetProductionBatchReportAsync(ReportFilterDto filter);
    Task<MaterialConsumptionVarianceReportDto> GetMaterialConsumptionVarianceReportAsync(ReportFilterDto filter);
    Task<ProductionCostSummaryReportDto> GetProductionCostSummaryReportAsync(ReportFilterDto filter);

    // 5. Waste Reports
    Task<WasteSummaryReportDto> GetWasteSummaryReportAsync(ReportFilterDto filter);
    Task<WasteTypeReportDto> GetWasteTypeReportAsync(ReportFilterDto filter);
    Task<WasteReasonReportDto> GetWasteReasonReportAsync(ReportFilterDto filter);

    // 6. Quality Reports
    Task<QualitySummaryReportDto> GetQualitySummaryReportAsync(ReportFilterDto filter);
    Task<ProductQualityReportDto> GetProductQualityReportAsync(ReportFilterDto filter);
    Task<QualityRejectionReportDto> GetQualityRejectionReportAsync(ReportFilterDto filter);

    // 7. Packaging Reports
    Task<PackagingSummaryReportDto> GetPackagingSummaryReportAsync(ReportFilterDto filter);
    Task<PackagingConsumptionReportDto> GetPackagingConsumptionReportAsync(ReportFilterDto filter);

    // 8. Finished Goods Reports
    Task<FinishedGoodsStockReportDto> GetFinishedGoodsStockReportAsync(ReportFilterDto filter);
    Task<FinishedGoodsReleaseReportDto> GetFinishedGoodsReleaseReportAsync(ReportFilterDto filter);
    Task<FinishedGoodsTraceabilityReportDto> GetFinishedGoodsTraceabilityReportAsync(string searchCodeOrBatch);

    // 9. Accounting Reports (Phase 16 Source-of-Truth)
    Task<ProfitAndLossReportDto> GetProfitAndLossReportAsync(ReportFilterDto filter);
    Task<BalanceSheetReportDto> GetBalanceSheetReportAsync(ReportFilterDto filter);
    Task<CustomerReceivablesReportDto> GetCustomerReceivablesReportAsync(ReportFilterDto filter);
    Task<SupplierPayablesReportDto> GetSupplierPayablesReportAsync(ReportFilterDto filter);
    Task<VatReportDto> GetVatReportAsync(ReportFilterDto filter);

    // 10. Management Profitability
    Task<ManagementProfitabilityReportDto> GetManagementProfitabilityReportAsync(ReportFilterDto filter);
}
