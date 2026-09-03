using FactoryX.Application.DTOs;
using FactoryX.Application.Services.Abstracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FactoryX.Web.Controllers;

[Authorize]
public class ReportsController : Controller
{
    private readonly IReportingService _reportingService;
    private readonly IWarehouseService _warehouseService;
    private readonly IProductService _productService;
    private readonly IMaterialService _materialService;
    private readonly ICustomerService _customerService;
    private readonly ISupplierService _supplierService;

    public ReportsController(
        IReportingService reportingService,
        IWarehouseService warehouseService,
        IProductService productService,
        IMaterialService materialService,
        ICustomerService customerService,
        ISupplierService supplierService)
    {
        _reportingService = reportingService;
        _warehouseService = warehouseService;
        _productService = productService;
        _materialService = materialService;
        _customerService = customerService;
        _supplierService = supplierService;
    }

    private async Task PopulateFilterLookupsAsync()
    {
        ViewBag.Warehouses = await _warehouseService.GetAllAsync();
        ViewBag.Products = await _productService.GetActiveProductsAsync();
        ViewBag.Materials = await _materialService.GetAllMaterialsAsync();
        ViewBag.Customers = await _customerService.GetAllCustomersAsync();
        ViewBag.Suppliers = await _supplierService.GetAllSuppliersAsync();
    }

    // Default entry point
    public async Task<IActionResult> Index(ReportFilterDto filter)
    {
        return await Dashboard(filter);
    }

    // 0. Executive Management Dashboard
    public async Task<IActionResult> Dashboard(ReportFilterDto filter)
    {
        await PopulateFilterLookupsAsync();
        var report = await _reportingService.GetManagementDashboardAsync(filter);
        return View("Dashboard", report);
    }

    // 1. Sales Reports
    public async Task<IActionResult> SalesSummary(ReportFilterDto filter)
    {
        await PopulateFilterLookupsAsync();
        var report = await _reportingService.GetSalesSummaryReportAsync(filter);
        return View(report);
    }

    public async Task<IActionResult> SalesOrderStatus(ReportFilterDto filter)
    {
        await PopulateFilterLookupsAsync();
        var report = await _reportingService.GetSalesOrderStatusReportAsync(filter);
        return View(report);
    }

    public async Task<IActionResult> CustomerSales(ReportFilterDto filter)
    {
        await PopulateFilterLookupsAsync();
        var report = await _reportingService.GetCustomerSalesReportAsync(filter);
        return View(report);
    }

    public async Task<IActionResult> ProductSales(ReportFilterDto filter)
    {
        await PopulateFilterLookupsAsync();
        var report = await _reportingService.GetProductSalesReportAsync(filter);
        return View(report);
    }

    // 2. Purchasing Reports
    public async Task<IActionResult> PurchaseSummary(ReportFilterDto filter)
    {
        await PopulateFilterLookupsAsync();
        var report = await _reportingService.GetPurchaseSummaryReportAsync(filter);
        return View(report);
    }

    public async Task<IActionResult> SupplierPurchases(ReportFilterDto filter)
    {
        await PopulateFilterLookupsAsync();
        var report = await _reportingService.GetSupplierPurchaseReportAsync(filter);
        return View(report);
    }

    public async Task<IActionResult> PurchaseOrderStatus(ReportFilterDto filter)
    {
        await PopulateFilterLookupsAsync();
        var report = await _reportingService.GetPurchaseOrderStatusReportAsync(filter);
        return View(report);
    }

    public async Task<IActionResult> SupplierPriceHistory(ReportFilterDto filter)
    {
        await PopulateFilterLookupsAsync();
        var report = await _reportingService.GetSupplierPriceHistoryReportAsync(filter);
        return View(report);
    }

    // 3. Inventory Reports
    public async Task<IActionResult> InventoryValuation(ReportFilterDto filter)
    {
        await PopulateFilterLookupsAsync();
        var report = await _reportingService.GetInventoryValuationReportAsync(filter);
        return View(report);
    }

    public async Task<IActionResult> StockBalance(ReportFilterDto filter)
    {
        await PopulateFilterLookupsAsync();
        var report = await _reportingService.GetStockBalanceReportAsync(filter);
        return View(report);
    }

    public async Task<IActionResult> InventoryMovements(ReportFilterDto filter)
    {
        await PopulateFilterLookupsAsync();
        var report = await _reportingService.GetInventoryMovementReportAsync(filter);
        return View(report);
    }

    public async Task<IActionResult> LowStock(ReportFilterDto filter)
    {
        await PopulateFilterLookupsAsync();
        var report = await _reportingService.GetLowStockReportAsync(filter);
        return View(report);
    }

    public async Task<IActionResult> Expiry(ReportFilterDto filter)
    {
        await PopulateFilterLookupsAsync();
        var report = await _reportingService.GetExpiryReportAsync(filter);
        return View(report);
    }

    // 4. Production Reports
    public async Task<IActionResult> ProductionSummary(ReportFilterDto filter)
    {
        await PopulateFilterLookupsAsync();
        var report = await _reportingService.GetProductionSummaryReportAsync(filter);
        return View(report);
    }

