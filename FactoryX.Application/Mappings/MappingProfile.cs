using AutoMapper;
using FactoryX.Application.DTOs;
using FactoryX.Application.DTOs.Requests.MachineRequests;
using FactoryX.Application.DTOs.Requests.OperatorRequests;
using FactoryX.Application.DTOs.Requests.ProductionRecordRequests;
using FactoryX.Application.DTOs.Requests.ProductRequests;
using FactoryX.Application.DTOs.Requests.ShiftRequests;
using FactoryX.Application.DTOs.Requests.WorkOrderRequests;
using FactoryX.Application.DTOs.Responses.MachineResponses;
using FactoryX.Application.DTOs.Responses.Operator;
using FactoryX.Application.DTOs.Responses.OperatorResponses;
using FactoryX.Application.DTOs.Responses.Product;
using FactoryX.Application.DTOs.Responses.ProductionRecord;
using FactoryX.Application.DTOs.Responses.ProductResponses;
using FactoryX.Application.DTOs.Responses.Shift;
using FactoryX.Application.DTOs.Responses.ShiftResponses;
using FactoryX.Application.DTOs.Responses.UserManagementResponses;
using FactoryX.Application.DTOs.Responses.WorkOrder;
using FactoryX.Application.DTOs.Responses.WorkOrderResponses;
using FactoryX.Domain.Entities;

