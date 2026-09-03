namespace FactoryX.Application.Services.Abstracts;

public interface IServiceManager
{
	IMachineService MachineService { get; }
	IOperatorService OperatorService { get; }
	IProductionRecordService ProductionRecordService { get; }
	IProductService ProductService { get; }
	IProductCategoryService ProductCategoryService { get; }
	IUserService UserService { get; }
	IShiftService ShiftService { get; }
	IWorkOrderService WorkOrderService { get; }
	IWarehouseService WarehouseService { get; }
	IWarehouseLocationService WarehouseLocationService { get; }
	IInventoryService InventoryService { get; }
	IMaterialCategoryService MaterialCategoryService { get; }
	IMaterialService MaterialService { get; }
	IRecipeService RecipeService { get; }
	IRecipeCostService RecipeCostService { get; }
	IProductionPlanningService ProductionPlanningService { get; }
	IProductionBatchService ProductionBatchService { get; }
	IProductionExecutionService ProductionExecutionService { get; }
	IWasteService WasteService { get; }
	IWasteReasonService WasteReasonService { get; }
	IQualityTemplateService QualityTemplateService { get; }
	IQualityInspectionService QualityInspectionService { get; }
	IQualityGateService QualityGateService { get; }
	IPackagingBOMService PackagingBOMService { get; }
	IPackagingCostService PackagingCostService { get; }
	IPackagingOrderService PackagingOrderService { get; }
	IFinishedGoodsService FinishedGoodsService { get; }
	IFinishedGoodsReleaseService FinishedGoodsReleaseService { get; }
	ISupplierService SupplierService { get; }
	IPurchaseRequestService PurchaseRequestService { get; }
	IPurchaseOrderService PurchaseOrderService { get; }
	IPurchaseReceiptService PurchaseReceiptService { get; }
	ICustomerService CustomerService { get; }
	ISalesOrderService SalesOrderService { get; }
	ISalesFulfillmentService SalesFulfillmentService { get; }
	IInvoiceService InvoiceService { get; }
	IPaymentService PaymentService { get; }
	ICustomerStatementService CustomerStatementService { get; }

	// Phase 16: Accounting & General Ledger
	IAccountService AccountService { get; }
	IAccountingPeriodService AccountingPeriodService { get; }
	IJournalEntryService JournalEntryService { get; }
	IAccountingPostingService AccountingPostingService { get; }
	IGeneralLedgerService GeneralLedgerService { get; }
	ISupplierPaymentService SupplierPaymentService { get; }
	IAccountingDashboardService AccountingDashboardService { get; }

	// Phase 19: System Administration & Configuration
	ISettingsService SettingsService { get; }
}