    public async Task<IActionResult> ProductionOrderPerformance(ReportFilterDto filter)
    {
        await PopulateFilterLookupsAsync();
        var report = await _reportingService.GetProductionOrderPerformanceReportAsync(filter);
        return View(report);
    }

    public async Task<IActionResult> ProductionBatches(ReportFilterDto filter)
    {
        await PopulateFilterLookupsAsync();
        var report = await _reportingService.GetProductionBatchReportAsync(filter);
        return View(report);
    }

    public async Task<IActionResult> MaterialVariance(ReportFilterDto filter)
    {
        await PopulateFilterLookupsAsync();
        var report = await _reportingService.GetMaterialConsumptionVarianceReportAsync(filter);
        return View(report);
    }

    public async Task<IActionResult> ProductionCost(ReportFilterDto filter)
    {
        await PopulateFilterLookupsAsync();
        var report = await _reportingService.GetProductionCostSummaryReportAsync(filter);
        return View(report);
    }

    // 5. Waste Reports
    public async Task<IActionResult> WasteSummary(ReportFilterDto filter)
    {
        await PopulateFilterLookupsAsync();
        var report = await _reportingService.GetWasteSummaryReportAsync(filter);
        return View(report);
    }

    public async Task<IActionResult> WasteByType(ReportFilterDto filter)
    {
        await PopulateFilterLookupsAsync();
        var report = await _reportingService.GetWasteTypeReportAsync(filter);
        return View(report);
    }

    public async Task<IActionResult> WasteByReason(ReportFilterDto filter)
    {
        await PopulateFilterLookupsAsync();
        var report = await _reportingService.GetWasteReasonReportAsync(filter);
        return View(report);
    }

    // 6. Quality Reports
    public async Task<IActionResult> QualitySummary(ReportFilterDto filter)
    {
        await PopulateFilterLookupsAsync();
        var report = await _reportingService.GetQualitySummaryReportAsync(filter);
        return View(report);
    }

    public async Task<IActionResult> ProductQuality(ReportFilterDto filter)
    {
        await PopulateFilterLookupsAsync();
        var report = await _reportingService.GetProductQualityReportAsync(filter);
        return View(report);
    }

    public async Task<IActionResult> QualityRejections(ReportFilterDto filter)
    {
        await PopulateFilterLookupsAsync();
        var report = await _reportingService.GetQualityRejectionReportAsync(filter);
        return View(report);
    }

    // 7. Packaging Reports
    public async Task<IActionResult> PackagingSummary(ReportFilterDto filter)
    {
        await PopulateFilterLookupsAsync();
        var report = await _reportingService.GetPackagingSummaryReportAsync(filter);
        return View(report);
    }

    public async Task<IActionResult> PackagingConsumption(ReportFilterDto filter)
    {
        await PopulateFilterLookupsAsync();
        var report = await _reportingService.GetPackagingConsumptionReportAsync(filter);
        return View(report);
    }

    // 8. Finished Goods Reports
    public async Task<IActionResult> FinishedGoodsStock(ReportFilterDto filter)
    {
        await PopulateFilterLookupsAsync();
        var report = await _reportingService.GetFinishedGoodsStockReportAsync(filter);
        return View(report);
    }

    public async Task<IActionResult> FinishedGoodsReleases(ReportFilterDto filter)
    {
        await PopulateFilterLookupsAsync();
        var report = await _reportingService.GetFinishedGoodsReleaseReportAsync(filter);
        return View(report);
    }

    public async Task<IActionResult> Traceability(string? search)
    {
        var report = await _reportingService.GetFinishedGoodsTraceabilityReportAsync(search ?? "");
        return View(report);
    }

    // 9. Accounting Reports
    public async Task<IActionResult> ProfitAndLoss(ReportFilterDto filter)
    {
        await PopulateFilterLookupsAsync();
        var report = await _reportingService.GetProfitAndLossReportAsync(filter);
        return View(report);
    }

    public async Task<IActionResult> BalanceSheet(ReportFilterDto filter)
    {
        await PopulateFilterLookupsAsync();
        var report = await _reportingService.GetBalanceSheetReportAsync(filter);
        return View(report);
    }

    public async Task<IActionResult> CustomerReceivables(ReportFilterDto filter)
    {
        await PopulateFilterLookupsAsync();
        var report = await _reportingService.GetCustomerReceivablesReportAsync(filter);
        return View(report);
    }

    public async Task<IActionResult> SupplierPayables(ReportFilterDto filter)
    {
        await PopulateFilterLookupsAsync();
        var report = await _reportingService.GetSupplierPayablesReportAsync(filter);
        return View(report);
    }

    public async Task<IActionResult> Vat(ReportFilterDto filter)
    {
        await PopulateFilterLookupsAsync();
        var report = await _reportingService.GetVatReportAsync(filter);
        return View(report);
    }

    // 10. Management Profitability
    public async Task<IActionResult> Profitability(ReportFilterDto filter)
    {
        await PopulateFilterLookupsAsync();
        var report = await _reportingService.GetManagementProfitabilityReportAsync(filter);
        return View(report);
    }
}
