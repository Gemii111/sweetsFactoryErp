using FactoryX.Application.DTOs;
using FactoryX.Application.Services.Abstracts;
using FactoryX.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FactoryX.Web.Controllers;

public class FinishedGoodsController : Controller
{
    private readonly IServiceManager _serviceManager;

    public FinishedGoodsController(IServiceManager serviceManager)
    {
        _serviceManager = serviceManager;
    }

    public async Task<IActionResult> Index(
        int? warehouseId,
        int? locationId,
        int? productId,
        string? batchNumber)
    {
        var stocks = await _serviceManager.FinishedGoodsService.GetStockAsync(
            warehouseId, locationId, productId, batchNumber);

        var summary = await _serviceManager.FinishedGoodsService.GetStockSummaryAsync();

        var warehouses = await _serviceManager.WarehouseService.GetAllAsync();
        var fgWarehouses = warehouses.Where(w => w.Type == WarehouseType.FinishedGoods && w.IsActive).ToList();
        var products = await _serviceManager.ProductService.GetActiveProductsAsync();

        ViewBag.Warehouses = new SelectList(fgWarehouses, "Id", "Name", warehouseId);
        ViewBag.Products = new SelectList(products, "Id", "Name", productId);
        ViewBag.Summary = summary;
        ViewBag.SelectedWarehouseId = warehouseId;
        ViewBag.SelectedProductId = productId;
        ViewBag.BatchNumber = batchNumber;

        return View(stocks);
    }

    public async Task<IActionResult> Details(int id)
    {
        var stock = await _serviceManager.FinishedGoodsService.GetStockByIdAsync(id);
        if (stock == null)
        {
            TempData["ErrorMessage"] = "سجل مخزون المنتج التام المطلوب غير موجود.";
            return RedirectToAction(nameof(Index));
        }

        return View(stock);
    }

    public async Task<IActionResult> Movements(
        int? warehouseId,
        int? productId,
        string? batchNumber,
        InventoryTransactionType? transactionType,
        DateTime? fromDate,
        DateTime? toDate)
    {
        var movements = await _serviceManager.FinishedGoodsService.GetStockMovementsAsync(
            warehouseId, productId, batchNumber, transactionType, fromDate, toDate);

        var warehouses = await _serviceManager.WarehouseService.GetAllAsync();
        var fgWarehouses = warehouses.Where(w => w.Type == WarehouseType.FinishedGoods && w.IsActive).ToList();
        var products = await _serviceManager.ProductService.GetActiveProductsAsync();

        ViewBag.Warehouses = new SelectList(fgWarehouses, "Id", "Name", warehouseId);
        ViewBag.Products = new SelectList(products, "Id", "Name", productId);
        ViewBag.SelectedWarehouseId = warehouseId;
        ViewBag.SelectedProductId = productId;
        ViewBag.BatchNumber = batchNumber;
        ViewBag.SelectedTransactionType = transactionType;
        ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
        ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");

        return View(movements);
    }

