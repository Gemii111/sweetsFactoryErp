using FactoryX.Application.DTOs;
using FactoryX.Application.Services.Abstracts;
using FactoryX.Domain.Entities;
using FactoryX.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FactoryX.Application.Services.Concretes;

public class ReportingService : IReportingService
{
    private readonly AppDbContext _context;

    public ReportingService(AppDbContext context)
    {
        _context = context;
    }

    #region Executive Management Dashboard
    public async Task<ManagementDashboardDto> GetManagementDashboardAsync(ReportFilterDto filter)
    {
        var dto = new ManagementDashboardDto
        {
            FromDate = filter.FromDate,
            ToDate = filter.ToDate
        };

        var fromDate = filter.FromDate ?? DateTime.UtcNow.AddMonths(-1).Date;
        var toDate = filter.ToDate?.Date.AddDays(1).AddTicks(-1) ?? DateTime.UtcNow;

        // 1. Sales KPIs
        var salesOrdersQuery = _context.SalesOrders.AsNoTracking();
        if (filter.CustomerId.HasValue) salesOrdersQuery = salesOrdersQuery.Where(s => s.CustomerId == filter.CustomerId.Value);
        if (filter.FromDate.HasValue) salesOrdersQuery = salesOrdersQuery.Where(s => s.OrderDate >= fromDate);
        if (filter.ToDate.HasValue) salesOrdersQuery = salesOrdersQuery.Where(s => s.OrderDate <= toDate);

        dto.TotalSalesOrdersCount = await salesOrdersQuery.CountAsync();
        dto.TotalSalesRevenue = await salesOrdersQuery.SumAsync(s => (decimal?)s.TotalAmount) ?? 0;
        dto.FulfilledSalesOrdersCount = await salesOrdersQuery.CountAsync(s => s.Status == SalesOrderStatus.FullyFulfilled || s.Status == SalesOrderStatus.Closed);
        dto.OutstandingSalesOrdersCount = await salesOrdersQuery.CountAsync(s => s.Status == SalesOrderStatus.Confirmed || s.Status == SalesOrderStatus.PartiallyFulfilled);

        var invoicesQuery = _context.Invoices.AsNoTracking().Where(i => i.Status != InvoiceStatus.Draft && i.Status != InvoiceStatus.Cancelled);
        if (filter.CustomerId.HasValue) invoicesQuery = invoicesQuery.Where(i => i.CustomerId == filter.CustomerId.Value);
        dto.TotalInvoicedSales = await invoicesQuery.SumAsync(i => (decimal?)i.TotalAmount) ?? 0;
        var totalPaid = await invoicesQuery.SumAsync(i => (decimal?)i.PaidAmount) ?? 0;
        dto.TotalCustomerReceivables = Math.Max(0, dto.TotalInvoicedSales - totalPaid);

        // 2. Production KPIs
        var poQuery = _context.WorkOrders.AsNoTracking();
        var batchesQuery = _context.ProductionBatches.AsNoTracking();
        if (filter.FromDate.HasValue)
        {
            poQuery = poQuery.Where(p => p.PlannedDate >= fromDate);
            batchesQuery = batchesQuery.Where(b => b.CreatedAt >= fromDate);
        }
        if (filter.ToDate.HasValue)
        {
            poQuery = poQuery.Where(p => p.PlannedDate <= toDate);
            batchesQuery = batchesQuery.Where(b => b.CreatedAt <= toDate);
        }

        dto.TotalProductionOrdersCount = await poQuery.CountAsync();
        dto.TotalPlannedProductionKg = await poQuery.SumAsync(p => (decimal?)p.PlannedQuantity) ?? 0;
        dto.TotalActualProductionKg = await poQuery.SumAsync(p => (decimal?)p.ActualQuantityDecimal) ?? 0;
        dto.ActiveBatchesCount = await batchesQuery.CountAsync(b => b.Status == ProductionBatchStatus.Planned || b.Status == ProductionBatchStatus.InProgress);
        dto.CompletedBatchesCount = await batchesQuery.CountAsync(b => b.Status == ProductionBatchStatus.Completed);

        // 3. Inventory KPIs
        var rawStocks = await _context.StockBalances.AsNoTracking()
            .Include(s => s.Material)
            .Where(s => s.MaterialId.HasValue && s.Quantity > 0)
            .ToListAsync();

        dto.RawMaterialInventoryValue = rawStocks
            .Where(s => s.Material?.IsPackagingMaterial == false)
            .Sum(s => s.Quantity * (s.Material?.StandardCost ?? 0));

        dto.PackagingInventoryValue = rawStocks
            .Where(s => s.Material?.IsPackagingMaterial == true)
            .Sum(s => s.Quantity * (s.Material?.StandardCost ?? 0));

        dto.FinishedGoodsInventoryValue = await _context.FinishedGoodsStocks.AsNoTracking()
            .Where(f => f.Quantity > 0)
            .SumAsync(f => (decimal?)(f.Quantity * f.UnitCost)) ?? 0;

        dto.LowStockItemsCount = await _context.Materials.AsNoTracking()
            .CountAsync(m => m.IsActive && m.MinimumStock > 0 && m.CurrentStock < m.MinimumStock);

        var today = DateTime.UtcNow.Date;
        var thresholdDate = today.AddDays(30);
        var expiringMaterials = await _context.StockBalances.AsNoTracking()
            .CountAsync(s => s.ExpiryDate.HasValue && s.ExpiryDate.Value <= thresholdDate && s.Quantity > 0);
        var expiringFG = await _context.FinishedGoodsStocks.AsNoTracking()
            .CountAsync(f => f.ExpiryDate <= thresholdDate && f.Quantity > 0);
        dto.ExpiringLotsCount = expiringMaterials + expiringFG;
        dto.TotalInventoryTransactionsCount = await _context.InventoryTransactions.AsNoTracking().CountAsync();

        // 4. Waste KPIs
        var approvedWaste = _context.Wastes.AsNoTracking()
            .Include(w => w.WasteReason)
            .Where(w => w.Status == WasteStatus.Approved);

        if (filter.FromDate.HasValue) approvedWaste = approvedWaste.Where(w => w.WasteDate >= fromDate);
        if (filter.ToDate.HasValue) approvedWaste = approvedWaste.Where(w => w.WasteDate <= toDate);

        dto.TotalWasteQuantityKg = await approvedWaste.SumAsync(w => (decimal?)w.Quantity) ?? 0;
        dto.TotalWasteCost = await approvedWaste.SumAsync(w => (decimal?)w.TotalCost) ?? 0;

        var wasteByTypeGroup = await approvedWaste
            .GroupBy(w => w.WasteType)
            .Select(g => new WasteByTypeSummaryDto
            {
                TypeName = g.Key.ToString(),
                Quantity = g.Sum(w => w.Quantity),
                Cost = g.Sum(w => w.TotalCost)
            }).ToListAsync();
        dto.WasteByType = wasteByTypeGroup;

        var wasteByReasonGroup = await approvedWaste
            .GroupBy(w => w.WasteReason != null ? w.WasteReason.Description : (w.ReasonDescription != "" ? w.ReasonDescription : "غير محدد"))
            .Select(g => new WasteByReasonSummaryDto
            {
                ReasonDescription = g.Key,
                Count = g.Count(),
                Quantity = g.Sum(w => w.Quantity),
                Cost = g.Sum(w => w.TotalCost)
            }).ToListAsync();
        dto.WasteByReason = wasteByReasonGroup;

        // 5. Quality KPIs
        var qcQuery = _context.QualityInspections.AsNoTracking();
        if (filter.FromDate.HasValue) qcQuery = qcQuery.Where(q => q.InspectionDate >= fromDate);
        if (filter.ToDate.HasValue) qcQuery = qcQuery.Where(q => q.InspectionDate <= toDate);

        dto.TotalQCInspectionsCount = await qcQuery.CountAsync();
        dto.QCApprovedCount = await qcQuery.CountAsync(q => q.Status == QualityInspectionStatus.Approved || q.FinalDecision == QualityDecision.Approved);
        dto.QCRejectedCount = await qcQuery.CountAsync(q => q.Status == QualityInspectionStatus.Rejected || q.FinalDecision == QualityDecision.Rejected);
        dto.QCOnHoldCount = await qcQuery.CountAsync(q => q.Status == QualityInspectionStatus.Hold || q.FinalDecision == QualityDecision.Hold);
        dto.QCReinspectionCount = await qcQuery.CountAsync(q => q.PreviousInspectionId.HasValue);

        // 6. Purchasing KPIs
        var poPurchasingQuery = _context.PurchaseOrders.AsNoTracking();
        if (filter.SupplierId.HasValue) poPurchasingQuery = poPurchasingQuery.Where(p => p.SupplierId == filter.SupplierId.Value);
        if (filter.FromDate.HasValue) poPurchasingQuery = poPurchasingQuery.Where(p => p.OrderDate >= fromDate);
        if (filter.ToDate.HasValue) poPurchasingQuery = poPurchasingQuery.Where(p => p.OrderDate <= toDate);

        dto.TotalPurchaseOrdersCount = await poPurchasingQuery.CountAsync();
        dto.ReceivedPurchaseOrdersCount = await poPurchasingQuery.CountAsync(p => p.Status == PurchaseOrderStatus.FullyReceived || p.Status == PurchaseOrderStatus.Closed);
        dto.OutstandingPurchaseOrdersCount = await poPurchasingQuery.CountAsync(p => p.Status == PurchaseOrderStatus.Approved || p.Status == PurchaseOrderStatus.PartiallyReceived);

        var receiptsTotal = await _context.PurchaseReceipts.AsNoTracking()
            .Where(r => r.Status == PurchaseReceiptStatus.Posted)
            .SumAsync(r => (decimal?)r.TotalCost) ?? 0;
        var paymentsTotal = await _context.SupplierPayments.AsNoTracking()
            .Where(p => p.Status == PaymentStatus.Recorded)
            .SumAsync(p => (decimal?)p.Amount) ?? 0;
        dto.TotalSupplierPayables = Math.Max(0, receiptsTotal - paymentsTotal);

        // 7. Finance & Accounting KPIs (Source-of-truth from Phase 16 journal entries)
        var postedLines = _context.JournalEntryLines.AsNoTracking()
            .Include(l => l.Account)
            .Include(l => l.JournalEntry)
            .Where(l => l.JournalEntry.Status == JournalEntryStatus.Posted);

        dto.AccountingRevenue = await postedLines
            .Where(l => l.Account.AccountType == AccountType.Revenue || l.Account.AccountRole == AccountRole.SalesRevenue)
            .SumAsync(l => (decimal?)(l.Credit - l.Debit)) ?? 0;

        dto.AccountingCOGS = await postedLines
            .Where(l => l.Account.AccountRole == AccountRole.CostOfGoodsSold)
            .SumAsync(l => (decimal?)(l.Debit - l.Credit)) ?? 0;

        dto.AccountingOperatingExpenses = await postedLines
            .Where(l => l.Account.AccountType == AccountType.Expense && l.Account.AccountRole != AccountRole.CostOfGoodsSold && l.Account.AccountRole != AccountRole.ProductionClearing)
            .SumAsync(l => (decimal?)(l.Debit - l.Credit)) ?? 0;

        dto.TotalCashBalance = await postedLines
            .Where(l => l.Account.AccountRole == AccountRole.Cash)
            .SumAsync(l => (decimal?)(l.Debit - l.Credit)) ?? 0;

        dto.TotalBankBalance = await postedLines
            .Where(l => l.Account.AccountRole == AccountRole.Bank)
            .SumAsync(l => (decimal?)(l.Debit - l.Credit)) ?? 0;

        // 8. Trends
        var salesOrders = await _context.SalesOrders.AsNoTracking()
            .Where(s => s.OrderDate >= DateTime.UtcNow.AddMonths(-6))
            .OrderBy(s => s.OrderDate)
            .ToListAsync();

        dto.MonthlySalesTrend = salesOrders
            .GroupBy(s => s.OrderDate.ToString("yyyy-MM"))
            .Select(g => new SalesTrendDto
            {
                PeriodName = g.Key,
                Revenue = g.Sum(s => s.TotalAmount),
                Invoiced = g.Sum(s => s.TotalAmount)
            }).ToList();

        var prodOrders = await _context.WorkOrders.AsNoTracking()
            .Where(p => p.PlannedDate >= DateTime.UtcNow.AddMonths(-6))
            .OrderBy(p => p.PlannedDate)
            .ToListAsync();

        dto.MonthlyProductionTrend = prodOrders
            .GroupBy(p => p.PlannedDate.ToString("yyyy-MM"))
            .Select(g => new ProductionTrendDto
            {
                PeriodName = g.Key,
                PlannedQuantity = g.Sum(p => p.PlannedQuantity),
                ActualQuantity = g.Sum(p => p.ActualQuantityDecimal)
            }).ToList();

        var topProducts = await _context.SalesOrderItems.AsNoTracking()
            .Include(i => i.Product)
            .GroupBy(i => i.Product.ArabicName)
            .Select(g => new TopSellingProductDto
            {
                ProductName = g.Key,
                QuantitySold = g.Sum(i => i.Quantity),
                Revenue = g.Sum(i => i.TotalPrice)
            })
            .OrderByDescending(p => p.Revenue)
            .Take(5)
            .ToListAsync();
        dto.TopProducts = topProducts;

        return dto;
    }
    #endregion

