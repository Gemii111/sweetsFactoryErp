using FactoryX.Application.Services.Abstracts;

namespace FactoryX.Application.Services.Concretes;

public sealed class ServiceManager : IServiceManager
{
	private readonly IMachineService _machineService;
	private readonly IOperatorService _operatorService;
	private readonly IProductionRecordService _productionRecordService;
	private readonly IProductService _productService;
	private readonly IProductCategoryService _productCategoryService;
	private readonly IUserService _userService;
	private readonly IShiftService _shiftService;
	private readonly IWorkOrderService _workOrderService;
	private readonly IWarehouseService _warehouseService;
	private readonly IWarehouseLocationService _warehouseLocationService;
	private readonly IInventoryService _inventoryService;
	private readonly IMaterialCategoryService _materialCategoryService;
	private readonly IMaterialService _materialService;
	private readonly IRecipeService _recipeService;
	private readonly IRecipeCostService _recipeCostService;
	private readonly IProductionPlanningService _productionPlanningService;
	private readonly IProductionBatchService _productionBatchService;
	private readonly IProductionExecutionService _productionExecutionService;
	private readonly IWasteService _wasteService;
	private readonly IWasteReasonService _wasteReasonService;
	private readonly IQualityTemplateService _qualityTemplateService;
	private readonly IQualityInspectionService _qualityInspectionService;
	private readonly IQualityGateService _qualityGateService;
	private readonly IPackagingBOMService _packagingBOMService;
	private readonly IPackagingCostService _packagingCostService;
	private readonly IPackagingOrderService _packagingOrderService;
	private readonly IFinishedGoodsService _finishedGoodsService;
	private readonly IFinishedGoodsReleaseService _finishedGoodsReleaseService;
	private readonly ISupplierService _supplierService;
	private readonly IPurchaseRequestService _purchaseRequestService;
	private readonly IPurchaseOrderService _purchaseOrderService;
	private readonly IPurchaseReceiptService _purchaseReceiptService;
	private readonly ICustomerService _customerService;
	private readonly ISalesOrderService _salesOrderService;
	private readonly ISalesFulfillmentService _salesFulfillmentService;
	private readonly IInvoiceService _invoiceService;
	private readonly IPaymentService _paymentService;
	private readonly ICustomerStatementService _customerStatementService;
	private readonly IAccountService _accountService;
	private readonly IAccountingPeriodService _accountingPeriodService;
	private readonly IJournalEntryService _journalEntryService;
	private readonly IAccountingPostingService _accountingPostingService;
	private readonly IGeneralLedgerService _generalLedgerService;
	private readonly ISupplierPaymentService _supplierPaymentService;
	private readonly IAccountingDashboardService _accountingDashboardService;
	private readonly ISettingsService _settingsService;

	public ServiceManager(
		IMachineService machineService,
		IOperatorService operatorService,
		IProductionRecordService productionRecordService,
		IProductService productService,
		IProductCategoryService productCategoryService,
		IUserService userService,
		IShiftService shiftService,
		IWorkOrderService workOrderService,
		IWarehouseService warehouseService,
		IWarehouseLocationService warehouseLocationService,
		IInventoryService inventoryService,
		IMaterialCategoryService materialCategoryService,
		IMaterialService materialService,
		IRecipeService recipeService,
		IRecipeCostService recipeCostService,
		IProductionPlanningService productionPlanningService,
		IProductionBatchService productionBatchService,
		IProductionExecutionService productionExecutionService,
		IWasteService wasteService,
		IWasteReasonService wasteReasonService,
		IQualityTemplateService qualityTemplateService,
		IQualityInspectionService qualityInspectionService,
		IQualityGateService qualityGateService,
		IPackagingBOMService packagingBOMService,
		IPackagingCostService packagingCostService,
		IPackagingOrderService packagingOrderService,
		IFinishedGoodsService finishedGoodsService,
		IFinishedGoodsReleaseService finishedGoodsReleaseService,
		ISupplierService supplierService,
		IPurchaseRequestService purchaseRequestService,
		IPurchaseOrderService purchaseOrderService,
		IPurchaseReceiptService purchaseReceiptService,
		ICustomerService customerService,
		ISalesOrderService salesOrderService,
		ISalesFulfillmentService salesFulfillmentService,
		IInvoiceService invoiceService,
		IPaymentService paymentService,
		ICustomerStatementService customerStatementService,
		IAccountService accountService,
		IAccountingPeriodService accountingPeriodService,
		IJournalEntryService journalEntryService,
		IAccountingPostingService accountingPostingService,
		IGeneralLedgerService generalLedgerService,
		ISupplierPaymentService supplierPaymentService,
		IAccountingDashboardService accountingDashboardService,
		ISettingsService settingsService)
	{
		_machineService = machineService;
		_operatorService = operatorService;
		_productionRecordService = productionRecordService;
		_productService = productService;
		_productCategoryService = productCategoryService;
		_userService = userService;
		_shiftService = shiftService;
		_workOrderService = workOrderService;
		_warehouseService = warehouseService;
		_warehouseLocationService = warehouseLocationService;
		_inventoryService = inventoryService;
		_materialCategoryService = materialCategoryService;
		_materialService = materialService;
		_recipeService = recipeService;
		_recipeCostService = recipeCostService;
		_productionPlanningService = productionPlanningService;
		_productionBatchService = productionBatchService;
		_productionExecutionService = productionExecutionService;
		_wasteService = wasteService;
		_wasteReasonService = wasteReasonService;
		_qualityTemplateService = qualityTemplateService;
		_qualityInspectionService = qualityInspectionService;
		_qualityGateService = qualityGateService;
		_packagingBOMService = packagingBOMService;
		_packagingCostService = packagingCostService;
		_packagingOrderService = packagingOrderService;
		_finishedGoodsService = finishedGoodsService;
		_finishedGoodsReleaseService = finishedGoodsReleaseService;
		_supplierService = supplierService;
		_purchaseRequestService = purchaseRequestService;
		_purchaseOrderService = purchaseOrderService;
		_purchaseReceiptService = purchaseReceiptService;
		_customerService = customerService;
		_salesOrderService = salesOrderService;
		_salesFulfillmentService = salesFulfillmentService;
		_invoiceService = invoiceService;
		_paymentService = paymentService;
		_customerStatementService = customerStatementService;
		_accountService = accountService;
		_accountingPeriodService = accountingPeriodService;
		_journalEntryService = journalEntryService;
		_accountingPostingService = accountingPostingService;
		_generalLedgerService = generalLedgerService;
		_supplierPaymentService = supplierPaymentService;
		_accountingDashboardService = accountingDashboardService;
		_settingsService = settingsService;
	}