    [HttpGet]
    public async Task<IActionResult> Adjust(int? stockId)
    {
        var warehouses = await _serviceManager.WarehouseService.GetAllAsync();
        var fgWarehouses = warehouses.Where(w => w.Type == WarehouseType.FinishedGoods && w.IsActive).ToList();
        var products = await _serviceManager.ProductService.GetActiveProductsAsync();

        ViewBag.Warehouses = new SelectList(fgWarehouses, "Id", "Name");
        ViewBag.Products = new SelectList(products, "Id", "Name");

        var model = new FinishedGoodsAdjustmentRequest();

        if (stockId.HasValue && stockId.Value > 0)
        {
            var stock = await _serviceManager.FinishedGoodsService.GetStockByIdAsync(stockId.Value);
            if (stock != null)
            {
                model.WarehouseId = stock.WarehouseId;
                model.LocationId = stock.LocationId;
                model.ProductId = stock.ProductId;
                model.BatchNumber = stock.BatchNumber;
                model.ActualQuantity = stock.Quantity;
                model.Unit = stock.Unit;
                ViewBag.CurrentStock = stock;
            }
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Adjust(FinishedGoodsAdjustmentRequest request)
    {
        if (!ModelState.IsValid)
        {
            var warehouses = await _serviceManager.WarehouseService.GetAllAsync();
            var fgWarehouses = warehouses.Where(w => w.Type == WarehouseType.FinishedGoods && w.IsActive).ToList();
            var products = await _serviceManager.ProductService.GetActiveProductsAsync();
            ViewBag.Warehouses = new SelectList(fgWarehouses, "Id", "Name", request.WarehouseId);
            ViewBag.Products = new SelectList(products, "Id", "Name", request.ProductId);
            return View(request);
        }

        try
        {
            var userId = GetCurrentUserId();
            await _serviceManager.FinishedGoodsService.AdjustStockAsync(request, userId);
            TempData["SuccessMessage"] = $"تم تسجيل التسوية الجردية لمخزون المنتج التام بنجاح (الكمية الفعلية: {request.ActualQuantity} {request.Unit})!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            var warehouses = await _serviceManager.WarehouseService.GetAllAsync();
            var fgWarehouses = warehouses.Where(w => w.Type == WarehouseType.FinishedGoods && w.IsActive).ToList();
            var products = await _serviceManager.ProductService.GetActiveProductsAsync();
            ViewBag.Warehouses = new SelectList(fgWarehouses, "Id", "Name", request.WarehouseId);
            ViewBag.Products = new SelectList(products, "Id", "Name", request.ProductId);
            return View(request);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Transfer(int? stockId)
    {
        var warehouses = await _serviceManager.WarehouseService.GetAllAsync();
        var fgWarehouses = warehouses.Where(w => w.Type == WarehouseType.FinishedGoods && w.IsActive).ToList();
        var products = await _serviceManager.ProductService.GetActiveProductsAsync();

        ViewBag.SourceWarehouses = new SelectList(fgWarehouses, "Id", "Name");
        ViewBag.DestinationWarehouses = new SelectList(fgWarehouses, "Id", "Name");
        ViewBag.Products = new SelectList(products, "Id", "Name");

        var model = new FinishedGoodsTransferRequest();

        if (stockId.HasValue && stockId.Value > 0)
        {
            var stock = await _serviceManager.FinishedGoodsService.GetStockByIdAsync(stockId.Value);
            if (stock != null)
            {
                model.SourceWarehouseId = stock.WarehouseId;
                model.SourceLocationId = stock.LocationId;
                model.ProductId = stock.ProductId;
                model.BatchNumber = stock.BatchNumber;
                model.Quantity = stock.Quantity;
                model.Unit = stock.Unit;
                ViewBag.CurrentStock = stock;
            }
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Transfer(FinishedGoodsTransferRequest request)
    {
        if (!ModelState.IsValid)
        {
            var warehouses = await _serviceManager.WarehouseService.GetAllAsync();
            var fgWarehouses = warehouses.Where(w => w.Type == WarehouseType.FinishedGoods && w.IsActive).ToList();
            var products = await _serviceManager.ProductService.GetActiveProductsAsync();
            ViewBag.SourceWarehouses = new SelectList(fgWarehouses, "Id", "Name", request.SourceWarehouseId);
            ViewBag.DestinationWarehouses = new SelectList(fgWarehouses, "Id", "Name", request.DestinationWarehouseId);
            ViewBag.Products = new SelectList(products, "Id", "Name", request.ProductId);
            return View(request);
        }

        try
        {
            var userId = GetCurrentUserId();
            await _serviceManager.FinishedGoodsService.TransferStockAsync(request, userId);
            TempData["SuccessMessage"] = $"تم نقل كمية ({request.Quantity} {request.Unit}) من المنتج التام بين مستودعات المنتجات التامة بنجاح!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            var warehouses = await _serviceManager.WarehouseService.GetAllAsync();
            var fgWarehouses = warehouses.Where(w => w.Type == WarehouseType.FinishedGoods && w.IsActive).ToList();
            var products = await _serviceManager.ProductService.GetActiveProductsAsync();
            ViewBag.SourceWarehouses = new SelectList(fgWarehouses, "Id", "Name", request.SourceWarehouseId);
            ViewBag.DestinationWarehouses = new SelectList(fgWarehouses, "Id", "Name", request.DestinationWarehouseId);
            ViewBag.Products = new SelectList(products, "Id", "Name", request.ProductId);
            return View(request);
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetLocationsByWarehouse(int warehouseId)
    {
        var locations = await _serviceManager.WarehouseLocationService.GetByWarehouseIdAsync(warehouseId);
        return Json(locations.Select(l => new { id = l.Id, name = l.Name, code = l.Code, section = l.Section }));
    }

    private int GetCurrentUserId()
    {
        var claim = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier);
        if (claim != null && int.TryParse(claim.Value, out int id))
        {
            return id;
        }
        return 1;
    }
}