    #region Sales Reports
    public async Task<SalesSummaryReportDto> GetSalesSummaryReportAsync(ReportFilterDto filter)
    {
        var query = _context.SalesOrders.AsNoTracking()
            .Include(s => s.Customer)
            .Include(s => s.Items).ThenInclude(i => i.Product)
            .AsQueryable();

        if (filter.CustomerId.HasValue) query = query.Where(s => s.CustomerId == filter.CustomerId.Value);
        if (filter.FromDate.HasValue) query = query.Where(s => s.OrderDate >= filter.FromDate.Value.Date);
        if (filter.ToDate.HasValue) query = query.Where(s => s.OrderDate <= filter.ToDate.Value.Date.AddDays(1).AddTicks(-1));
        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            query = query.Where(s => s.OrderNumber.Contains(filter.SearchTerm) || s.Customer.Name.Contains(filter.SearchTerm));
        }

        var orders = await query.OrderByDescending(s => s.OrderDate).ToListAsync();

        var fulfillments = await _context.SalesFulfillmentItems.AsNoTracking()
            .GroupBy(f => f.SalesOrderItem.SalesOrderId)
            .Select(g => new { SalesOrderId = g.Key, FulfilledQty = g.Sum(x => x.ShippedQuantity) })
            .ToListAsync();
        var fulfilledDict = fulfillments.ToDictionary(f => f.SalesOrderId, f => f.FulfilledQty);

        var invoices = await _context.Invoices.AsNoTracking()
            .Where(i => i.Status != InvoiceStatus.Cancelled)
            .GroupBy(i => i.SalesOrderId)
            .Select(g => new { SalesOrderId = g.Key, InvoicedAmt = g.Sum(x => x.TotalAmount), PaidAmt = g.Sum(x => x.PaidAmount) })
            .ToListAsync();
        var invoiceDict = invoices.ToDictionary(i => i.SalesOrderId, i => new { i.InvoicedAmt, i.PaidAmt });

        var report = new SalesSummaryReportDto { Filter = filter };
        report.TotalOrders = orders.Count;
        report.TotalOrderedQuantity = orders.Sum(o => o.Items.Sum(i => i.Quantity));
        report.TotalOrderValue = orders.Sum(o => o.TotalAmount);

        foreach (var o in orders)
        {
            var fulQty = fulfilledDict.ContainsKey(o.Id) ? fulfilledDict[o.Id] : 0;
            var invAmt = invoiceDict.ContainsKey(o.Id) ? invoiceDict[o.Id].InvoicedAmt : 0;
            var paidAmt = invoiceDict.ContainsKey(o.Id) ? invoiceDict[o.Id].PaidAmt : 0;

            report.TotalFulfilledQuantity += fulQty;
            report.TotalInvoicedAmount += invAmt;
            report.TotalPaidAmount += paidAmt;

            report.Items.Add(new SalesSummaryItemDto
            {
                OrderId = o.Id,
                OrderNumber = o.OrderNumber,
                OrderDate = o.OrderDate,
                CustomerCode = o.Customer?.Code ?? "",
                CustomerName = o.Customer?.Name ?? "",
                OrderedQuantity = o.Items.Sum(i => i.Quantity),
                FulfilledQuantity = fulQty,
                OrderAmount = o.TotalAmount,
                InvoicedAmount = invAmt,
                Status = o.Status.ToString()
            });
        }

        report.TotalOutstandingReceivable = Math.Max(0, report.TotalInvoicedAmount - report.TotalPaidAmount);

        // Group by Customer
        report.SalesByCustomer = orders
            .GroupBy(o => o.Customer)
            .Select(g => new CustomerSalesItemDto
            {
                CustomerId = g.Key?.Id ?? 0,
                CustomerCode = g.Key?.Code ?? "",
                CustomerName = g.Key?.Name ?? "غير محدد",
                CustomerType = g.Key?.Type.ToString() ?? "",
                TotalOrders = g.Count(),
                OrderedValue = g.Sum(o => o.TotalAmount),
                FulfilledValue = g.Sum(o => o.TotalAmount),
                InvoicedValue = g.Sum(o => invoiceDict.ContainsKey(o.Id) ? invoiceDict[o.Id].InvoicedAmt : 0),
                PaidValue = g.Sum(o => invoiceDict.ContainsKey(o.Id) ? invoiceDict[o.Id].PaidAmt : 0),
                OutstandingReceivable = g.Sum(o => invoiceDict.ContainsKey(o.Id) ? (invoiceDict[o.Id].InvoicedAmt - invoiceDict[o.Id].PaidAmt) : 0)
            }).ToList();

        // Group by Product
        var orderItems = orders.SelectMany(o => o.Items).ToList();
        report.SalesByProduct = orderItems
            .GroupBy(i => i.Product)
            .Select(g => new ProductSalesItemDto
            {
                ProductId = g.Key?.Id ?? 0,
                ProductCode = g.Key?.Code ?? "",
                ProductName = g.Key?.ArabicName ?? "",
                CategoryName = g.Key?.ProductCategory?.Name ?? "حلويات المولد",
                QuantitySold = g.Sum(i => i.Quantity),
                Unit = g.Key?.Unit ?? "KG",
                Revenue = g.Sum(i => i.TotalPrice),
                CostOfGoodsSold = g.Sum(i => i.Quantity * (g.Key?.StandardCost ?? 0))
            }).ToList();

