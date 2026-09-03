using FactoryX.Application.DTOs;
using FactoryX.Application.Services.Abstracts;
using FactoryX.Domain.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FactoryX.Web.Controllers;

[Authorize]
public class ProductionOrdersController : Controller
{
    private readonly IServiceManager _serviceManager;

    public ProductionOrdersController(IServiceManager serviceManager)
    {
        _serviceManager = serviceManager;
    }

    #region List & Dashboard

    [HttpGet]
    [Route("ProductionOrders")]
    [Route("ProductionOrders/Index")]
    [Route("WorkOrder")]
    [Route("WorkOrder/Index")]
    public async Task<IActionResult> Index([FromQuery] ProductionOrderFilterRequest filter)
    {
        var summary = await _serviceManager.WorkOrderService.GetProductionOrderSummaryAsync();
        var orders = await _serviceManager.WorkOrderService.GetProductionOrdersAsync(filter);

        ViewBag.Summary = summary;
        ViewBag.Filter = filter;
        await PopulateFilterDropdownsAsync(filter.ProductId, filter.Status, filter.Priority);

        return View(orders);
    }

    #endregion

    #region Create Workflow

    [HttpGet]
    [Route("ProductionOrders/Create")]
    [Route("WorkOrder/Create")]
    public async Task<IActionResult> Create([FromQuery] int? productId, [FromQuery] int? recipeVersionId)
    {
        var model = new CreateProductionOrderRequest
        {
            PlannedDate = DateTime.UtcNow.Date,
            DueDate = DateTime.UtcNow.Date.AddDays(3),
            PlannedQuantity = 100m,
            OutputUnit = "KG",
            Priority = ProductionOrderPriority.Normal,
            InitialStatus = ProductionOrderStatus.Draft
        };

        if (productId.HasValue && productId.Value > 0)
        {
            model.ProductId = productId.Value;
        }

        if (recipeVersionId.HasValue && recipeVersionId.Value > 0)
        {
            model.RecipeVersionId = recipeVersionId.Value;
        }

        await PopulateResourcesDropdownsAsync();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Route("ProductionOrders/Create")]
    [Route("WorkOrder/Create")]
    public async Task<IActionResult> Create(CreateProductionOrderRequest request)
    {
        try
        {
            var created = await _serviceManager.WorkOrderService.CreateProductionOrderAsync(request);
            TempData["Success"] = $"تم إنشاء أمر الإنتاج '{created.OrderNumber}' بنجاح وحفظه بحالة '{created.StatusName}'.";
            return RedirectToAction(nameof(Details), new { id = created.Id });
        }
        catch (ValidationException ex)
        {
            foreach (var error in ex.Errors)
            {
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }

        await PopulateResourcesDropdownsAsync(request.ProductId, request.ProductionAreaId, request.ProductionLineId, request.WorkCenterId, request.MachineId, request.OperatorId, request.ShiftId);
        return View(request);
    }

    #endregion

    #region Edit Workflow

    [HttpGet]
    [Route("ProductionOrders/Edit/{id}")]
    [Route("WorkOrder/Edit/{id}")]
    public async Task<IActionResult> Edit(int id)
    {
        var order = await _serviceManager.WorkOrderService.GetProductionOrderByIdAsync(id);
        if (order == null) return NotFound();

        if (order.OrderStatus != ProductionOrderStatus.Draft && order.OrderStatus != ProductionOrderStatus.Planned)
        {
            TempData["Error"] = $"لا يمكن تعديل أمر الإنتاج في حالة '{order.StatusName}'. التعديل متاح فقط للأوامر في حالة مسودة أو مخطط.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var model = new UpdateProductionOrderRequest
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            ProductId = order.ProductId,
            RecipeVersionId = order.RecipeVersionId ?? 0,
            PlannedQuantity = order.PlannedQuantity,
            OutputUnit = order.OutputUnit,
            PlannedDate = order.PlannedDate,
            DueDate = order.DueDate,
            Priority = order.Priority,
            ProductionAreaId = order.ProductionAreaId,
            ProductionLineId = order.ProductionLineId,
            WorkCenterId = order.WorkCenterId,
            MachineId = order.MachineId,
            OperatorId = order.OperatorId,
            ShiftId = order.ShiftId,
            Notes = order.Notes
        };

        await PopulateResourcesDropdownsAsync(order.ProductId, order.ProductionAreaId, order.ProductionLineId, order.WorkCenterId, order.MachineId, order.OperatorId, order.ShiftId);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Route("ProductionOrders/Edit/{id}")]
    [Route("WorkOrder/Edit/{id}")]
    public async Task<IActionResult> Edit(int id, UpdateProductionOrderRequest request)
    {
        if (id != request.Id) return BadRequest();

        try
        {
            var updated = await _serviceManager.WorkOrderService.UpdateProductionOrderAsync(request);
            TempData["Success"] = $"تم تحديث بيانات أمر الإنتاج '{updated.OrderNumber}' بنجاح.";
            return RedirectToAction(nameof(Details), new { id = updated.Id });
        }
        catch (ValidationException ex)
        {
            foreach (var error in ex.Errors)
            {
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }

        await PopulateResourcesDropdownsAsync(request.ProductId, request.ProductionAreaId, request.ProductionLineId, request.WorkCenterId, request.MachineId, request.OperatorId, request.ShiftId);
        return View(request);
    }

    #endregion

    #region Details & Planning Sheet

    [HttpGet]
    [Route("ProductionOrders/Details/{id}")]
    [Route("WorkOrder/Details/{id}")]
    public async Task<IActionResult> Details(int id)
    {
        var order = await _serviceManager.WorkOrderService.GetProductionOrderByIdAsync(id);
        if (order == null) return NotFound();

        return View(order);
    }

    #endregion

    #region Status Lifecycle Transitions

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Route("ProductionOrders/Release/{id}")]
    [Route("WorkOrder/Release/{id}")]
    public async Task<IActionResult> Release(int id)
    {
        try
        {
            var released = await _serviceManager.WorkOrderService.ReleaseProductionOrderAsync(id);
            TempData["Success"] = $"تم اعتماد وإطلاق أمر الإنتاج '{released.OrderNumber}' وتجميد لقطة احتياجات الخامات (BOM Snapshot) بنجاح.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Route("ProductionOrders/Start/{id}")]
    [Route("WorkOrder/Start/{id}")]
    public async Task<IActionResult> Start(int id)
    {
        try
        {
            var started = await _serviceManager.WorkOrderService.StartProductionOrderAsync(id);
            TempData["Success"] = $"تم بدء تشغيل أمر الإنتاج '{started.OrderNumber}' بنجاح.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Route("ProductionOrders/Complete/{id}")]
    [Route("WorkOrder/Complete/{id}")]
    public async Task<IActionResult> Complete(int id)
    {
        try
        {
            var completed = await _serviceManager.WorkOrderService.CompleteProductionOrderAsync(id);
            TempData["Success"] = $"تم إنهاء وإكمال أمر الإنتاج '{completed.OrderNumber}' بنجاح.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Route("ProductionOrders/Cancel/{id}")]
    [Route("WorkOrder/Cancel/{id}")]
    public async Task<IActionResult> Cancel(int id, [FromForm] string? cancellationReason)
    {
        try
        {
            var cancelled = await _serviceManager.WorkOrderService.CancelProductionOrderAsync(id, cancellationReason);
            TempData["Success"] = $"تم إلغاء أمر الإنتاج '{cancelled.OrderNumber}'.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Route("ProductionOrders/Delete/{id}")]
    [Route("WorkOrder/Delete/{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var deleted = await _serviceManager.WorkOrderService.DeleteProductionOrderAsync(id);
            if (deleted)
            {
                TempData["Success"] = "تم حذف مسودة أمر الإنتاج بنجاح.";
                return RedirectToAction(nameof(Index));
            }
            return NotFound();
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Details), new { id });
        }
    }

    #endregion

    #region AJAX / Live Simulation APIs

    [HttpGet]
    [Route("ProductionOrders/CalculateRequirements")]
    [Route("WorkOrder/CalculateRequirements")]
    public async Task<IActionResult> CalculateRequirements(int recipeVersionId, decimal plannedQuantity)
    {
        try
        {
            var requirements = await _serviceManager.ProductionPlanningService.CalculateMaterialRequirementsAsync(recipeVersionId, plannedQuantity);
            return Json(new { success = true, items = requirements });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpGet]
    [Route("ProductionOrders/GetActiveVersions")]
    [Route("WorkOrder/GetActiveVersions")]
    public async Task<IActionResult> GetActiveVersions(int productId, DateTime? plannedDate)
    {
        var targetDate = plannedDate ?? DateTime.UtcNow.Date;
        var versions = await _serviceManager.ProductionPlanningService.GetActiveRecipeVersionsForProductAsync(productId, targetDate);

        var list = versions.Select(v => new
        {
            id = v.Id,
            versionNumber = v.VersionNumber,
            versionName = v.VersionName,
            expectedOutput = v.ExpectedOutput,
            outputUnit = v.OutputUnit,
            effectiveFrom = v.EffectiveFrom.ToString("yyyy-MM-dd"),
            effectiveTo = v.EffectiveTo?.ToString("yyyy-MM-dd") ?? "مستمر",
            displayText = $"{v.VersionNumber} - {(string.IsNullOrWhiteSpace(v.VersionName) ? "إصدار قياسي" : v.VersionName)} ({v.ExpectedOutput} {v.OutputUnit})"
        });

        return Json(list);
    }

    #endregion

    #region Helpers & Dropdown Population

    private async Task PopulateFilterDropdownsAsync(int? selectedProduct, ProductionOrderStatus? selectedStatus, ProductionOrderPriority? selectedPriority)
    {
        var products = await _serviceManager.ProductService.GetActiveProductsAsync();
        ViewBag.Products = new SelectList(products, "Id", "Name", selectedProduct);

        ViewBag.Statuses = Enum.GetValues<ProductionOrderStatus>().Select(s => new SelectListItem
        {
            Value = ((int)s).ToString(),
            Text = s switch
            {
                ProductionOrderStatus.Draft => "مسودة (Draft)",
                ProductionOrderStatus.Planned => "مخطط (Planned)",
                ProductionOrderStatus.Released => "مطلق وجاهز (Released)",
                ProductionOrderStatus.InProgress => "قيد الإنتاج (In Progress)",
                ProductionOrderStatus.Completed => "مكتمل (Completed)",
                ProductionOrderStatus.Cancelled => "ملغي (Cancelled)",
                _ => s.ToString()
            },
            Selected = selectedStatus.HasValue && selectedStatus.Value == s
        }).ToList();

        ViewBag.Priorities = Enum.GetValues<ProductionOrderPriority>().Select(p => new SelectListItem
        {
            Value = ((int)p).ToString(),
            Text = p switch
            {
                ProductionOrderPriority.Low => "منخفضة (Low)",
                ProductionOrderPriority.Normal => "عادية (Normal)",
                ProductionOrderPriority.High => "مرتفعة (High)",
                ProductionOrderPriority.Urgent => "طارئة / عاجلة (Urgent)",
                _ => p.ToString()
            },
            Selected = selectedPriority.HasValue && selectedPriority.Value == p
        }).ToList();
    }

    private async Task PopulateResourcesDropdownsAsync(
        int? selectedProduct = null,
        int? selectedArea = null,
        int? selectedLine = null,
        int? selectedWorkCenter = null,
        int? selectedMachine = null,
        int? selectedOperator = null,
        int? selectedShift = null)
    {
        var products = await _serviceManager.ProductService.GetActiveProductsAsync();
        var machines = await _serviceManager.MachineService.GetAllAsync();
        var operators = await _serviceManager.OperatorService.GetAllAsync();
        var shifts = await _serviceManager.ShiftService.GetAllAsync();

        ViewBag.Products = new SelectList(products, "Id", "Name", selectedProduct);
        ViewBag.Machines = new SelectList(machines, "Id", "Name", selectedMachine);
        ViewBag.Operators = new SelectList(operators, "Id", "Name", selectedOperator);
        ViewBag.Shifts = new SelectList(shifts, "Id", "Name", selectedShift);

        ViewBag.Priorities = Enum.GetValues<ProductionOrderPriority>().Select(p => new SelectListItem
        {
            Value = ((int)p).ToString(),
            Text = p switch
            {
                ProductionOrderPriority.Low => "منخفضة (Low)",
                ProductionOrderPriority.Normal => "عادية (Normal)",
                ProductionOrderPriority.High => "مرتفعة (High)",
                ProductionOrderPriority.Urgent => "طارئة / عاجلة (Urgent)",
                _ => p.ToString()
            }
        }).ToList();
    }

    #endregion
}