namespace FactoryX.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Machine, MachineDto>().ReverseMap();
        CreateMap<Operator, OperatorDto>().ReverseMap();
        CreateMap<WorkOrder, WorkOrderDto>().ReverseMap();
        CreateMap<Shift, ShiftDto>().ReverseMap();
        CreateMap<Downtime, DowntimeDto>().ReverseMap();
        CreateMap<User, UserDto>().ReverseMap();
        CreateMap<Material, MaterialDto>().ReverseMap();
        CreateMap<Product, ProductDto>().ReverseMap();
        CreateMap<MaterialUsage, MaterialUsageDto>().ReverseMap();
        CreateMap<ProductionRecord, ProductionRecordDto>().ReverseMap();

		#region Machine Mapping
		CreateMap<Machine, GetAllMachinesResponse>().ReverseMap();
		CreateMap<Machine, GetMachineResponse?>().ReverseMap();
        CreateMap<Machine, InsertMachineResponse>().ReverseMap();
        CreateMap<Machine, InsertMachineRequest>().ReverseMap();
        CreateMap<Machine, UpdateMachineRequest>().ReverseMap();
        CreateMap<UpdateMachineRequest, GetMachineResponse>().ReverseMap();
        CreateMap<DeleteMachineRequest, GetMachineResponse>().ReverseMap();
		#endregion

		#region Operator Mapping
		CreateMap<Operator, GetAllOperatorResponse>().ReverseMap();
        CreateMap<Operator, GetOperatorResponse?>().ReverseMap();
        CreateMap<Operator, InsertOperatorRequest>().ReverseMap();
        CreateMap<Operator, InsertOperatorResponse>().ReverseMap();
        CreateMap<Operator, UpdateOperatorRequest>().ReverseMap();
        CreateMap<UpdateOperatorRequest, GetOperatorResponse>().ReverseMap();
        CreateMap<DeleteOperatorRequest, GetOperatorResponse>().ReverseMap();
		#endregion

		#region ProductionRecord Mapping
		CreateMap<ProductionRecord, InsertProductionRecordRequest>().ReverseMap();
        CreateMap<ProductionRecord, InsertProductionRecordResponse>().ReverseMap();
        CreateMap<ProductionRecord, UpdateProductionRecordRequest>().ReverseMap();
        #endregion

        #region Product Mapping
        CreateMap<Product, GetAllProductResponse>().ReverseMap();
        CreateMap<Product, GetProductResponse>().ReverseMap();
        CreateMap<Product, InsertProductRequest>().ReverseMap();
        CreateMap<Product, InsertProductResponse>().ReverseMap();
        CreateMap<Product, FactoryX.Application.DTOs.Requests.ProductRequests.UpdateProductRequest>().ReverseMap();
		CreateMap<FactoryX.Application.DTOs.Requests.ProductRequests.UpdateProductRequest, GetProductResponse>().ReverseMap();
		CreateMap<DeleteProductRequest, GetProductResponse>().ReverseMap();

		#endregion

		#region Shift Mapping
		CreateMap<Shift, GetAllShiftResponse>().ReverseMap();
        CreateMap<Shift, GetShiftResponse>().ReverseMap();
        CreateMap<Shift, InsertShiftRequest>().ReverseMap();
		CreateMap<Shift, InsertShiftResponse>().ReverseMap();
		CreateMap<Shift, UpdateShiftRequest>().ReverseMap();
		CreateMap<UpdateShiftRequest, GetShiftResponse>().ReverseMap();
		CreateMap<DeleteShiftRequest, GetShiftResponse>().ReverseMap();
		#endregion

		#region User Mapping
		CreateMap<GetUserProfileResponse, UserProfileDto>().ReverseMap();
		#endregion

		#region WorkOrder Mapping
		CreateMap<WorkOrder, InsertWorkOrderRequest>().ReverseMap();
		CreateMap<WorkOrder, InsertWorkOrderResponse>().ReverseMap();
        CreateMap<WorkOrder, UpdateWorkOrderRequest>().ReverseMap();
		CreateMap<UpdateWorkOrderRequest, GetWorkOrderResponse>().ReverseMap();
		CreateMap<DeleteWorkOrderRequest, GetWorkOrderResponse>().ReverseMap();
		#endregion

		#region Phase 3 Inventory Mappings
		CreateMap<Warehouse, WarehouseDto>()
			.ForMember(dest => dest.LocationCount, opt => opt.MapFrom(src => src.Locations != null ? src.Locations.Count : 0));
		CreateMap<CreateWarehouseRequest, Warehouse>();
		CreateMap<UpdateWarehouseRequest, Warehouse>();

		CreateMap<WarehouseLocation, WarehouseLocationDto>()
			.ForMember(dest => dest.WarehouseName, opt => opt.MapFrom(src => src.Warehouse != null ? src.Warehouse.Name : string.Empty));
		CreateMap<CreateWarehouseLocationRequest, WarehouseLocation>();
		CreateMap<UpdateWarehouseLocationRequest, WarehouseLocation>();

		CreateMap<StockBalance, StockBalanceDto>()
			.ForMember(dest => dest.WarehouseName, opt => opt.MapFrom(src => src.Warehouse != null ? src.Warehouse.Name : string.Empty))
			.ForMember(dest => dest.LocationName, opt => opt.MapFrom(src => src.Location != null ? src.Location.Name : string.Empty))
			.ForMember(dest => dest.MaterialName, opt => opt.MapFrom(src => src.Material != null ? src.Material.Name : string.Empty))
			.ForMember(dest => dest.MaterialCode, opt => opt.MapFrom(src => src.Material != null ? src.Material.Code : string.Empty))
			.ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty))
			.ForMember(dest => dest.ProductCode, opt => opt.MapFrom(src => src.Product != null ? src.Product.Code : string.Empty));

		CreateMap<InventoryTransaction, InventoryTransactionDto>()
			.ForMember(dest => dest.WarehouseName, opt => opt.MapFrom(src => src.Warehouse != null ? src.Warehouse.Name : string.Empty))
			.ForMember(dest => dest.SourceLocationName, opt => opt.MapFrom(src => src.SourceLocation != null ? src.SourceLocation.Name : string.Empty))
			.ForMember(dest => dest.DestinationLocationName, opt => opt.MapFrom(src => src.DestinationLocation != null ? src.DestinationLocation.Name : string.Empty))
			.ForMember(dest => dest.MaterialName, opt => opt.MapFrom(src => src.Material != null ? src.Material.Name : string.Empty))
			.ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty))
			.ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.Username : string.Empty));
		#endregion

		#region Phase 4 Raw Material Mappings
		CreateMap<MaterialCategory, MaterialCategoryDto>()
			.ForMember(dest => dest.MaterialCount, opt => opt.MapFrom(src => src.Materials != null ? src.Materials.Count : 0));
		CreateMap<CreateMaterialCategoryRequest, MaterialCategory>();
		CreateMap<UpdateMaterialCategoryRequest, MaterialCategory>();

		CreateMap<Material, MaterialDto>()
			.ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.MaterialCategory != null ? src.MaterialCategory.Name : string.Empty))
			.ForMember(dest => dest.WarehouseName, opt => opt.MapFrom(src => src.Warehouse != null ? src.Warehouse.Name : string.Empty))
			.ForMember(dest => dest.StockStatus, opt => opt.MapFrom(src => 
				src.CurrentStock <= 0 ? MaterialStockStatus.OUT_OF_STOCK :
				src.CurrentStock < src.MinimumStock ? MaterialStockStatus.LOW_STOCK :
				src.CurrentStock <= src.ReorderLevel ? MaterialStockStatus.REORDER_REQUIRED :
				src.CurrentStock <= src.MaximumStock ? MaterialStockStatus.NORMAL :
				MaterialStockStatus.OVERSTOCKED))
			.ForMember(dest => dest.StockStatusName, opt => opt.MapFrom(src => 
				src.CurrentStock <= 0 ? "نفد المخزون (Out of Stock)" :
				src.CurrentStock < src.MinimumStock ? "مخزون حرج / منخفض (Low Stock)" :
				src.CurrentStock <= src.ReorderLevel ? "يتطلب إعادة الطلب (Reorder Required)" :
				src.CurrentStock <= src.MaximumStock ? "مخزون طبيعي (Normal)" :
				"مخزون زائد (Overstocked)"))
			.ForMember(dest => dest.IsExpired, opt => opt.MapFrom(src => 
				src.ExpiryDate.HasValue && src.ExpiryDate.Value.Date < DateTime.UtcNow.Date))
			.ForMember(dest => dest.IsExpiringSoon, opt => opt.MapFrom(src => 
				src.ExpiryDate.HasValue && src.ExpiryDate.Value.Date >= DateTime.UtcNow.Date && src.ExpiryDate.Value.Date <= DateTime.UtcNow.Date.AddDays(30)));

		CreateMap<CreateMaterialRequest, Material>();
		CreateMap<UpdateMaterialRequest, Material>();
		#endregion

		#region Phase 5 Finished Products Mappings
		CreateMap<ProductCategory, ProductCategoryDto>()
			.ForMember(dest => dest.ProductCount, opt => opt.MapFrom(src => src.Products != null ? src.Products.Count : 0));
		CreateMap<CreateProductCategoryRequest, ProductCategory>();
		CreateMap<UpdateProductCategoryRequest, ProductCategory>();

		CreateMap<Product, ProductDto>()
			.ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.ProductCategory != null ? src.ProductCategory.Name : string.Empty))
			.ForMember(dest => dest.WorkOrderCount, opt => opt.MapFrom(src => src.WorkOrders != null ? src.WorkOrders.Count : 0));
		CreateMap<CreateProductRequest, Product>();
		CreateMap<FactoryX.Application.DTOs.UpdateProductRequest, Product>();
		#endregion

		#region Phase 6 Recipes and BOM Mappings
		CreateMap<Recipe, RecipeDto>()
			.ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty))
			.ForMember(dest => dest.ProductCode, opt => opt.MapFrom(src => src.Product != null ? src.Product.Code : string.Empty))
			.ForMember(dest => dest.ProductSKU, opt => opt.MapFrom(src => src.Product != null ? src.Product.SKU : string.Empty))
			.ForMember(dest => dest.VersionCount, opt => opt.MapFrom(src => src.Versions != null ? src.Versions.Count : 0));
		CreateMap<CreateRecipeRequest, Recipe>();
		CreateMap<UpdateRecipeRequest, Recipe>();

		CreateMap<RecipeVersion, RecipeVersionDto>()
			.ForMember(dest => dest.RecipeCode, opt => opt.MapFrom(src => src.Recipe != null ? src.Recipe.Code : string.Empty))
			.ForMember(dest => dest.RecipeName, opt => opt.MapFrom(src => src.Recipe != null ? src.Recipe.Name : string.Empty))
			.ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => (src.Recipe != null && src.Recipe.Product != null) ? src.Recipe.Product.Name : string.Empty));
		CreateMap<CreateRecipeVersionRequest, RecipeVersion>();
		CreateMap<UpdateRecipeVersionRequest, RecipeVersion>();

		CreateMap<RecipeItem, RecipeItemDto>()
			.ForMember(dest => dest.MaterialCode, opt => opt.MapFrom(src => src.Material != null ? src.Material.Code : string.Empty))
			.ForMember(dest => dest.MaterialName, opt => opt.MapFrom(src => src.Material != null ? src.Material.Name : string.Empty))
			.ForMember(dest => dest.MaterialArabicName, opt => opt.MapFrom(src => src.Material != null ? src.Material.ArabicName : string.Empty));
		CreateMap<RecipeItemRequest, RecipeItem>();
		#endregion

		#region Phase 7 Production Planning Mappings
		CreateMap<WorkOrder, ProductionOrderDto>()
			.ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty))
			.ForMember(dest => dest.ProductArabicName, opt => opt.MapFrom(src => src.Product != null ? src.Product.ArabicName : null))
			.ForMember(dest => dest.ProductCode, opt => opt.MapFrom(src => src.Product != null ? src.Product.Code : string.Empty))
			.ForMember(dest => dest.ProductSKU, opt => opt.MapFrom(src => src.Product != null ? src.Product.SKU : string.Empty))
			.ForMember(dest => dest.RecipeCode, opt => opt.MapFrom(src => (src.RecipeVersion != null && src.RecipeVersion.Recipe != null) ? src.RecipeVersion.Recipe.Code : (src.Recipe != null ? src.Recipe.Code : null)))
			.ForMember(dest => dest.RecipeName, opt => opt.MapFrom(src => (src.RecipeVersion != null && src.RecipeVersion.Recipe != null) ? src.RecipeVersion.Recipe.Name : (src.Recipe != null ? src.Recipe.Name : null)))
			.ForMember(dest => dest.RecipeVersionNumber, opt => opt.MapFrom(src => src.RecipeVersion != null ? src.RecipeVersion.VersionNumber : null))
			.ForMember(dest => dest.RecipeVersionName, opt => opt.MapFrom(src => src.RecipeVersion != null ? src.RecipeVersion.VersionName : null))
			.ForMember(dest => dest.RecipeExpectedOutput, opt => opt.MapFrom(src => src.RecipeVersion != null ? src.RecipeVersion.ExpectedOutput : 0m))
			.ForMember(dest => dest.RecipeOutputUnit, opt => opt.MapFrom(src => src.RecipeVersion != null ? src.RecipeVersion.OutputUnit : "KG"))
			.ForMember(dest => dest.ProductionAreaName, opt => opt.MapFrom(src => src.ProductionArea != null ? src.ProductionArea.Name : null))
			.ForMember(dest => dest.ProductionLineName, opt => opt.MapFrom(src => src.ProductionLine != null ? src.ProductionLine.Name : null))
			.ForMember(dest => dest.WorkCenterName, opt => opt.MapFrom(src => src.WorkCenter != null ? src.WorkCenter.Name : null))
			.ForMember(dest => dest.MachineName, opt => opt.MapFrom(src => src.Machine != null ? src.Machine.Name : null))
			.ForMember(dest => dest.OperatorName, opt => opt.MapFrom(src => src.Operator != null ? src.Operator.Name : null))
			.ForMember(dest => dest.ShiftName, opt => opt.MapFrom(src => src.Shift != null ? src.Shift.Name : null))
			.ForMember(dest => dest.ActualQuantity, opt => opt.MapFrom(src => src.ActualQuantityDecimal))
			.ForMember(dest => dest.MaterialRequirements, opt => opt.MapFrom(src => src.MaterialRequirements ?? new List<WorkOrderMaterialRequirement>()));

		CreateMap<WorkOrderMaterialRequirement, WorkOrderMaterialRequirementDto>();
		CreateMap<CreateProductionOrderRequest, WorkOrder>();
		CreateMap<UpdateProductionOrderRequest, WorkOrder>();
		#endregion

		#region Phase 8 Production Batch and Execution Mappings
		CreateMap<ProductionBatch, ProductionBatchDto>()
			.ForMember(dest => dest.WorkOrderNumber, opt => opt.MapFrom(src => src.WorkOrder != null ? src.WorkOrder.OrderNumber : string.Empty))
			.ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty))
			.ForMember(dest => dest.ProductArabicName, opt => opt.MapFrom(src => src.Product != null ? src.Product.ArabicName : null))
			.ForMember(dest => dest.ProductCode, opt => opt.MapFrom(src => src.Product != null ? src.Product.Code : string.Empty))
			.ForMember(dest => dest.ProductSKU, opt => opt.MapFrom(src => src.Product != null ? src.Product.SKU : string.Empty))
			.ForMember(dest => dest.RecipeVersionNumber, opt => opt.MapFrom(src => src.RecipeVersion != null ? src.RecipeVersion.VersionNumber : null))
			.ForMember(dest => dest.RecipeVersionName, opt => opt.MapFrom(src => src.RecipeVersion != null ? src.RecipeVersion.VersionName : null))
			.ForMember(dest => dest.ProductionLineName, opt => opt.MapFrom(src => src.ProductionLine != null ? src.ProductionLine.Name : null))
			.ForMember(dest => dest.WorkCenterName, opt => opt.MapFrom(src => src.WorkCenter != null ? src.WorkCenter.Name : null))
			.ForMember(dest => dest.MachineName, opt => opt.MapFrom(src => src.Machine != null ? src.Machine.Name : null))
			.ForMember(dest => dest.OperatorName, opt => opt.MapFrom(src => src.Operator != null ? src.Operator.Name : null))
			.ForMember(dest => dest.ShiftName, opt => opt.MapFrom(src => src.Shift != null ? src.Shift.Name : null))
			.ForMember(dest => dest.TargetWarehouseName, opt => opt.MapFrom(src => src.TargetWarehouse != null ? src.TargetWarehouse.Name : null))
			.ForMember(dest => dest.Consumptions, opt => opt.MapFrom(src => src.Consumptions ?? new List<ProductionConsumption>()));

		CreateMap<ProductionConsumption, ProductionConsumptionDto>()
			.ForMember(dest => dest.MaterialCode, opt => opt.MapFrom(src => src.Material != null ? src.Material.Code : string.Empty))
			.ForMember(dest => dest.MaterialName, opt => opt.MapFrom(src => src.Material != null ? src.Material.Name : string.Empty))
			.ForMember(dest => dest.MaterialArabicName, opt => opt.MapFrom(src => src.Material != null ? src.Material.ArabicName : null))
			.ForMember(dest => dest.WarehouseName, opt => opt.MapFrom(src => src.Warehouse != null ? src.Warehouse.Name : null))
			.ForMember(dest => dest.LocationName, opt => opt.MapFrom(src => src.Location != null ? src.Location.Name : null));

		CreateMap<CreateProductionBatchRequest, ProductionBatch>();
		#endregion

		#region Phase 9 Waste and Rejection Mappings
		CreateMap<WasteReason, WasteReasonDto>()
			.ForMember(dest => dest.WastesCount, opt => opt.MapFrom(src => src.Wastes != null ? src.Wastes.Count : 0));
		CreateMap<CreateWasteReasonRequest, WasteReason>();
		CreateMap<UpdateWasteReasonRequest, WasteReason>();

		CreateMap<Waste, WasteDto>()
			.ForMember(dest => dest.WasteTypeName, opt => opt.MapFrom(src =>
				src.WasteType == WasteType.RawMaterialWaste ? "هالك مواد خام (Raw Material)" :
				src.WasteType == WasteType.ProductionProcessWaste ? "هالك مراحل التشغيل (Process Waste)" :
				"مرفوضات الإنتاج التام (Output Rejection)"))
			.ForMember(dest => dest.StatusName, opt => opt.MapFrom(src =>
				src.Status == WasteStatus.Draft ? "مسودة (Draft)" :
				src.Status == WasteStatus.PendingApproval ? "قيد المراجعة والاعتماد (Pending Approval)" :
				src.Status == WasteStatus.Approved ? "معتمد (Approved)" :
				src.Status == WasteStatus.Rejected ? "مرفوض (Rejected)" :
				"ملغى (Cancelled)"))
			.ForMember(dest => dest.ProductionBatchNumber, opt => opt.MapFrom(src => src.ProductionBatch != null ? src.ProductionBatch.BatchNumber : null))
			.ForMember(dest => dest.WorkOrderNumber, opt => opt.MapFrom(src => src.WorkOrder != null ? src.WorkOrder.OrderNumber : (src.ProductionBatch != null && src.ProductionBatch.WorkOrder != null ? src.ProductionBatch.WorkOrder.OrderNumber : null)))
			.ForMember(dest => dest.MaterialName, opt => opt.MapFrom(src => src.Material != null ? src.Material.Name : null))
			.ForMember(dest => dest.MaterialCode, opt => opt.MapFrom(src => src.Material != null ? src.Material.Code : null))
			.ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : null))
			.ForMember(dest => dest.ProductCode, opt => opt.MapFrom(src => src.Product != null ? src.Product.Code : null))
			.ForMember(dest => dest.WarehouseName, opt => opt.MapFrom(src => src.Warehouse != null ? src.Warehouse.Name : null))
			.ForMember(dest => dest.LocationName, opt => opt.MapFrom(src => src.Location != null ? src.Location.Name : null))
			.ForMember(dest => dest.WasteReasonCode, opt => opt.MapFrom(src => src.WasteReason != null ? src.WasteReason.Code : null))
			.ForMember(dest => dest.WasteReasonName, opt => opt.MapFrom(src => src.WasteReason != null ? src.WasteReason.Reason : null))
			.ForMember(dest => dest.CreatedByUserName, opt => opt.MapFrom(src => src.CreatedByUser != null ? src.CreatedByUser.FullName ?? src.CreatedByUser.Username : null))
			.ForMember(dest => dest.ApprovedByUserName, opt => opt.MapFrom(src => src.ApprovedByUser != null ? src.ApprovedByUser.FullName ?? src.ApprovedByUser.Username : null))
			.ForMember(dest => dest.InventoryTransactionRef, opt => opt.MapFrom(src => src.InventoryTransaction != null ? src.InventoryTransaction.ReferenceDocumentNumber : null));

		CreateMap<CreateWasteRequest, Waste>();
		CreateMap<UpdateWasteRequest, Waste>();
		#endregion

		#region Phase 10 Quality Control Mappings
		CreateMap<QualityTemplate, QualityTemplateDto>()
			.ForMember(dest => dest.ProductCategoryName, opt => opt.MapFrom(src => src.ProductCategory != null ? src.ProductCategory.Name : null))
			.ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : null))
			.ForMember(dest => dest.ProductCode, opt => opt.MapFrom(src => src.Product != null ? src.Product.Code : null))
			.ForMember(dest => dest.ItemsCount, opt => opt.MapFrom(src => src.Items != null ? src.Items.Count : 0))
			.ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items != null ? src.Items.OrderBy(i => i.Sequence).ToList() : new List<QualityTemplateItem>()));

		CreateMap<QualityTemplateItem, QualityTemplateItemDto>()
			.ForMember(dest => dest.DataTypeName, opt => opt.MapFrom(src =>
				src.DataType == InspectionDataType.Text ? "نصي (Text)" :
				src.DataType == InspectionDataType.Number ? "رقمي (Number)" :
				src.DataType == InspectionDataType.Boolean ? "منطقي (Boolean)" :
				"مطابق / غير مطابق (Pass/Fail)"));

		CreateMap<QualityInspection, QualityInspectionDto>()
			.ForMember(dest => dest.TypeName, opt => opt.MapFrom(src =>
				src.Type == QualityInspectionType.RawMaterial ? "فحص خامات واردة (Raw Material)" :
				src.Type == QualityInspectionType.InProcess ? "فحص أثناء التشغيل (In-Process)" :
				"فحص دفعة إنتاج تام (Production Batch)"))
			.ForMember(dest => dest.StatusName, opt => opt.MapFrom(src =>
				src.Status == QualityInspectionStatus.Draft ? "مسودة (Draft)" :
				src.Status == QualityInspectionStatus.Pending ? "معلق للاعتماد (Pending)" :
				src.Status == QualityInspectionStatus.InProgress ? "قيد التنفيذ (In Progress)" :
				src.Status == QualityInspectionStatus.Approved ? "معتمد (Approved)" :
				src.Status == QualityInspectionStatus.Rejected ? "مرفوض (Rejected)" :
				src.Status == QualityInspectionStatus.Hold ? "محجوز / معلق (Hold)" :
				"ملغى (Cancelled)"))
			.ForMember(dest => dest.FinalDecisionName, opt => opt.MapFrom(src =>
				src.FinalDecision == QualityDecision.Approved ? "مطابق ومعتمد (Approved)" :
				src.FinalDecision == QualityDecision.Rejected ? "غير مطابق ومرفوض (Rejected)" :
				src.FinalDecision == QualityDecision.Hold ? "محجوز للفحص الإضافي (Hold)" :
				"لم يحدد بعد (Pending)"))
			.ForMember(dest => dest.RecommendedDecisionName, opt => opt.MapFrom(src =>
				src.RecommendedDecision == QualityDecision.Approved ? "مطابق ومعتمد (Approved)" :
				src.RecommendedDecision == QualityDecision.Rejected ? "غير مطابق ومرفوض (Rejected)" :
				src.RecommendedDecision == QualityDecision.Hold ? "محجوز للفحص الإضافي (Hold)" :
				"غير مكتمل (Pending)"))
			.ForMember(dest => dest.ProductionBatchNumber, opt => opt.MapFrom(src => src.ProductionBatch != null ? src.ProductionBatch.BatchNumber : null))
			.ForMember(dest => dest.BatchPlannedQuantity, opt => opt.MapFrom(src => src.ProductionBatch != null ? src.ProductionBatch.PlannedQuantity : 0m))
			.ForMember(dest => dest.BatchActualQuantity, opt => opt.MapFrom(src => src.ProductionBatch != null ? src.ProductionBatch.ActualOutputQuantity : 0m))
			.ForMember(dest => dest.BatchOutputUnit, opt => opt.MapFrom(src => src.ProductionBatch != null ? src.ProductionBatch.OutputUnit : "KG"))
			.ForMember(dest => dest.WorkOrderNumber, opt => opt.MapFrom(src => src.WorkOrder != null ? src.WorkOrder.OrderNumber : (src.ProductionBatch != null && src.ProductionBatch.WorkOrder != null ? src.ProductionBatch.WorkOrder.OrderNumber : null)))
			.ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : (src.ProductionBatch != null && src.ProductionBatch.Product != null ? src.ProductionBatch.Product.Name : null)))
			.ForMember(dest => dest.ProductCode, opt => opt.MapFrom(src => src.Product != null ? src.Product.Code : (src.ProductionBatch != null && src.ProductionBatch.Product != null ? src.ProductionBatch.Product.Code : null)))
			.ForMember(dest => dest.QualityTemplateCode, opt => opt.MapFrom(src => src.QualityTemplate != null ? src.QualityTemplate.Code : null))
			.ForMember(dest => dest.QualityTemplateName, opt => opt.MapFrom(src => src.QualityTemplate != null ? src.QualityTemplate.Name : null))
			.ForMember(dest => dest.InspectorName, opt => opt.MapFrom(src => src.Inspector != null ? src.Inspector.FullName ?? src.Inspector.Username : null))
			.ForMember(dest => dest.CreatedByUserName, opt => opt.MapFrom(src => src.CreatedByUser != null ? src.CreatedByUser.FullName ?? src.CreatedByUser.Username : null))
			.ForMember(dest => dest.SubmittedByUserName, opt => opt.MapFrom(src => src.SubmittedByUser != null ? src.SubmittedByUser.FullName ?? src.SubmittedByUser.Username : null))
			.ForMember(dest => dest.CompletedByUserName, opt => opt.MapFrom(src => src.CompletedByUser != null ? src.CompletedByUser.FullName ?? src.CompletedByUser.Username : null))
			.ForMember(dest => dest.DecisionByUserName, opt => opt.MapFrom(src => src.DecisionByUser != null ? src.DecisionByUser.FullName ?? src.DecisionByUser.Username : null))
			.ForMember(dest => dest.PreviousInspectionNumber, opt => opt.MapFrom(src => src.PreviousInspection != null ? src.PreviousInspection.InspectionNumber : null))
			.ForMember(dest => dest.ReinspectionsCount, opt => opt.MapFrom(src => src.Reinspections != null ? src.Reinspections.Count : 0))
			.ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items != null ? src.Items.OrderBy(i => i.Sequence).ToList() : new List<QualityInspectionItem>()));

		CreateMap<QualityInspectionItem, QualityInspectionItemDto>()
			.ForMember(dest => dest.DataTypeName, opt => opt.MapFrom(src =>
				src.DataType == InspectionDataType.Text ? "نصي" :
				src.DataType == InspectionDataType.Number ? "رقمي" :
				src.DataType == InspectionDataType.Boolean ? "منطقي" :
				"Pass/Fail"))
			.ForMember(dest => dest.ResultName, opt => opt.MapFrom(src =>
				src.Result == ItemEvaluationResult.Pass ? "مطابق (PASS)" :
				src.Result == ItemEvaluationResult.Fail ? "راسب (FAIL)" :
				src.Result == ItemEvaluationResult.Warning ? "تحذير (WARNING)" :
				"معلق (PENDING)"));
		#endregion

		#region Packaging Mapping
		CreateMap<PackagingBOM, PackagingBOMDto>()
			.ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty))
			.ForMember(dest => dest.ProductCode, opt => opt.MapFrom(src => src.Product != null ? src.Product.Code : string.Empty));

		CreateMap<PackagingBOMVersion, PackagingBOMVersionDto>();
		CreateMap<PackagingItem, PackagingItemDto>()
			.ForMember(dest => dest.MaterialName, opt => opt.MapFrom(src => src.Material != null ? src.Material.Name : string.Empty))
			.ForMember(dest => dest.MaterialCode, opt => opt.MapFrom(src => src.Material != null ? src.Material.Code : string.Empty))
			.ForMember(dest => dest.MaterialArabicName, opt => opt.MapFrom(src => src.Material != null ? src.Material.ArabicName : string.Empty))
			.ForMember(dest => dest.MaterialUnitCost, opt => opt.MapFrom(src => src.Material != null ? (src.Material.CurrentCost > 0 ? src.Material.CurrentCost : (src.Material.StandardCost > 0 ? src.Material.StandardCost : src.Material.UnitCost)) : 0m));

		CreateMap<PackagingOrder, PackagingOrderDto>()
			.ForMember(dest => dest.BatchNumber, opt => opt.MapFrom(src => src.ProductionBatch != null ? src.ProductionBatch.BatchNumber : string.Empty))
			.ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty))
			.ForMember(dest => dest.ProductCode, opt => opt.MapFrom(src => src.Product != null ? src.Product.Code : string.Empty))
			.ForMember(dest => dest.PackagingBOMName, opt => opt.MapFrom(src => src.PackagingBOM != null ? src.PackagingBOM.Name : string.Empty))
			.ForMember(dest => dest.PackagingBOMCode, opt => opt.MapFrom(src => src.PackagingBOM != null ? src.PackagingBOM.Code : string.Empty))
			.ForMember(dest => dest.OperatorName, opt => opt.MapFrom(src => src.Operator != null ? src.Operator.Name : string.Empty))
			.ForMember(dest => dest.VersionNumber, opt => opt.MapFrom(src => src.PackagingBOMVersion != null ? src.PackagingBOMVersion.VersionNumber : 1));

		CreateMap<PackagingConsumption, PackagingConsumptionDto>()
			.ForMember(dest => dest.MaterialName, opt => opt.MapFrom(src => src.Material != null ? src.Material.Name : string.Empty))
			.ForMember(dest => dest.MaterialCode, opt => opt.MapFrom(src => src.Material != null ? src.Material.Code : string.Empty))
			.ForMember(dest => dest.WarehouseName, opt => opt.MapFrom(src => src.Warehouse != null ? src.Warehouse.Name : string.Empty))
			.ForMember(dest => dest.LocationName, opt => opt.MapFrom(src => src.Location != null ? src.Location.Name : string.Empty));
		#endregion

		#region Finished Goods Mapping
		CreateMap<FinishedGoodsStock, FinishedGoodsStockDto>()
			.ForMember(dest => dest.ProductCode, opt => opt.MapFrom(src => src.Product != null ? src.Product.Code : string.Empty))
			.ForMember(dest => dest.ProductSKU, opt => opt.MapFrom(src => src.Product != null ? src.Product.SKU : string.Empty))
			.ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty))
			.ForMember(dest => dest.ProductArabicName, opt => opt.MapFrom(src => src.Product != null ? src.Product.ArabicName : null))
			.ForMember(dest => dest.ProductCategoryName, opt => opt.MapFrom(src => src.Product != null && src.Product.ProductCategory != null ? src.Product.ProductCategory.Name : null))
			.ForMember(dest => dest.WarehouseName, opt => opt.MapFrom(src => src.Warehouse != null ? src.Warehouse.Name : string.Empty))
			.ForMember(dest => dest.WarehouseCode, opt => opt.MapFrom(src => src.Warehouse != null ? src.Warehouse.Code : string.Empty))
			.ForMember(dest => dest.LocationName, opt => opt.MapFrom(src => src.Location != null ? src.Location.Name : null))
			.ForMember(dest => dest.LocationCode, opt => opt.MapFrom(src => src.Location != null ? src.Location.Code : null))
			.ForMember(dest => dest.LocationSection, opt => opt.MapFrom(src => src.Location != null ? src.Location.Section : null))
			.ForMember(dest => dest.QCInspectionNumber, opt => opt.MapFrom(src => src.QCInspection != null ? src.QCInspection.InspectionNumber : null))
			.ForMember(dest => dest.PackagingOrderNumber, opt => opt.MapFrom(src => src.PackagingOrder != null ? src.PackagingOrder.OrderNumber : null));

		CreateMap<FinishedGoodsRelease, FinishedGoodsReleaseDto>()
			.ForMember(dest => dest.ProductCode, opt => opt.MapFrom(src => src.Product != null ? src.Product.Code : string.Empty))
			.ForMember(dest => dest.ProductSKU, opt => opt.MapFrom(src => src.Product != null ? src.Product.SKU : string.Empty))
			.ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty))
			.ForMember(dest => dest.ProductArabicName, opt => opt.MapFrom(src => src.Product != null ? src.Product.ArabicName : null))
			.ForMember(dest => dest.BatchActualOutputQuantity, opt => opt.MapFrom(src => src.ProductionBatch != null ? src.ProductionBatch.ActualOutputQuantity : 0m))
			.ForMember(dest => dest.PackagingOrderNumber, opt => opt.MapFrom(src => src.PackagingOrder != null ? src.PackagingOrder.OrderNumber : null))
			.ForMember(dest => dest.PackagingBOMName, opt => opt.MapFrom(src => src.PackagingOrder != null && src.PackagingOrder.PackagingBOM != null ? src.PackagingOrder.PackagingBOM.Name : null))
			.ForMember(dest => dest.ActualPackagedQuantity, opt => opt.MapFrom(src => src.PackagingOrder != null ? (decimal?)src.PackagingOrder.ActualQuantity : null))
			.ForMember(dest => dest.QCInspectionNumber, opt => opt.MapFrom(src => src.QCInspection != null ? src.QCInspection.InspectionNumber : null))
			.ForMember(dest => dest.QCInspectionStatus, opt => opt.MapFrom(src => src.QCInspection != null ? src.QCInspection.Status.ToString() : null))
			.ForMember(dest => dest.WarehouseName, opt => opt.MapFrom(src => src.Warehouse != null ? src.Warehouse.Name : string.Empty))
			.ForMember(dest => dest.WarehouseCode, opt => opt.MapFrom(src => src.Warehouse != null ? src.Warehouse.Code : string.Empty))
			.ForMember(dest => dest.LocationName, opt => opt.MapFrom(src => src.Location != null ? src.Location.Name : null))
			.ForMember(dest => dest.LocationCode, opt => opt.MapFrom(src => src.Location != null ? src.Location.Code : null))
			.ForMember(dest => dest.ReleasedByUserName, opt => opt.MapFrom(src => src.ReleasedByUser != null ? (src.ReleasedByUser.FullName ?? src.ReleasedByUser.Username) : string.Empty))
			.ForMember(dest => dest.WorkOrderId, opt => opt.MapFrom(src => src.ProductionBatch != null ? (int?)src.ProductionBatch.WorkOrderId : null))
			.ForMember(dest => dest.WorkOrderNumber, opt => opt.MapFrom(src => src.ProductionBatch != null && src.ProductionBatch.WorkOrder != null ? src.ProductionBatch.WorkOrder.OrderNumber : null));
		#endregion

		#region Purchasing Mappings
		CreateMap<SupplierCategory, SupplierCategoryDto>()
			.ForMember(dest => dest.SuppliersCount, opt => opt.MapFrom(src => src.Suppliers != null ? src.Suppliers.Count : 0));
		CreateMap<CreateSupplierCategoryRequest, SupplierCategory>();

		CreateMap<Supplier, SupplierDto>()
			.ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : null))
			.ForMember(dest => dest.PurchaseOrdersCount, opt => opt.MapFrom(src => src.PurchaseOrders != null ? src.PurchaseOrders.Count : 0))
			.ForMember(dest => dest.PurchaseReceiptsCount, opt => opt.MapFrom(src => src.PurchaseReceipts != null ? src.PurchaseReceipts.Count : 0));
		CreateMap<CreateSupplierRequest, Supplier>();
		CreateMap<UpdateSupplierRequest, Supplier>();

		CreateMap<SupplierPriceHistory, SupplierPriceHistoryDto>()
			.ForMember(dest => dest.SupplierName, opt => opt.MapFrom(src => src.Supplier != null ? src.Supplier.Name : string.Empty))
			.ForMember(dest => dest.MaterialName, opt => opt.MapFrom(src => src.Material != null ? src.Material.Name : string.Empty))
			.ForMember(dest => dest.MaterialCode, opt => opt.MapFrom(src => src.Material != null ? src.Material.Code : string.Empty))
			.ForMember(dest => dest.PurchaseOrderNumber, opt => opt.MapFrom(src => src.PurchaseOrder != null ? src.PurchaseOrder.OrderNumber : null))
			.ForMember(dest => dest.ReceiptNumber, opt => opt.MapFrom(src => src.PurchaseReceipt != null ? src.PurchaseReceipt.ReceiptNumber : null));

		CreateMap<PurchaseRequestItem, PurchaseRequestItemDto>()
			.ForMember(dest => dest.MaterialName, opt => opt.MapFrom(src => src.Material != null ? src.Material.Name : string.Empty))
			.ForMember(dest => dest.MaterialCode, opt => opt.MapFrom(src => src.Material != null ? src.Material.Code : string.Empty));
		CreateMap<PurchaseRequest, PurchaseRequestDto>()
			.ForMember(dest => dest.DepartmentName, opt => opt.MapFrom(src => src.Department != null ? src.Department.Name : null))
			.ForMember(dest => dest.RequestedByName, opt => opt.MapFrom(src => src.RequestedByUser != null ? (src.RequestedByUser.FullName ?? src.RequestedByUser.Username) : null))
			.ForMember(dest => dest.ApprovedByName, opt => opt.MapFrom(src => src.ApprovedByUser != null ? (src.ApprovedByUser.FullName ?? src.ApprovedByUser.Username) : null))
			.ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));

		CreateMap<PurchaseOrderItem, PurchaseOrderItemDto>()
			.ForMember(dest => dest.MaterialName, opt => opt.MapFrom(src => src.Material != null ? src.Material.Name : string.Empty))
			.ForMember(dest => dest.MaterialCode, opt => opt.MapFrom(src => src.Material != null ? src.Material.Code : string.Empty))
			.ForMember(dest => dest.MaterialSKU, opt => opt.MapFrom(src => src.Material != null ? src.Material.SKU : null));
		CreateMap<PurchaseOrder, PurchaseOrderDto>()
			.ForMember(dest => dest.SupplierName, opt => opt.MapFrom(src => src.Supplier != null ? src.Supplier.Name : string.Empty))
			.ForMember(dest => dest.SupplierCode, opt => opt.MapFrom(src => src.Supplier != null ? src.Supplier.Code : string.Empty))
			.ForMember(dest => dest.PurchaseRequestNumber, opt => opt.MapFrom(src => src.PurchaseRequest != null ? src.PurchaseRequest.RequestNumber : null))
			.ForMember(dest => dest.WarehouseName, opt => opt.MapFrom(src => src.Warehouse != null ? src.Warehouse.Name : string.Empty))
			.ForMember(dest => dest.ApprovedByName, opt => opt.MapFrom(src => src.ApprovedByUser != null ? (src.ApprovedByUser.FullName ?? src.ApprovedByUser.Username) : null))
			.ForMember(dest => dest.ReceiptsCount, opt => opt.MapFrom(src => src.Receipts != null ? src.Receipts.Count : 0))
			.ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));

		CreateMap<PurchaseReceiptItem, PurchaseReceiptItemDto>()
			.ForMember(dest => dest.MaterialName, opt => opt.MapFrom(src => src.Material != null ? src.Material.Name : string.Empty))
			.ForMember(dest => dest.MaterialCode, opt => opt.MapFrom(src => src.Material != null ? src.Material.Code : string.Empty))
			.ForMember(dest => dest.WarehouseName, opt => opt.MapFrom(src => src.Warehouse != null ? src.Warehouse.Name : null))
			.ForMember(dest => dest.LocationName, opt => opt.MapFrom(src => src.Location != null ? src.Location.Name : null));
		CreateMap<PurchaseReceipt, PurchaseReceiptDto>()
			.ForMember(dest => dest.PurchaseOrderNumber, opt => opt.MapFrom(src => src.PurchaseOrder != null ? src.PurchaseOrder.OrderNumber : string.Empty))
			.ForMember(dest => dest.SupplierName, opt => opt.MapFrom(src => src.Supplier != null ? src.Supplier.Name : string.Empty))
			.ForMember(dest => dest.SupplierCode, opt => opt.MapFrom(src => src.Supplier != null ? src.Supplier.Code : string.Empty))
			.ForMember(dest => dest.WarehouseName, opt => opt.MapFrom(src => src.Warehouse != null ? src.Warehouse.Name : string.Empty))
			.ForMember(dest => dest.ReceivedByName, opt => opt.MapFrom(src => src.ReceivedByUser != null ? (src.ReceivedByUser.FullName ?? src.ReceivedByUser.Username) : null))
			.ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));
		#endregion

		#region Sales Mappings
		CreateMap<Customer, CustomerDto>()
			.ForMember(dest => dest.SalesOrdersCount, opt => opt.MapFrom(src => src.SalesOrders != null ? src.SalesOrders.Count : 0))
			.ForMember(dest => dest.SalesFulfillmentsCount, opt => opt.MapFrom(src => src.SalesFulfillments != null ? src.SalesFulfillments.Count : 0));
		CreateMap<CreateCustomerRequest, Customer>();
		CreateMap<UpdateCustomerRequest, Customer>();

		CreateMap<SalesOrderItem, SalesOrderItemDto>()
			.ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty))
			.ForMember(dest => dest.ProductCode, opt => opt.MapFrom(src => src.Product != null ? src.Product.Code : string.Empty))
			.ForMember(dest => dest.ProductSKU, opt => opt.MapFrom(src => src.Product != null ? src.Product.SKU : null));
		CreateMap<SalesOrder, SalesOrderDto>()
			.ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.Name : string.Empty))
			.ForMember(dest => dest.CustomerCode, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.Code : string.Empty))
			.ForMember(dest => dest.WarehouseName, opt => opt.MapFrom(src => src.Warehouse != null ? src.Warehouse.Name : string.Empty))
			.ForMember(dest => dest.ConfirmedByName, opt => opt.MapFrom(src => src.ConfirmedByUser != null ? (src.ConfirmedByUser.FullName ?? src.ConfirmedByUser.Username) : null))
			.ForMember(dest => dest.FulfillmentsCount, opt => opt.MapFrom(src => src.Fulfillments != null ? src.Fulfillments.Count : 0))
			.ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));

		CreateMap<SalesFulfillmentItem, SalesFulfillmentItemDto>()
			.ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty))
			.ForMember(dest => dest.ProductCode, opt => opt.MapFrom(src => src.Product != null ? src.Product.Code : string.Empty))
			.ForMember(dest => dest.WarehouseName, opt => opt.MapFrom(src => src.Warehouse != null ? src.Warehouse.Name : null))
			.ForMember(dest => dest.LocationName, opt => opt.MapFrom(src => src.Location != null ? src.Location.Name : null));
		CreateMap<SalesFulfillment, SalesFulfillmentDto>()
			.ForMember(dest => dest.SalesOrderNumber, opt => opt.MapFrom(src => src.SalesOrder != null ? src.SalesOrder.OrderNumber : string.Empty))
			.ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.Name : string.Empty))
			.ForMember(dest => dest.CustomerCode, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.Code : string.Empty))
			.ForMember(dest => dest.WarehouseName, opt => opt.MapFrom(src => src.Warehouse != null ? src.Warehouse.Name : string.Empty))
			.ForMember(dest => dest.ShippedByName, opt => opt.MapFrom(src => src.ShippedByUser != null ? (src.ShippedByUser.FullName ?? src.ShippedByUser.Username) : null))
			.ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));
		#endregion

		#region Invoicing & Payments (Phase 15)
		CreateMap<InvoiceItem, InvoiceItemDto>()
			.ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty))
			.ForMember(dest => dest.ProductCode, opt => opt.MapFrom(src => src.Product != null ? src.Product.Code : string.Empty))
			.ForMember(dest => dest.ProductSKU, opt => opt.MapFrom(src => src.Product != null ? src.Product.SKU : null));

		CreateMap<Invoice, InvoiceDto>()
			.ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.Name : string.Empty))
			.ForMember(dest => dest.CustomerCode, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.Code : string.Empty))
			.ForMember(dest => dest.SalesOrderNumber, opt => opt.MapFrom(src => src.SalesOrder != null ? src.SalesOrder.OrderNumber : string.Empty))
			.ForMember(dest => dest.FulfillmentNumber, opt => opt.MapFrom(src => src.SalesFulfillment != null ? src.SalesFulfillment.FulfillmentNumber : null))
			.ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items))
			.ForMember(dest => dest.Payments, opt => opt.MapFrom(src => src.Payments));

		CreateMap<Payment, PaymentDto>()
			.ForMember(dest => dest.InvoiceNumber, opt => opt.MapFrom(src => src.Invoice != null ? src.Invoice.InvoiceNumber : string.Empty))
			.ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.Name : (src.Invoice != null && src.Invoice.Customer != null ? src.Invoice.Customer.Name : string.Empty)))
			.ForMember(dest => dest.CustomerCode, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.Code : (src.Invoice != null && src.Invoice.Customer != null ? src.Invoice.Customer.Code : string.Empty)))
			.ForMember(dest => dest.InvoiceTotalAmount, opt => opt.MapFrom(src => src.Invoice != null ? src.Invoice.TotalAmount : 0))
			.ForMember(dest => dest.InvoiceRemainingAmount, opt => opt.MapFrom(src => src.Invoice != null ? src.Invoice.RemainingAmount : 0));
		#endregion

		#region Accounting & General Ledger (Phase 16)
		CreateMap<Account, AccountDto>()
			.ForMember(dest => dest.ParentAccountName, opt => opt.MapFrom(src => src.ParentAccount != null ? src.ParentAccount.AccountNameAr : null))
			.ForMember(dest => dest.ParentAccountCode, opt => opt.MapFrom(src => src.ParentAccount != null ? src.ParentAccount.AccountCode : null))
			.ForMember(dest => dest.ChildCount, opt => opt.MapFrom(src => src.ChildAccounts != null ? src.ChildAccounts.Count : 0));
		CreateMap<AccountCreateDto, Account>();
		CreateMap<AccountUpdateDto, Account>();

		CreateMap<AccountingPeriod, AccountingPeriodDto>()
			.ForMember(dest => dest.ClosedByName, opt => opt.MapFrom(src => src.ClosedByUser != null ? (src.ClosedByUser.FullName ?? src.ClosedByUser.Username) : null))
			.ForMember(dest => dest.JournalCount, opt => opt.MapFrom(src => src.JournalEntries != null ? src.JournalEntries.Count : 0));
		CreateMap<AccountingPeriodCreateDto, AccountingPeriod>();

		CreateMap<JournalEntryLine, JournalEntryLineDto>()
			.ForMember(dest => dest.AccountCode, opt => opt.MapFrom(src => src.Account != null ? src.Account.AccountCode : string.Empty))
			.ForMember(dest => dest.AccountName, opt => opt.MapFrom(src => src.Account != null ? src.Account.AccountName : string.Empty))
			.ForMember(dest => dest.AccountNameAr, opt => opt.MapFrom(src => src.Account != null ? src.Account.AccountNameAr : string.Empty))
			.ForMember(dest => dest.AccountType, opt => opt.MapFrom(src => src.Account != null ? src.Account.AccountType : AccountType.Asset))
			.ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.Name : null))
			.ForMember(dest => dest.SupplierName, opt => opt.MapFrom(src => src.Supplier != null ? src.Supplier.Name : null))
			.ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : null))
			.ForMember(dest => dest.MaterialName, opt => opt.MapFrom(src => src.Material != null ? src.Material.Name : null));

		CreateMap<JournalEntry, JournalEntryDto>()
			.ForMember(dest => dest.PeriodName, opt => opt.MapFrom(src => src.AccountingPeriod != null ? src.AccountingPeriod.PeriodName : null))
			.ForMember(dest => dest.ReversalOfJournalNumber, opt => opt.MapFrom(src => src.ReversalOfJournalEntry != null ? src.ReversalOfJournalEntry.JournalNumber : null))
			.ForMember(dest => dest.CreatedByName, opt => opt.MapFrom(src => src.CreatedByUser != null ? (src.CreatedByUser.FullName ?? src.CreatedByUser.Username) : null))
			.ForMember(dest => dest.PostedByName, opt => opt.MapFrom(src => src.PostedByUser != null ? (src.PostedByUser.FullName ?? src.PostedByUser.Username) : null))
			.ForMember(dest => dest.Lines, opt => opt.MapFrom(src => src.Lines));

		CreateMap<SupplierPayment, SupplierPaymentDto>()
			.ForMember(dest => dest.SupplierName, opt => opt.MapFrom(src => src.Supplier != null ? src.Supplier.Name : string.Empty))
			.ForMember(dest => dest.PurchaseReceiptNumber, opt => opt.MapFrom(src => src.PurchaseReceipt != null ? src.PurchaseReceipt.ReceiptNumber : null))
			.ForMember(dest => dest.PurchaseOrderNumber, opt => opt.MapFrom(src => src.PurchaseOrder != null ? src.PurchaseOrder.OrderNumber : null))
			.ForMember(dest => dest.CreatedByName, opt => opt.MapFrom(src => src.CreatedByUser != null ? (src.CreatedByUser.FullName ?? src.CreatedByUser.Username) : null));
		CreateMap<SupplierPaymentCreateDto, SupplierPayment>();
		#endregion
	}
}