        return report;
    }

    public async Task<SalesOrderStatusReportDto> GetSalesOrderStatusReportAsync(ReportFilterDto filter)
    {
        var query = _context.SalesOrders.AsNoTracking()
            .Include(s => s.Customer)
            .Include(s => s.Items)
            .AsQueryable();

        if (filter.CustomerId.HasValue) query = query.Where(s => s.CustomerId == filter.CustomerId.Value);
        if (filter.FromDate.HasValue) query = query.Where(s => s.OrderDate >= filter.FromDate.Value.Date);
        if (filter.ToDate.HasValue) query = query.Where(s => s.OrderDate <= filter.ToDate.Value.Date.AddDays(1).AddTicks(-1));
        if (!string.IsNullOrWhiteSpace(filter.Status) && Enum.TryParse<SalesOrderStatus>(filter.Status, out var st))
        {
            query = query.Where(s => s.Status == st);
        }

        var orders = await query.OrderByDescending(s => s.OrderDate).ToListAsync();

        var fulfillments = await _context.SalesFulfillmentItems.AsNoTracking()
            .GroupBy(f => f.SalesOrderItem.SalesOrderId)
            .Select(g => new { SalesOrderId = g.Key, FulfilledQty = g.Sum(x => x.ShippedQuantity) })
            .ToListAsync();
        var fulfilledDict = fulfillments.ToDictionary(f => f.SalesOrderId, f => f.FulfilledQty);

        var dto = new SalesOrderStatusReportDto { Filter = filter };
        dto.DraftCount = orders.Count(o => o.Status == SalesOrderStatus.Draft);
        dto.ConfirmedCount = orders.Count(o => o.Status == SalesOrderStatus.Confirmed);
        dto.PartiallyFulfilledCount = orders.Count(o => o.Status == SalesOrderStatus.PartiallyFulfilled);
        dto.FullyFulfilledCount = orders.Count(o => o.Status == SalesOrderStatus.FullyFulfilled);
        dto.ClosedCount = orders.Count(o => o.Status == SalesOrderStatus.Closed);

        dto.Orders = orders.Select(o => new SalesOrderStatusItemDto
        {
            OrderId = o.Id,
            OrderNumber = o.OrderNumber,
            OrderDate = o.OrderDate,
            RequiredDeliveryDate = o.RequiredDeliveryDate,
            CustomerName = o.Customer?.Name ?? "",
            TotalQuantity = o.Items.Sum(i => i.Quantity),
            FulfilledQuantity = fulfilledDict.ContainsKey(o.Id) ? fulfilledDict[o.Id] : 0,
            TotalAmount = o.TotalAmount,
            Status = o.Status.ToString()
        }).ToList();

        return dto;
    }

    public async Task<CustomerSalesReportDto> GetCustomerSalesReportAsync(ReportFilterDto filter)
    {
        var summary = await GetSalesSummaryReportAsync(filter);
        return new CustomerSalesReportDto
        {
            Filter = filter,
            Customers = summary.SalesByCustomer
        };
    }

    public async Task<ProductSalesReportDto> GetProductSalesReportAsync(ReportFilterDto filter)
    {
        var summary = await GetSalesSummaryReportAsync(filter);
        return new ProductSalesReportDto
        {
            Filter = filter,
            Products = summary.SalesByProduct
        };
    }
    #endregion

    #region Purchasing Reports
    public async Task<PurchaseSummaryReportDto> GetPurchaseSummaryReportAsync(ReportFilterDto filter)
    {
        var prQuery = _context.PurchaseRequests.AsNoTracking().AsQueryable();
        IQueryable<PurchaseOrder> poQuery = _context.PurchaseOrders.AsNoTracking().Include(p => p.Supplier).Include(p => p.Items);
        IQueryable<PurchaseReceipt> grnQuery = _context.PurchaseReceipts.AsNoTracking().Include(g => g.Supplier).Include(g => g.Items);

        if (filter.SupplierId.HasValue)
        {
            poQuery = poQuery.Where(p => p.SupplierId == filter.SupplierId.Value);
            grnQuery = grnQuery.Where(g => g.SupplierId == filter.SupplierId.Value);
        }
        if (filter.FromDate.HasValue)
        {
            prQuery = prQuery.Where(p => p.RequestDate >= filter.FromDate.Value.Date);
            poQuery = poQuery.Where(p => p.OrderDate >= filter.FromDate.Value.Date);
            grnQuery = grnQuery.Where(g => g.ReceiptDate >= filter.FromDate.Value.Date);
        }
        if (filter.ToDate.HasValue)
        {
            var to = filter.ToDate.Value.Date.AddDays(1).AddTicks(-1);
            prQuery = prQuery.Where(p => p.RequestDate <= to);
            poQuery = poQuery.Where(p => p.OrderDate <= to);
            grnQuery = grnQuery.Where(g => g.ReceiptDate <= to);
        }

        var report = new PurchaseSummaryReportDto { Filter = filter };
        report.TotalRequestsCount = await prQuery.CountAsync();
        
        var pos = await poQuery.ToListAsync();
        report.TotalOrdersCount = pos.Count;
        report.TotalOrderedQuantity = pos.Sum(p => p.Items.Sum(i => i.OrderedQuantity));
        report.TotalPurchaseValue = pos.Sum(p => p.TotalAmount);

        var grns = await grnQuery.ToListAsync();
        report.TotalReceiptsCount = grns.Count;
        report.TotalReceivedQuantity = grns.Sum(g => g.Items.Sum(i => i.ReceivedQuantity));
        report.TotalRejectedQuantity = grns.Sum(g => g.Items.Sum(i => i.RejectedQuantity));

        var payments = await _context.SupplierPayments.AsNoTracking()
            .Where(p => p.Status == PaymentStatus.Recorded)
            .GroupBy(p => p.SupplierId)
            .Select(g => new { SupplierId = g.Key, TotalPaid = g.Sum(p => p.Amount) })
            .ToListAsync();
        var paidDict = payments.ToDictionary(p => p.SupplierId, p => p.TotalPaid);

        report.SupplierSummaries = pos
            .GroupBy(p => p.Supplier)
            .Select(g => new SupplierPurchaseItemDto
            {
                SupplierId = g.Key?.Id ?? 0,
                SupplierCode = g.Key?.Code ?? "",
                SupplierName = g.Key?.Name ?? "غير محدد",
                OrdersCount = g.Count(),
                OrderedValue = g.Sum(p => p.TotalAmount),
                ReceivedValue = grns.Where(r => r.SupplierId == (g.Key != null ? g.Key.Id : 0)).Sum(r => r.TotalCost),
                PaidValue = (g.Key != null && paidDict.ContainsKey(g.Key.Id)) ? paidDict[g.Key.Id] : 0,
                OutstandingPayable = Math.Max(0, grns.Where(r => r.SupplierId == (g.Key != null ? g.Key.Id : 0)).Sum(r => r.TotalCost) - ((g.Key != null && paidDict.ContainsKey(g.Key.Id)) ? paidDict[g.Key.Id] : 0))
            }).ToList();

        return report;
    }

    public async Task<SupplierPurchaseReportDto> GetSupplierPurchaseReportAsync(ReportFilterDto filter)
    {
        var summary = await GetPurchaseSummaryReportAsync(filter);
        return new SupplierPurchaseReportDto
        {
            Filter = filter,
            Suppliers = summary.SupplierSummaries
        };
    }

    public async Task<PurchaseOrderStatusReportDto> GetPurchaseOrderStatusReportAsync(ReportFilterDto filter)
    {
        var query = _context.PurchaseOrders.AsNoTracking()
            .Include(p => p.Supplier)
            .Include(p => p.Items)
            .Include(p => p.Receipts).ThenInclude(r => r.Items)
            .AsQueryable();

        if (filter.SupplierId.HasValue) query = query.Where(p => p.SupplierId == filter.SupplierId.Value);
        if (filter.FromDate.HasValue) query = query.Where(p => p.OrderDate >= filter.FromDate.Value.Date);
        if (filter.ToDate.HasValue) query = query.Where(p => p.OrderDate <= filter.ToDate.Value.Date.AddDays(1).AddTicks(-1));
        if (!string.IsNullOrWhiteSpace(filter.Status) && Enum.TryParse<PurchaseOrderStatus>(filter.Status, out var st))
        {
            query = query.Where(p => p.Status == st);
        }

        var orders = await query.OrderByDescending(p => p.OrderDate).ToListAsync();

        return new PurchaseOrderStatusReportDto
        {
            Filter = filter,
            Orders = orders.Select(o => new PurchaseOrderItemStatusDto
            {
                OrderId = o.Id,
                OrderNumber = o.OrderNumber,
                OrderDate = o.OrderDate,
                SupplierName = o.Supplier?.Name ?? "",
                TotalOrderedQuantity = o.Items.Sum(i => i.OrderedQuantity),
                TotalReceivedQuantity = o.Receipts != null ? o.Receipts.Where(r => r.Status == PurchaseReceiptStatus.Posted).Sum(r => r.Items.Sum(i => i.AcceptedQuantity)) : 0,
                TotalCost = o.TotalAmount,
                Status = o.Status.ToString()
            }).ToList()
        };
    }

    public async Task<SupplierPriceHistoryReportDto> GetSupplierPriceHistoryReportAsync(ReportFilterDto filter)
    {
        var query = _context.SupplierPriceHistories.AsNoTracking()
            .Include(s => s.Supplier)
            .Include(s => s.Material)
            .AsQueryable();

        if (filter.SupplierId.HasValue) query = query.Where(s => s.SupplierId == filter.SupplierId.Value);
        if (filter.MaterialId.HasValue) query = query.Where(s => s.MaterialId == filter.MaterialId.Value);
        if (filter.FromDate.HasValue) query = query.Where(s => s.PurchaseDate >= filter.FromDate.Value.Date);
        if (filter.ToDate.HasValue) query = query.Where(s => s.PurchaseDate <= filter.ToDate.Value.Date.AddDays(1).AddTicks(-1));

        var items = await query.OrderByDescending(s => s.PurchaseDate).ToListAsync();

        return new SupplierPriceHistoryReportDto
        {
            Filter = filter,
            PriceHistories = items.Select(i => new SupplierPriceHistoryItemDto
            {
                Id = i.Id,
                SupplierName = i.Supplier?.Name ?? "",
                MaterialName = i.Material?.Name ?? "",
                PreviousPrice = i.UnitPrice,
                CurrentPrice = i.UnitPrice,
                EffectiveDate = i.PurchaseDate,
                Notes = $"سعر شراء توريد {i.Currency}"
            }).ToList()
        };
    }
    #endregion

    #region Inventory Reports
    public async Task<InventoryValuationReportDto> GetInventoryValuationReportAsync(ReportFilterDto filter)
    {
        var matQuery = _context.StockBalances.AsNoTracking()
            .Include(s => s.Warehouse)
            .Include(s => s.Material)
            .Where(s => s.MaterialId.HasValue && s.Quantity > 0);

        var fgQuery = _context.FinishedGoodsStocks.AsNoTracking()
            .Include(f => f.Warehouse)
            .Include(f => f.Product)
            .Where(f => f.Quantity > 0);

        if (filter.WarehouseId.HasValue)
        {
            matQuery = matQuery.Where(s => s.WarehouseId == filter.WarehouseId.Value);
            fgQuery = fgQuery.Where(f => f.WarehouseId == filter.WarehouseId.Value);
        }

        var matStocks = await matQuery.ToListAsync();
        var fgStocks = await fgQuery.ToListAsync();

        var dto = new InventoryValuationReportDto { Filter = filter };

        foreach (var m in matStocks)
        {
            var isPkg = m.Material?.IsPackagingMaterial == true;
            var type = isPkg ? "مواد تعبئة وتغليف" : "مواد خام";
            var unitCost = m.Material?.StandardCost ?? 0;
            var val = Math.Round(m.Quantity * unitCost, 2);

            if (isPkg) dto.TotalPackagingValuation += val;
            else dto.TotalRawMaterialValuation += val;

            dto.Items.Add(new InventoryValuationItemDto
            {
                ItemType = type,
                ItemCode = m.Material?.Code ?? "",
                ItemName = m.Material?.ArabicName ?? m.Material?.Name ?? "",
                WarehouseName = m.Warehouse?.Name ?? "",
                BatchNumber = m.BatchNumber,
                Quantity = m.Quantity,
                Unit = m.Material?.Unit ?? "KG",
                UnitCost = unitCost
            });
        }

        foreach (var f in fgStocks)
        {
            var val = Math.Round(f.Quantity * f.UnitCost, 2);
            dto.TotalFinishedGoodsValuation += val;

            dto.Items.Add(new InventoryValuationItemDto
            {
                ItemType = "منتج تام",
                ItemCode = f.Product?.Code ?? "",
                ItemName = f.Product?.ArabicName ?? f.Product?.Name ?? "",
                WarehouseName = f.Warehouse?.Name ?? "",
                BatchNumber = f.BatchNumber,
                Quantity = f.Quantity,
                Unit = f.Product?.Unit ?? "KG",
                UnitCost = f.UnitCost
            });
        }

        return dto;
    }

    public async Task<StockBalanceReportDto> GetStockBalanceReportAsync(ReportFilterDto filter)
    {
        var matQuery = _context.StockBalances.AsNoTracking()
            .Include(s => s.Warehouse)
            .Include(s => s.Location)
            .Include(s => s.Material)
            .AsQueryable();

        var fgQuery = _context.FinishedGoodsStocks.AsNoTracking()
            .Include(f => f.Warehouse)
            .Include(f => f.Location)
            .Include(f => f.Product)
            .AsQueryable();

        if (filter.WarehouseId.HasValue)
        {
            matQuery = matQuery.Where(s => s.WarehouseId == filter.WarehouseId.Value);
            fgQuery = fgQuery.Where(f => f.WarehouseId == filter.WarehouseId.Value);
        }

        var matStocks = await matQuery.ToListAsync();
        var fgStocks = await fgQuery.ToListAsync();

        var dto = new StockBalanceReportDto { Filter = filter };

        foreach (var m in matStocks)
        {
            dto.Stocks.Add(new StockBalanceItemDto
            {
                WarehouseName = m.Warehouse?.Name ?? "",
                LocationCode = m.Location?.Code,
                ItemType = m.Material?.IsPackagingMaterial == true ? "مواد تعبئة" : "مواد خام",
                ItemCode = m.Material?.Code ?? "",
                ItemName = m.Material?.ArabicName ?? m.Material?.Name ?? "",
                BatchNumber = m.BatchNumber,
                Quantity = m.Quantity,
                ReservedQuantity = 0,
                Unit = m.Material?.Unit ?? "KG",
                MinimumStock = m.Material?.MinimumStock ?? 0,
                MaximumStock = m.Material?.MaximumStock ?? 0
            });
        }

        foreach (var f in fgStocks)
        {
            dto.Stocks.Add(new StockBalanceItemDto
            {
                WarehouseName = f.Warehouse?.Name ?? "",
                LocationCode = f.Location?.Code,
                ItemType = "منتج تام",
                ItemCode = f.Product?.Code ?? "",
                ItemName = f.Product?.ArabicName ?? f.Product?.Name ?? "",
                BatchNumber = f.BatchNumber,
                Quantity = f.Quantity,
                ReservedQuantity = 0,
                Unit = f.Product?.Unit ?? "KG",
                MinimumStock = f.Product?.MinimumStock ?? 0,
                MaximumStock = 0
            });
        }

        return dto;
    }

    public async Task<InventoryMovementReportDto> GetInventoryMovementReportAsync(ReportFilterDto filter)
    {
        var query = _context.InventoryTransactions.AsNoTracking()
            .Include(t => t.Warehouse)
            .Include(t => t.Material)
            .Include(t => t.Product)
            .AsQueryable();

        if (filter.WarehouseId.HasValue) query = query.Where(t => t.WarehouseId == filter.WarehouseId.Value);
        if (filter.MaterialId.HasValue) query = query.Where(t => t.MaterialId == filter.MaterialId.Value);
        if (filter.ProductId.HasValue) query = query.Where(t => t.ProductId == filter.ProductId.Value);
        if (filter.FromDate.HasValue) query = query.Where(t => t.TransactionDate >= filter.FromDate.Value.Date);
        if (filter.ToDate.HasValue) query = query.Where(t => t.TransactionDate <= filter.ToDate.Value.Date.AddDays(1).AddTicks(-1));

        var txs = await query.OrderByDescending(t => t.TransactionDate).Take(100).ToListAsync();

        return new InventoryMovementReportDto
        {
            Filter = filter,
            Movements = txs.Select(t => new InventoryMovementItemDto
            {
                Date = t.TransactionDate,
                TransactionNumber = $"TX-{t.Id:D6}",
                TransactionType = t.TransactionType.ToString(),
                ItemName = t.Material != null ? (t.Material.ArabicName ?? t.Material.Name) : (t.Product != null ? (t.Product.ArabicName ?? t.Product.Name) : "عنصر مخزني"),
                WarehouseName = t.Warehouse?.Name ?? "",
                BatchNumber = t.BatchNumber,
                Quantity = t.Quantity,
                Unit = t.Unit,
                Direction = (t.TransactionType == InventoryTransactionType.PurchaseReceipt || t.TransactionType == InventoryTransactionType.ProductionOutput || t.TransactionType == InventoryTransactionType.FinishedGoodsReceipt) ? "وارد (IN)" : "صادر (OUT)",
                ReferenceDocument = t.ReferenceDocumentNumber,
                Notes = t.Notes
            }).ToList()
        };
    }

    public async Task<LowStockReportDto> GetLowStockReportAsync(ReportFilterDto filter)
    {
        var materials = await _context.Materials.AsNoTracking()
            .Where(m => m.IsActive && m.MinimumStock > 0 && m.CurrentStock < m.MinimumStock)
            .ToListAsync();

        var products = await _context.Products.AsNoTracking()
            .Where(p => p.IsActive && p.MinimumStock > 0)
            .ToListAsync();

        var fgStocks = await _context.FinishedGoodsStocks.AsNoTracking()
            .GroupBy(f => f.ProductId)
            .Select(g => new { ProductId = g.Key, TotalQty = g.Sum(x => x.Quantity) })
            .ToListAsync();
        var fgDict = fgStocks.ToDictionary(f => f.ProductId, f => f.TotalQty);

        var dto = new LowStockReportDto { Filter = filter };

        foreach (var m in materials)
        {
            dto.Items.Add(new LowStockItemDto
            {
                ItemType = m.IsPackagingMaterial ? "مواد تعبئة" : "مواد خام",
                ItemCode = m.Code,
                ItemName = m.ArabicName ?? m.Name,
                WarehouseName = "المخزن الرئيسي للخامات",
                CurrentQuantity = m.CurrentStock,
                MinimumQuantity = m.MinimumStock,
                Unit = m.Unit
            });
        }

        foreach (var p in products)
        {
            var cur = fgDict.ContainsKey(p.Id) ? fgDict[p.Id] : 0;
            if (cur < p.MinimumStock)
            {
                dto.Items.Add(new LowStockItemDto
                {
                    ItemType = "منتج تام",
                    ItemCode = p.Code,
                    ItemName = p.ArabicName ?? p.Name,
                    WarehouseName = "مخزن الإنتاج التام",
                    CurrentQuantity = cur,
                    MinimumQuantity = p.MinimumStock,
                    Unit = p.Unit
                });
            }
        }

        return dto;
    }

    public async Task<ExpiryReportDto> GetExpiryReportAsync(ReportFilterDto filter)
    {
        var matQuery = _context.StockBalances.AsNoTracking()
            .Include(s => s.Warehouse)
            .Include(s => s.Material)
            .Where(s => s.ExpiryDate.HasValue && s.Quantity > 0);

        var fgQuery = _context.FinishedGoodsStocks.AsNoTracking()
            .Include(f => f.Warehouse)
            .Include(f => f.Product)
            .Where(f => f.Quantity > 0);

        if (filter.WarehouseId.HasValue)
        {
            matQuery = matQuery.Where(s => s.WarehouseId == filter.WarehouseId.Value);
            fgQuery = fgQuery.Where(f => f.WarehouseId == filter.WarehouseId.Value);
        }

        var matStocks = await matQuery.ToListAsync();
        var fgStocks = await fgQuery.ToListAsync();

        var dto = new ExpiryReportDto { Filter = filter };

        foreach (var m in matStocks)
        {
            dto.Lots.Add(new ExpiryItemDto
            {
                ItemType = m.Material?.IsPackagingMaterial == true ? "مواد تعبئة" : "مواد خام",
                ItemCode = m.Material?.Code ?? "",
                ItemName = m.Material?.ArabicName ?? m.Material?.Name ?? "",
                BatchNumber = m.BatchNumber,
                WarehouseName = m.Warehouse?.Name ?? "",
                Quantity = m.Quantity,
                Unit = m.Material?.Unit ?? "KG",
                ProductionDate = m.ManufacturingDate,
                ExpiryDate = m.ExpiryDate!.Value
            });
        }

        foreach (var f in fgStocks)
        {
            dto.Lots.Add(new ExpiryItemDto
            {
                ItemType = "منتج تام",
                ItemCode = f.Product?.Code ?? "",
                ItemName = f.Product?.ArabicName ?? f.Product?.Name ?? "",
                BatchNumber = f.BatchNumber,
                WarehouseName = f.Warehouse?.Name ?? "",
                Quantity = f.Quantity,
                Unit = f.Product?.Unit ?? "KG",
                ProductionDate = f.ProductionDate,
                ExpiryDate = f.ExpiryDate
            });
        }

        dto.Lots = dto.Lots.OrderBy(l => l.ExpiryDate).ToList();
        return dto;
    }
    #endregion

    #region Production Reports
    public async Task<ProductionSummaryReportDto> GetProductionSummaryReportAsync(ReportFilterDto filter)
    {
        var perf = await GetProductionOrderPerformanceReportAsync(filter);
        var batches = _context.ProductionBatches.AsNoTracking();

        return new ProductionSummaryReportDto
        {
            Filter = filter,
            TotalProductionOrders = perf.Orders.Count,
            TotalPlannedOutputKg = perf.Orders.Sum(o => o.PlannedQuantity),
            TotalActualOutputKg = perf.Orders.Sum(o => o.ActualQuantity),
            CompletedBatchesCount = await batches.CountAsync(b => b.Status == ProductionBatchStatus.Completed),
            ActiveBatchesCount = await batches.CountAsync(b => b.Status == ProductionBatchStatus.Planned || b.Status == ProductionBatchStatus.InProgress),
            CancelledBatchesCount = await batches.CountAsync(b => b.Status == ProductionBatchStatus.Cancelled),
            Orders = perf.Orders
        };
    }

    public async Task<ProductionOrderPerformanceReportDto> GetProductionOrderPerformanceReportAsync(ReportFilterDto filter)
    {
        var query = _context.WorkOrders.AsNoTracking()
            .Include(p => p.Product)
            .AsQueryable();

        if (filter.ProductId.HasValue) query = query.Where(p => p.ProductId == filter.ProductId.Value);
        if (filter.FromDate.HasValue) query = query.Where(p => p.PlannedDate >= filter.FromDate.Value.Date);
        if (filter.ToDate.HasValue) query = query.Where(p => p.PlannedDate <= filter.ToDate.Value.Date.AddDays(1).AddTicks(-1));

        var orders = await query.OrderByDescending(p => p.PlannedDate).ToListAsync();

        return new ProductionOrderPerformanceReportDto
        {
            Filter = filter,
            Orders = orders.Select(o => new ProductionOrderPerformanceItemDto
            {
                OrderId = o.Id,
                OrderNumber = o.OrderNumber,
                ProductName = o.Product?.ArabicName ?? o.Product?.Name ?? "",
                PlannedQuantity = o.PlannedQuantity,
                ActualQuantity = o.ActualQuantityDecimal,
                PlannedDate = o.PlannedDate,
                CompletionDate = o.ActualCompletionDate,
                Status = o.Status.ToString()
            }).ToList()
        };
    }

    public async Task<ProductionBatchReportDto> GetProductionBatchReportAsync(ReportFilterDto filter)
    {
        var query = _context.ProductionBatches.AsNoTracking()
            .Include(b => b.Product)
            .Include(b => b.WorkOrder)
            .AsQueryable();

        if (filter.ProductId.HasValue) query = query.Where(b => b.ProductId == filter.ProductId.Value);
        if (filter.FromDate.HasValue) query = query.Where(b => b.CreatedAt >= filter.FromDate.Value.Date);
        if (filter.ToDate.HasValue) query = query.Where(b => b.CreatedAt <= filter.ToDate.Value.Date.AddDays(1).AddTicks(-1));

        var batches = await query.OrderByDescending(b => b.CreatedAt).ToListAsync();

        return new ProductionBatchReportDto
        {
            Filter = filter,
            Batches = batches.Select(b => new ProductionBatchItemDto
            {
                BatchId = b.Id,
                BatchNumber = b.BatchNumber,
                WorkOrderNumber = b.WorkOrder?.OrderNumber ?? "",
                ProductName = b.Product?.ArabicName ?? b.Product?.Name ?? "",
                PlannedQuantity = b.PlannedQuantity,
                ActualQuantity = b.ActualOutputQuantity,
                StartDate = b.StartTime,
                EndDate = b.EndTime,
                Status = b.Status.ToString()
            }).ToList()
        };
    }

    public async Task<MaterialConsumptionVarianceReportDto> GetMaterialConsumptionVarianceReportAsync(ReportFilterDto filter)
    {
        var batches = await _context.ProductionBatches.AsNoTracking()
            .Include(b => b.Product)
            .Include(b => b.WorkOrder)
            .Include(b => b.Consumptions)!.ThenInclude(c => c.Material)
            .OrderByDescending(b => b.CreatedAt)
            .Take(50)
            .ToListAsync();

        var dto = new MaterialConsumptionVarianceReportDto { Filter = filter };

        foreach (var b in batches)
        {
            if (b.Consumptions != null)
            {
                foreach (var c in b.Consumptions)
                {
                    dto.Variances.Add(new MaterialConsumptionVarianceItemDto
                    {
                        WorkOrderNumber = b.WorkOrder?.OrderNumber ?? "",
                        BatchNumber = b.BatchNumber,
                        ProductName = b.Product?.ArabicName ?? b.Product?.Name ?? "",
                        MaterialName = c.Material?.ArabicName ?? c.Material?.Name ?? "",
                        PlannedRequirementQuantity = c.PlannedQuantity,
                        ActualConsumedQuantity = c.ActualQuantity,
                        Unit = c.Unit
                    });
                }
            }
        }

        return dto;
    }

    public async Task<ProductionCostSummaryReportDto> GetProductionCostSummaryReportAsync(ReportFilterDto filter)
    {
        var batches = await _context.ProductionBatches.AsNoTracking()
            .Include(b => b.Product)
            .Include(b => b.Consumptions)!.ThenInclude(c => c.Material)
            .Where(b => b.Status == ProductionBatchStatus.Completed && b.ActualOutputQuantity > 0)
            .OrderByDescending(b => b.CreatedAt)
            .Take(50)
            .ToListAsync();

        var wastes = await _context.Wastes.AsNoTracking()
            .Where(w => w.ProductionBatchId.HasValue && w.Status == WasteStatus.Approved)
            .GroupBy(w => w.ProductionBatchId!.Value)
            .Select(g => new { BatchId = g.Key, TotalWasteCost = g.Sum(w => w.TotalCost) })
            .ToListAsync();
        var wasteDict = wastes.ToDictionary(w => w.BatchId, w => w.TotalWasteCost);

        var dto = new ProductionCostSummaryReportDto { Filter = filter };

        foreach (var b in batches)
        {
            var matCost = (b.Consumptions != null) ? b.Consumptions.Sum(c => c.ActualQuantity * (c.Material?.StandardCost ?? 0)) : 0;
            var wasteCost = wasteDict.ContainsKey(b.Id) ? wasteDict[b.Id] : 0;
            var laborCost = b.ActualOutputQuantity * 1.5m;
            var machineCost = b.ActualOutputQuantity * 0.8m;
            var overhead = b.ActualOutputQuantity * 0.5m;

            dto.CostSummaries.Add(new ProductionCostSummaryItemDto
            {
                BatchNumber = b.BatchNumber,
                ProductName = b.Product?.ArabicName ?? b.Product?.Name ?? "",
                OutputQuantity = b.ActualOutputQuantity,
                MaterialCost = matCost,
                LaborCost = laborCost,
                MachineCost = machineCost,
                OverheadCost = overhead,
                WasteCost = wasteCost
            });
        }

        return dto;
    }
    #endregion

    #region Waste Reports
    public async Task<WasteSummaryReportDto> GetWasteSummaryReportAsync(ReportFilterDto filter)
    {
        var query = _context.Wastes.AsNoTracking()
            .Include(w => w.WasteReason)
            .Include(w => w.Product)
            .Include(w => w.Material)
            .Where(w => w.Status == WasteStatus.Approved);

        if (filter.FromDate.HasValue) query = query.Where(w => w.WasteDate >= filter.FromDate.Value.Date);
        if (filter.ToDate.HasValue) query = query.Where(w => w.WasteDate <= filter.ToDate.Value.Date.AddDays(1).AddTicks(-1));

        var wastes = await query.OrderByDescending(w => w.WasteDate).ToListAsync();

        var dto = new WasteSummaryReportDto
        {
            Filter = filter,
            TotalWasteQuantity = wastes.Sum(w => w.Quantity),
            TotalWasteCost = wastes.Sum(w => w.TotalCost)
        };

        dto.WasteByType = wastes
            .GroupBy(w => w.WasteType.ToString())
            .Select(g => new WasteByTypeSummaryDto
            {
                TypeName = g.Key,
                Quantity = g.Sum(w => w.Quantity),
                Cost = g.Sum(w => w.TotalCost)
            }).ToList();

        dto.WasteByReason = wastes
            .GroupBy(w => w.WasteReason != null ? w.WasteReason.Description : (w.ReasonDescription != "" ? w.ReasonDescription : "غير محدد"))
            .Select(g => new WasteByReasonSummaryDto
            {
                ReasonDescription = g.Key,
                Count = g.Count(),
                Quantity = g.Sum(w => w.Quantity),
                Cost = g.Sum(w => w.TotalCost)
            }).ToList();

        dto.WasteRecords = wastes.Select(w => new WasteRecordReportItemDto
        {
            WasteNumber = w.WasteNumber,
            WasteDate = w.WasteDate,
            WasteType = w.WasteType.ToString(),
            ItemName = w.Product != null ? (w.Product.ArabicName ?? w.Product.Name) : (w.Material != null ? (w.Material.ArabicName ?? w.Material.Name) : ""),
            Quantity = w.Quantity,
            Unit = w.Unit,
            TotalCost = w.TotalCost,
            ReasonDescription = w.WasteReason != null ? w.WasteReason.Description : w.ReasonDescription,
            Status = w.Status.ToString()
        }).ToList();

        return dto;
    }

    public async Task<WasteTypeReportDto> GetWasteTypeReportAsync(ReportFilterDto filter)
    {
        var summary = await GetWasteSummaryReportAsync(filter);
        return new WasteTypeReportDto { Filter = filter, WasteTypes = summary.WasteByType };
    }

    public async Task<WasteReasonReportDto> GetWasteReasonReportAsync(ReportFilterDto filter)
    {
        var summary = await GetWasteSummaryReportAsync(filter);
        return new WasteReasonReportDto { Filter = filter, WasteReasons = summary.WasteByReason };
    }
    #endregion

    #region Quality Reports
    public async Task<QualitySummaryReportDto> GetQualitySummaryReportAsync(ReportFilterDto filter)
    {
        var query = _context.QualityInspections.AsNoTracking()
            .Include(q => q.Product)
            .AsQueryable();

        if (filter.FromDate.HasValue) query = query.Where(q => q.InspectionDate >= filter.FromDate.Value.Date);
        if (filter.ToDate.HasValue) query = query.Where(q => q.InspectionDate <= filter.ToDate.Value.Date.AddDays(1).AddTicks(-1));

        var inspections = await query.ToListAsync();

        var dto = new QualitySummaryReportDto
        {
            Filter = filter,
            TotalInspectionsCount = inspections.Count,
            ApprovedCount = inspections.Count(q => q.Status == QualityInspectionStatus.Approved || q.FinalDecision == QualityDecision.Approved),
            RejectedCount = inspections.Count(q => q.Status == QualityInspectionStatus.Rejected || q.FinalDecision == QualityDecision.Rejected),
            HoldCount = inspections.Count(q => q.Status == QualityInspectionStatus.Hold || q.FinalDecision == QualityDecision.Hold),
            CancelledCount = inspections.Count(q => q.Status == QualityInspectionStatus.Cancelled),
            ReinspectionCount = inspections.Count(q => q.PreviousInspectionId.HasValue)
        };

        dto.ProductQualitySummaries = inspections
            .GroupBy(q => q.Product)
            .Select(g => new ProductQualityItemDto
            {
                ProductId = g.Key?.Id ?? 0,
                ProductName = g.Key?.ArabicName ?? g.Key?.Name ?? "غير محدد",
                TotalInspections = g.Count(),
                ApprovedCount = g.Count(q => q.Status == QualityInspectionStatus.Approved || q.FinalDecision == QualityDecision.Approved),
                RejectedCount = g.Count(q => q.Status == QualityInspectionStatus.Rejected || q.FinalDecision == QualityDecision.Rejected),
                HoldCount = g.Count(q => q.Status == QualityInspectionStatus.Hold || q.FinalDecision == QualityDecision.Hold)
            }).ToList();

        return dto;
    }

    public async Task<ProductQualityReportDto> GetProductQualityReportAsync(ReportFilterDto filter)
    {
        var summary = await GetQualitySummaryReportAsync(filter);
        return new ProductQualityReportDto { Filter = filter, Products = summary.ProductQualitySummaries };
    }

    public async Task<QualityRejectionReportDto> GetQualityRejectionReportAsync(ReportFilterDto filter)
    {
        var rejections = await _context.QualityInspections.AsNoTracking()
            .Include(q => q.Product)
            .Include(q => q.ProductionBatch)
            .Include(q => q.Items)
            .Where(q => q.Status == QualityInspectionStatus.Rejected || q.Status == QualityInspectionStatus.Hold || q.FinalDecision == QualityDecision.Rejected || q.FinalDecision == QualityDecision.Hold)
            .OrderByDescending(q => q.InspectionDate)
            .ToListAsync();

        return new QualityRejectionReportDto
        {
            Filter = filter,
            Rejections = rejections.Select(r => new QualityRejectionItemDto
            {
                InspectionNumber = r.InspectionNumber,
                ProductName = r.Product?.ArabicName ?? r.Product?.Name ?? "",
                BatchNumber = r.ProductionBatch?.BatchNumber,
                InspectionDate = r.InspectionDate,
                Decision = r.Status.ToString(),
                FailedParameters = string.Join(", ", r.Items.Where(i => i.Result == ItemEvaluationResult.Fail || i.ActualPassFailValue == "FAIL").Select(i => i.SpecificationName)),
                Comments = r.Notes ?? r.RejectionReason
            }).ToList()
        };
    }
    #endregion

    #region Packaging Reports
    public async Task<PackagingSummaryReportDto> GetPackagingSummaryReportAsync(ReportFilterDto filter)
    {
        var orders = await _context.PackagingOrders.AsNoTracking()
            .Include(p => p.Product)
            .Include(p => p.Consumptions).ThenInclude(c => c.Material)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        var dto = new PackagingSummaryReportDto
        {
            Filter = filter,
            TotalPackagingOrders = orders.Count,
            CompletedPackagingOrders = orders.Count(o => o.Status == PackagingOrderStatus.Completed),
            TotalPlannedQuantity = orders.Sum(o => o.PlannedQuantity),
            TotalCompletedQuantity = orders.Sum(o => o.ActualQuantity),
            TotalPackagingCost = orders.Sum(o => o.PackagingMaterialCost)
        };

        var consumptions = await GetPackagingConsumptionReportAsync(filter);
        dto.Consumptions = consumptions.Items;

        return dto;
    }

    public async Task<PackagingConsumptionReportDto> GetPackagingConsumptionReportAsync(ReportFilterDto filter)
    {
        var orders = await _context.PackagingOrders.AsNoTracking()
            .Include(p => p.Product)
            .Include(p => p.Consumptions).ThenInclude(c => c.Material)
            .OrderByDescending(p => p.CreatedAt)
            .Take(50)
            .ToListAsync();

        var dto = new PackagingConsumptionReportDto { Filter = filter };

        foreach (var o in orders)
        {
            foreach (var c in o.Consumptions)
            {
                dto.Items.Add(new PackagingConsumptionItemDto
                {
                    PackagingOrderNumber = o.OrderNumber,
                    ProductName = o.Product?.ArabicName ?? o.Product?.Name ?? "",
                    PackagingMaterialName = c.Material?.ArabicName ?? c.Material?.Name ?? "",
                    PlannedQuantity = c.PlannedQuantity,
                    ActualQuantity = c.ActualQuantity,
                    Unit = c.Unit
                });
            }
        }

        return dto;
    }
    #endregion

    #region Finished Goods Reports
    public async Task<FinishedGoodsStockReportDto> GetFinishedGoodsStockReportAsync(ReportFilterDto filter)
    {
        var stocks = await _context.FinishedGoodsStocks.AsNoTracking()
            .Include(f => f.Warehouse)
            .Include(f => f.Product)
            .Where(f => f.Quantity > 0)
            .OrderBy(f => f.Product.ArabicName)
            .ToListAsync();

        return new FinishedGoodsStockReportDto
        {
            Filter = filter,
            TotalQuantity = stocks.Sum(s => s.Quantity),
            TotalValuation = stocks.Sum(s => s.Quantity * s.UnitCost),
            Items = stocks.Select(s => new FinishedGoodsStockItemDto
            {
                ProductName = s.Product?.ArabicName ?? s.Product?.Name ?? "",
                ProductCode = s.Product?.Code ?? "",
                WarehouseName = s.Warehouse?.Name ?? "",
                BatchNumber = s.BatchNumber,
                ExpiryDate = s.ExpiryDate,
                CurrentQuantity = s.Quantity,
                ReservedQuantity = 0,
                Unit = s.Product?.Unit ?? "KG",
                UnitCost = s.UnitCost
            }).ToList()
        };
    }

    public async Task<FinishedGoodsReleaseReportDto> GetFinishedGoodsReleaseReportAsync(ReportFilterDto filter)
    {
        var releases = await _context.FinishedGoodsReleases.AsNoTracking()
            .Include(r => r.Product)
            .Include(r => r.ProductionBatch)
            .Include(r => r.QCInspection)
            .Include(r => r.PackagingOrder)
            .OrderByDescending(r => r.ReleasedAt)
            .ToListAsync();

        return new FinishedGoodsReleaseReportDto
        {
            Filter = filter,
            Releases = releases.Select(r => new FinishedGoodsReleaseItemDto
            {
                ReleaseNumber = r.ReleaseNumber,
                ReleasedAt = r.ReleasedAt,
                ProductName = r.Product?.ArabicName ?? r.Product?.Name ?? "",
                BatchNumber = r.BatchNumber,
                ReleasedQuantity = r.Quantity,
                Unit = r.Unit,
                TotalCost = r.TotalCost,
                QCInspectionNumber = r.QCInspection?.InspectionNumber ?? (r.QCInspectionId.HasValue ? $"QC-{r.QCInspectionId}" : null),
                PackagingOrderNumber = r.PackagingOrder?.OrderNumber ?? (r.PackagingOrderId.HasValue ? $"PKG-{r.PackagingOrderId}" : null)
            }).ToList()
        };
    }

    public async Task<FinishedGoodsTraceabilityReportDto> GetFinishedGoodsTraceabilityReportAsync(string searchCodeOrBatch)
    {
        if (string.IsNullOrWhiteSpace(searchCodeOrBatch))
        {
            var latest = await _context.ProductionBatches.AsNoTracking().OrderByDescending(b => b.Id).FirstOrDefaultAsync();
            if (latest != null) searchCodeOrBatch = latest.BatchNumber;
        }

        var batch = await _context.ProductionBatches.AsNoTracking()
            .Include(b => b.Product)
            .Include(b => b.WorkOrder).ThenInclude(w => w.RecipeVersion)
            .Include(b => b.Consumptions)!.ThenInclude(c => c.Material)
            .FirstOrDefaultAsync(b => b.BatchNumber == searchCodeOrBatch || b.BatchNumber.Contains(searchCodeOrBatch));

        if (batch == null)
        {
            return new FinishedGoodsTraceabilityReportDto
            {
                QuerySearch = searchCodeOrBatch,
                Found = false
            };
        }

        var qc = await _context.QualityInspections.AsNoTracking()
            .FirstOrDefaultAsync(q => q.ProductionBatchId == batch.Id);

        var pkg = await _context.PackagingOrders.AsNoTracking()
            .FirstOrDefaultAsync(p => p.ProductionBatchId == batch.Id);

        var release = await _context.FinishedGoodsReleases.AsNoTracking()
            .FirstOrDefaultAsync(r => r.ProductionBatchId == batch.Id || r.BatchNumber == batch.BatchNumber);

        var fulfillments = await _context.SalesFulfillmentItems.AsNoTracking()
            .Include(f => f.SalesFulfillment).ThenInclude(sf => sf.Customer)
            .Include(f => f.SalesFulfillment).ThenInclude(sf => sf.SalesOrder)
            .Where(f => f.BatchNumber == batch.BatchNumber)
            .ToListAsync();

        var tree = new FinishedGoodsTraceabilityTreeDto
        {
            BatchNumber = batch.BatchNumber,
            ProductName = batch.Product?.ArabicName ?? batch.Product?.Name ?? "",
            ProductCode = batch.Product?.Code ?? "",
            ProductionDate = batch.StartTime ?? batch.CreatedAt,
            ExpiryDate = (batch.StartTime ?? batch.CreatedAt).AddDays(batch.Product?.ExpiryPeriodDays ?? 90),
            ProductionOrderNumber = batch.WorkOrder?.OrderNumber,
            RecipeCode = batch.WorkOrder?.RecipeVersion?.VersionNumber ?? "V1.0",
            QCInspectionNumber = qc?.InspectionNumber,
            QCDecision = qc != null ? qc.Status.ToString() : null,
            PackagingOrderNumber = pkg?.OrderNumber,
            ReleaseNumber = release?.ReleaseNumber,
            ReleasedAt = release?.ReleasedAt
        };

        // Upstream Raw Material Consumptions & GRNs
        if (batch.Consumptions != null)
        {
            foreach (var c in batch.Consumptions)
            {
                var grn = await _context.PurchaseReceiptItems.AsNoTracking()
                    .Include(i => i.PurchaseReceipt).ThenInclude(pr => pr.Supplier)
                    .FirstOrDefaultAsync(i => i.MaterialId == c.MaterialId);

                tree.RawMaterials.Add(new TraceabilityRawMaterialItemDto
                {
                    MaterialName = c.Material?.ArabicName ?? c.Material?.Name ?? "",
                    RawMaterialBatchNumber = c.RawMaterialBatchNumber,
                    ConsumedQuantity = c.ActualQuantity,
                    Unit = c.Unit,
                    PurchaseReceiptNumber = grn?.PurchaseReceipt?.ReceiptNumber,
                    SupplierName = grn?.PurchaseReceipt?.Supplier?.Name
                });
            }
        }

        // Downstream Sales Deliveries
        foreach (var f in fulfillments)
        {
            tree.SalesDeliveries.Add(new TraceabilitySalesItemDto
            {
                SalesOrderNumber = f.SalesFulfillment?.SalesOrder?.OrderNumber ?? "",
                FulfillmentNumber = f.SalesFulfillment?.FulfillmentNumber ?? "",
                CustomerName = f.SalesFulfillment?.Customer?.Name ?? "",
                ShippedDate = f.SalesFulfillment?.FulfillmentDate ?? DateTime.UtcNow,
                ShippedQuantity = f.ShippedQuantity,
                Unit = f.Unit
            });
        }

        return new FinishedGoodsTraceabilityReportDto
        {
            QuerySearch = searchCodeOrBatch,
            Found = true,
            TraceTree = tree
        };
    }
    #endregion

    #region Accounting Reports (Phase 16 Source-of-Truth)
    public async Task<ProfitAndLossReportDto> GetProfitAndLossReportAsync(ReportFilterDto filter)
    {
        var lines = _context.JournalEntryLines.AsNoTracking()
            .Include(l => l.Account)
            .Include(l => l.JournalEntry)
            .Where(l => l.JournalEntry.Status == JournalEntryStatus.Posted);

        if (filter.FromDate.HasValue) lines = lines.Where(l => l.JournalEntry.EntryDate >= filter.FromDate.Value.Date);
        if (filter.ToDate.HasValue) lines = lines.Where(l => l.JournalEntry.EntryDate <= filter.ToDate.Value.Date.AddDays(1).AddTicks(-1));

        var linesList = await lines.ToListAsync();

        var dto = new ProfitAndLossReportDto { Filter = filter };

        var revenues = linesList.Where(l => l.Account.AccountType == AccountType.Revenue).ToList();
        dto.SalesRevenue = revenues.Where(l => l.Account.AccountRole == AccountRole.SalesRevenue).Sum(l => l.Credit - l.Debit);
        dto.OtherRevenue = revenues.Where(l => l.Account.AccountRole != AccountRole.SalesRevenue).Sum(l => l.Credit - l.Debit);

        dto.RevenueLines = revenues
            .GroupBy(l => l.Account)
            .Select(g => new ProfitAndLossDetailLineDto
            {
                AccountCode = g.Key.AccountCode,
                AccountNameAr = g.Key.AccountNameAr,
                Amount = g.Sum(l => l.Credit - l.Debit)
            }).ToList();

        var cogsLines = linesList.Where(l => l.Account.AccountRole == AccountRole.CostOfGoodsSold).ToList();
        dto.CostOfGoodsSold = cogsLines.Sum(l => l.Debit - l.Credit);

        var wasteLines = linesList.Where(l => l.Account.AccountRole == AccountRole.WasteExpense).ToList();
        dto.WasteExpense = wasteLines.Sum(l => l.Debit - l.Credit);

        var generalExpenses = linesList.Where(l => l.Account.AccountType == AccountType.Expense && l.Account.AccountRole != AccountRole.CostOfGoodsSold && l.Account.AccountRole != AccountRole.WasteExpense && l.Account.AccountRole != AccountRole.ProductionClearing).ToList();
        dto.GeneralAndAdminExpenses = generalExpenses.Sum(l => l.Debit - l.Credit);

        var expenseGroup = linesList.Where(l => l.Account.AccountType == AccountType.Expense && l.Account.AccountRole != AccountRole.ProductionClearing).ToList();
        dto.ExpenseLines = expenseGroup
            .GroupBy(l => l.Account)
            .Select(g => new ProfitAndLossDetailLineDto
            {
                AccountCode = g.Key.AccountCode,
                AccountNameAr = g.Key.AccountNameAr,
                Amount = g.Sum(l => l.Debit - l.Credit)
            }).ToList();

        return dto;
    }

    public async Task<BalanceSheetReportDto> GetBalanceSheetReportAsync(ReportFilterDto filter)
    {
        var asOfDate = filter.ToDate ?? DateTime.UtcNow.Date;

        var lines = await _context.JournalEntryLines.AsNoTracking()
            .Include(l => l.Account)
            .Include(l => l.JournalEntry)
            .Where(l => l.JournalEntry.Status == JournalEntryStatus.Posted && l.JournalEntry.EntryDate <= asOfDate)
            .ToListAsync();

        var pnl = await GetProfitAndLossReportAsync(new ReportFilterDto { ToDate = asOfDate });

        var dto = new BalanceSheetReportDto
        {
            Filter = filter,
            AsOfDate = asOfDate,
            CurrentYearNetIncome = pnl.NetOperatingProfit
        };

        var assetLines = lines.Where(l => l.Account.AccountType == AccountType.Asset).ToList();
        dto.CashAndBankBalances = assetLines.Where(l => l.Account.AccountRole == AccountRole.Cash || l.Account.AccountRole == AccountRole.Bank).Sum(l => l.Debit - l.Credit);
        dto.AccountsReceivable = assetLines.Where(l => l.Account.AccountRole == AccountRole.AccountsReceivable).Sum(l => l.Debit - l.Credit);
        dto.RawMaterialInventory = assetLines.Where(l => l.Account.AccountRole == AccountRole.RawMaterialInventory).Sum(l => l.Debit - l.Credit);
        dto.PackagingInventory = assetLines.Where(l => l.Account.AccountRole == AccountRole.PackagingInventory).Sum(l => l.Debit - l.Credit);
        dto.FinishedGoodsInventory = assetLines.Where(l => l.Account.AccountRole == AccountRole.FinishedGoodsInventory).Sum(l => l.Debit - l.Credit);

        dto.AssetAccounts = assetLines
            .GroupBy(l => l.Account)
            .Select(g => new BalanceSheetAccountItemDto
            {
                AccountCode = g.Key.AccountCode,
                AccountNameAr = g.Key.AccountNameAr,
                Balance = g.Sum(l => l.Debit - l.Credit)
            }).ToList();

        var liabilityLines = lines.Where(l => l.Account.AccountType == AccountType.Liability).ToList();
        dto.AccountsPayable = liabilityLines.Where(l => l.Account.AccountRole == AccountRole.AccountsPayable).Sum(l => l.Credit - l.Debit);
        dto.OutputVatLiability = liabilityLines.Where(l => l.Account.AccountRole == AccountRole.OutputVat).Sum(l => l.Credit - l.Debit);

        dto.LiabilityAccounts = liabilityLines
            .GroupBy(l => l.Account)
            .Select(g => new BalanceSheetAccountItemDto
            {
                AccountCode = g.Key.AccountCode,
                AccountNameAr = g.Key.AccountNameAr,
                Balance = g.Sum(l => l.Credit - l.Debit)
            }).ToList();

        var equityLines = lines.Where(l => l.Account.AccountType == AccountType.Equity).ToList();
        dto.PaidInCapital = equityLines.Sum(l => l.Credit - l.Debit);

        dto.EquityAccounts = equityLines
            .GroupBy(l => l.Account)
            .Select(g => new BalanceSheetAccountItemDto
            {
                AccountCode = g.Key.AccountCode,
                AccountNameAr = g.Key.AccountNameAr,
                Balance = g.Sum(l => l.Credit - l.Debit)
            }).ToList();

        return dto;
    }

    public async Task<CustomerReceivablesReportDto> GetCustomerReceivablesReportAsync(ReportFilterDto filter)
    {
        var arLines = await _context.JournalEntryLines.AsNoTracking()
            .Include(l => l.Account)
            .Include(l => l.JournalEntry)
            .Where(l => l.JournalEntry.Status == JournalEntryStatus.Posted && l.Account.AccountRole == AccountRole.AccountsReceivable)
            .ToListAsync();

        var controlBalance = arLines.Sum(l => l.Debit - l.Credit);

        var customers = await _context.Customers.AsNoTracking().ToListAsync();
        var invoices = await _context.Invoices.AsNoTracking().Where(i => i.Status != InvoiceStatus.Cancelled).ToListAsync();
        var payments = await _context.Payments.AsNoTracking().Where(p => p.Status == PaymentStatus.Recorded).ToListAsync();

        var dto = new CustomerReceivablesReportDto
        {
            Filter = filter,
            AccountingControlBalance = controlBalance
        };

        foreach (var c in customers)
        {
            var invAmt = invoices.Where(i => i.CustomerId == c.Id).Sum(i => i.TotalAmount);
            var paidAmt = payments.Where(p => p.CustomerId == c.Id).Sum(p => p.Amount);
            var outAmt = Math.Max(0, invAmt - paidAmt);

            dto.SubledgerTotalReceivable += outAmt;
            dto.Customers.Add(new CustomerReceivableItemDto
            {
                CustomerId = c.Id,
                CustomerCode = c.Code,
                CustomerName = c.Name,
                InvoicedAmount = invAmt,
                PaidAmount = paidAmt,
                OutstandingReceivable = outAmt,
                CurrentBalanceInDb = c.CurrentBalance
            });
        }

        return dto;
    }

    public async Task<SupplierPayablesReportDto> GetSupplierPayablesReportAsync(ReportFilterDto filter)
    {
        var apLines = await _context.JournalEntryLines.AsNoTracking()
            .Include(l => l.Account)
            .Include(l => l.JournalEntry)
            .Where(l => l.JournalEntry.Status == JournalEntryStatus.Posted && l.Account.AccountRole == AccountRole.AccountsPayable)
            .ToListAsync();

        var controlBalance = apLines.Sum(l => l.Credit - l.Debit);

        var suppliers = await _context.Suppliers.AsNoTracking().ToListAsync();
        var receipts = await _context.PurchaseReceipts.AsNoTracking().Where(r => r.Status == PurchaseReceiptStatus.Posted).ToListAsync();
        var payments = await _context.SupplierPayments.AsNoTracking().Where(p => p.Status == PaymentStatus.Recorded).ToListAsync();

        var dto = new SupplierPayablesReportDto
        {
            Filter = filter,
            AccountingControlBalance = controlBalance
        };

        foreach (var s in suppliers)
        {
            var purAmt = receipts.Where(r => r.SupplierId == s.Id).Sum(r => r.TotalCost);
            var paidAmt = payments.Where(p => p.SupplierId == s.Id).Sum(p => p.Amount);
            var outAmt = Math.Max(0, purAmt - paidAmt);

            dto.SubledgerTotalPayable += outAmt;
            dto.Suppliers.Add(new SupplierPayableItemDto
            {
                SupplierId = s.Id,
                SupplierCode = s.Code,
                SupplierName = s.Name,
                TotalPurchases = purAmt,
                TotalPaid = paidAmt,
                OutstandingPayable = outAmt
            });
        }

        return dto;
    }

    public async Task<VatReportDto> GetVatReportAsync(ReportFilterDto filter)
    {
        var vatLines = await _context.JournalEntryLines.AsNoTracking()
            .Include(l => l.Account)
            .Include(l => l.JournalEntry)
            .Where(l => l.JournalEntry.Status == JournalEntryStatus.Posted && (l.Account.AccountRole == AccountRole.OutputVat || l.Account.AccountRole == AccountRole.InputVat))
            .ToListAsync();

        var dto = new VatReportDto
        {
            Filter = filter,
            OutputVatTotal = vatLines.Where(l => l.Account.AccountRole == AccountRole.OutputVat).Sum(l => l.Credit - l.Debit),
            InputVatTotal = vatLines.Where(l => l.Account.AccountRole == AccountRole.InputVat).Sum(l => l.Debit - l.Credit),
            Transactions = vatLines.Select(l => new VatTransactionItemDto
            {
                Date = l.JournalEntry.EntryDate,
                JournalNumber = l.JournalEntry.JournalNumber,
                Type = l.Account.AccountRole == AccountRole.OutputVat ? "ضريبة مخرجات (مبيعات)" : "ضريبة مدخلات (مشتريات)",
                DocumentNumber = l.JournalEntry.ReferenceDocumentNumber ?? "",
                PartnerName = l.CustomerId.HasValue ? $"عميل #{l.CustomerId}" : (l.SupplierId.HasValue ? $"مورد #{l.SupplierId}" : ""),
                TaxableAmount = Math.Round((l.Credit > 0 ? l.Credit : l.Debit) / 0.14m, 2),
                VatAmount = l.Credit > 0 ? l.Credit : l.Debit
            }).ToList()
        };

        return dto;
    }
    #endregion

    #region Management Profitability
    public async Task<ManagementProfitabilityReportDto> GetManagementProfitabilityReportAsync(ReportFilterDto filter)
    {
        var sales = await GetSalesSummaryReportAsync(filter);

        var dto = new ManagementProfitabilityReportDto
        {
            Filter = filter,
            TotalRevenue = sales.SalesByProduct.Sum(p => p.Revenue),
            TotalCOGS = sales.SalesByProduct.Sum(p => p.CostOfGoodsSold),
            ProductProfitability = sales.SalesByProduct.Select(p => new ProductProfitabilityItemDto
            {
                ProductId = p.ProductId,
                ProductCode = p.ProductCode,
                ProductName = p.ProductName,
                CategoryName = p.CategoryName,
                QuantitySold = p.QuantitySold,
                Revenue = p.Revenue,
                CostOfGoodsSold = p.CostOfGoodsSold
            }).ToList(),
            CustomerProfitability = sales.SalesByCustomer.Select(c => new CustomerProfitabilityItemDto
            {
                CustomerId = c.CustomerId,
                CustomerCode = c.CustomerCode,
                CustomerName = c.CustomerName,
                Revenue = c.InvoicedValue,
                CostOfGoodsSold = Math.Round(c.InvoicedValue * 0.65m, 2)
            }).ToList()
        };

        return dto;
    }
    #endregion
}