	public IMachineService MachineService => _machineService;
	public IOperatorService OperatorService => _operatorService;
	public IProductionRecordService ProductionRecordService => _productionRecordService;
	public IProductService ProductService => _productService;
	public IProductCategoryService ProductCategoryService => _productCategoryService;
	public IUserService UserService => _userService;
	public IShiftService ShiftService => _shiftService;
	public IWorkOrderService WorkOrderService => _workOrderService;
	public IWarehouseService WarehouseService => _warehouseService;
	public IWarehouseLocationService WarehouseLocationService => _warehouseLocationService;
	public IInventoryService InventoryService => _inventoryService;
	public IMaterialCategoryService MaterialCategoryService => _materialCategoryService;
	public IMaterialService MaterialService => _materialService;
	public IRecipeService RecipeService => _recipeService;
	public IRecipeCostService RecipeCostService => _recipeCostService;
	public IProductionPlanningService ProductionPlanningService => _productionPlanningService;
	public IProductionBatchService ProductionBatchService => _productionBatchService;
	public IProductionExecutionService ProductionExecutionService => _productionExecutionService;
	public IWasteService WasteService => _wasteService;
	public IWasteReasonService WasteReasonService => _wasteReasonService;
	public IQualityTemplateService QualityTemplateService => _qualityTemplateService;
	public IQualityInspectionService QualityInspectionService => _qualityInspectionService;
	public IQualityGateService QualityGateService => _qualityGateService;
	public IPackagingBOMService PackagingBOMService => _packagingBOMService;
	public IPackagingCostService PackagingCostService => _packagingCostService;
	public IPackagingOrderService PackagingOrderService => _packagingOrderService;
	public IFinishedGoodsService FinishedGoodsService => _finishedGoodsService;
	public IFinishedGoodsReleaseService FinishedGoodsReleaseService => _finishedGoodsReleaseService;
	public ISupplierService SupplierService => _supplierService;
	public IPurchaseRequestService PurchaseRequestService => _purchaseRequestService;
	public IPurchaseOrderService PurchaseOrderService => _purchaseOrderService;
	public IPurchaseReceiptService PurchaseReceiptService => _purchaseReceiptService;
	public ICustomerService CustomerService => _customerService;
	public ISalesOrderService SalesOrderService => _salesOrderService;
	public ISalesFulfillmentService SalesFulfillmentService => _salesFulfillmentService;
	public IInvoiceService InvoiceService => _invoiceService;
	public IPaymentService PaymentService => _paymentService;
	public ICustomerStatementService CustomerStatementService => _customerStatementService;

	// Phase 16: Accounting & General Ledger
	public IAccountService AccountService => _accountService;
	public IAccountingPeriodService AccountingPeriodService => _accountingPeriodService;
	public IJournalEntryService JournalEntryService => _journalEntryService;
	public IAccountingPostingService AccountingPostingService => _accountingPostingService;
	public IGeneralLedgerService GeneralLedgerService => _generalLedgerService;
	public ISupplierPaymentService SupplierPaymentService => _supplierPaymentService;
	public IAccountingDashboardService AccountingDashboardService => _accountingDashboardService;

	// Phase 19: System Administration & Configuration
	public ISettingsService SettingsService => _settingsService;
}

