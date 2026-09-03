using FactoryX.Infrastructure.Contracts;

namespace FactoryX.Infrastructure.Repositories;

public class RepositoryManager : IRepositoryManager
{
	private readonly AppDbContext _context;
	private readonly IMachineRepository _machineRepository;
	private readonly IOperatorRepository _operatorRepository;
	private readonly IWorkOrderRepository _workOrderRepository;
	private readonly IShiftRepository _shiftRepository;
	private readonly IDowntimeRepository _downtimeRepository;
	private readonly IUserRepository _userRepository;
	private readonly IProductRepository _productRepository;
	private readonly IProductCategoryRepository _productCategoryRepository;
	private readonly IMaterialRepository _materialRepository;
	private readonly IMaterialCategoryRepository _materialCategoryRepository;
	private readonly IMaterialUsageRepository _materialUsageRepository;
	private readonly IProductionRecordRepository _productionRecordRepository;
	private readonly IWarehouseRepository _warehouseRepository;
	private readonly IWarehouseLocationRepository _warehouseLocationRepository;
	private readonly IStockBalanceRepository _stockBalanceRepository;
	private readonly IInventoryTransactionRepository _inventoryTransactionRepository;
	private readonly IRecipeRepository _recipeRepository;
	private readonly IRecipeVersionRepository _recipeVersionRepository;
	private readonly IRecipeItemRepository _recipeItemRepository;
	private readonly IProductionBatchRepository _productionBatchRepository;
	private readonly IWasteRepository _wasteRepository;
	private readonly IWasteReasonRepository _wasteReasonRepository;
	private readonly IQualityTemplateRepository _qualityTemplateRepository;
	private readonly IQualityInspectionRepository _qualityInspectionRepository;
	private readonly IPackagingBOMRepository _packagingBOMRepository;
	private readonly IPackagingOrderRepository _packagingOrderRepository;
	private readonly IFinishedGoodsStockRepository _finishedGoodsStockRepository;
	private readonly IFinishedGoodsReleaseRepository _finishedGoodsReleaseRepository;
	private readonly ISupplierCategoryRepository _supplierCategoryRepository;
	private readonly ISupplierRepository _supplierRepository;
	private readonly IPurchaseRequestRepository _purchaseRequestRepository;
	private readonly IPurchaseOrderRepository _purchaseOrderRepository;
	private readonly IPurchaseReceiptRepository _purchaseReceiptRepository;
	private readonly ISupplierPriceHistoryRepository _supplierPriceHistoryRepository;
	private readonly ICustomerRepository _customerRepository;
	private readonly ISalesOrderRepository _salesOrderRepository;
	private readonly ISalesFulfillmentRepository _salesFulfillmentRepository;
	private readonly IInvoiceRepository _invoiceRepository;
	private readonly IPaymentRepository _paymentRepository;
	private readonly IAccountRepository _accountRepository;
	private readonly IAccountingPeriodRepository _accountingPeriodRepository;
	private readonly IJournalEntryRepository _journalEntryRepository;
	private readonly ISupplierPaymentRepository _supplierPaymentRepository;
	private readonly IAccountingSettingRepository _accountingSettingRepository;

	public RepositoryManager(
		AppDbContext context,
		IMachineRepository machineRepository,
		IOperatorRepository operatorRepository,
		IWorkOrderRepository workOrderRepository,
		IShiftRepository shiftRepository,
		IDowntimeRepository downtimeRepository,
		IUserRepository userRepository,
		IProductRepository productRepository,
		IProductCategoryRepository productCategoryRepository,
		IMaterialRepository materialRepository,
		IMaterialCategoryRepository materialCategoryRepository,
		IMaterialUsageRepository materialUsageRepository,
		IProductionRecordRepository productionRecordRepository,
		IWarehouseRepository warehouseRepository,
		IWarehouseLocationRepository warehouseLocationRepository,
		IStockBalanceRepository stockBalanceRepository,
		IInventoryTransactionRepository inventoryTransactionRepository,
		IRecipeRepository recipeRepository,
		IRecipeVersionRepository recipeVersionRepository,
		IRecipeItemRepository recipeItemRepository,
		IProductionBatchRepository productionBatchRepository,
		IWasteRepository wasteRepository,
		IWasteReasonRepository wasteReasonRepository,
		IQualityTemplateRepository qualityTemplateRepository,
		IQualityInspectionRepository qualityInspectionRepository,
		IPackagingBOMRepository packagingBOMRepository,
		IPackagingOrderRepository packagingOrderRepository,
		IFinishedGoodsStockRepository finishedGoodsStockRepository,
		IFinishedGoodsReleaseRepository finishedGoodsReleaseRepository,
		ISupplierCategoryRepository supplierCategoryRepository,
		ISupplierRepository supplierRepository,
		IPurchaseRequestRepository purchaseRequestRepository,
		IPurchaseOrderRepository purchaseOrderRepository,
		IPurchaseReceiptRepository purchaseReceiptRepository,
		ISupplierPriceHistoryRepository supplierPriceHistoryRepository,
		ICustomerRepository customerRepository,
		ISalesOrderRepository salesOrderRepository,
		ISalesFulfillmentRepository salesFulfillmentRepository,
		IInvoiceRepository invoiceRepository,
		IPaymentRepository paymentRepository,
		IAccountRepository accountRepository,
		IAccountingPeriodRepository accountingPeriodRepository,
		IJournalEntryRepository journalEntryRepository,
		ISupplierPaymentRepository supplierPaymentRepository,
		IAccountingSettingRepository accountingSettingRepository)
	{
		_context = context;
		_machineRepository = machineRepository;
		_operatorRepository = operatorRepository;
		_workOrderRepository = workOrderRepository;
		_shiftRepository = shiftRepository;
		_downtimeRepository = downtimeRepository;
		_userRepository = userRepository;
		_productRepository = productRepository;
		_productCategoryRepository = productCategoryRepository;
		_materialRepository = materialRepository;
		_materialCategoryRepository = materialCategoryRepository;
		_materialUsageRepository = materialUsageRepository;
		_productionRecordRepository = productionRecordRepository;
		_warehouseRepository = warehouseRepository;
		_warehouseLocationRepository = warehouseLocationRepository;
		_stockBalanceRepository = stockBalanceRepository;
		_inventoryTransactionRepository = inventoryTransactionRepository;
		_recipeRepository = recipeRepository;
		_recipeVersionRepository = recipeVersionRepository;
		_recipeItemRepository = recipeItemRepository;
		_productionBatchRepository = productionBatchRepository;
		_wasteRepository = wasteRepository;
		_wasteReasonRepository = wasteReasonRepository;
		_qualityTemplateRepository = qualityTemplateRepository;
		_qualityInspectionRepository = qualityInspectionRepository;
		_packagingBOMRepository = packagingBOMRepository;
		_packagingOrderRepository = packagingOrderRepository;
		_finishedGoodsStockRepository = finishedGoodsStockRepository;
		_finishedGoodsReleaseRepository = finishedGoodsReleaseRepository;
		_supplierCategoryRepository = supplierCategoryRepository;
		_supplierRepository = supplierRepository;
		_purchaseRequestRepository = purchaseRequestRepository;
		_purchaseOrderRepository = purchaseOrderRepository;
		_purchaseReceiptRepository = purchaseReceiptRepository;
		_supplierPriceHistoryRepository = supplierPriceHistoryRepository;
		_customerRepository = customerRepository;
		_salesOrderRepository = salesOrderRepository;
		_salesFulfillmentRepository = salesFulfillmentRepository;
		_invoiceRepository = invoiceRepository;
		_paymentRepository = paymentRepository;
		_accountRepository = accountRepository;
		_accountingPeriodRepository = accountingPeriodRepository;
		_journalEntryRepository = journalEntryRepository;
		_supplierPaymentRepository = supplierPaymentRepository;
		_accountingSettingRepository = accountingSettingRepository;
	}

	public IMachineRepository MachineRepository => _machineRepository;
	public IOperatorRepository OperatorRepository => _operatorRepository;
	public IWorkOrderRepository WorkOrderRepository => _workOrderRepository;
	public IShiftRepository ShiftRepository => _shiftRepository;
	public IDowntimeRepository DowntimeRepository => _downtimeRepository;
	public IUserRepository UserRepository => _userRepository;
	public IProductRepository ProductRepository => _productRepository;
	public IProductCategoryRepository ProductCategoryRepository => _productCategoryRepository;
	public IMaterialRepository MaterialRepository => _materialRepository;
	public IMaterialCategoryRepository MaterialCategoryRepository => _materialCategoryRepository;
	public IMaterialUsageRepository MaterialUsageRepository => _materialUsageRepository;
	public IProductionRecordRepository ProductionRecordRepository => _productionRecordRepository;
	public IWarehouseRepository WarehouseRepository => _warehouseRepository;
	public IWarehouseLocationRepository WarehouseLocationRepository => _warehouseLocationRepository;
	public IStockBalanceRepository StockBalanceRepository => _stockBalanceRepository;
	public IInventoryTransactionRepository InventoryTransactionRepository => _inventoryTransactionRepository;
	public IRecipeRepository RecipeRepository => _recipeRepository;
	public IRecipeVersionRepository RecipeVersionRepository => _recipeVersionRepository;
	public IRecipeItemRepository RecipeItemRepository => _recipeItemRepository;
	public IProductionBatchRepository ProductionBatchRepository => _productionBatchRepository;
	public IWasteRepository WasteRepository => _wasteRepository;
	public IWasteReasonRepository WasteReasonRepository => _wasteReasonRepository;
	public IQualityTemplateRepository QualityTemplateRepository => _qualityTemplateRepository;
	public IQualityInspectionRepository QualityInspectionRepository => _qualityInspectionRepository;
	public IPackagingBOMRepository PackagingBOMRepository => _packagingBOMRepository;
	public IPackagingOrderRepository PackagingOrderRepository => _packagingOrderRepository;
	public IFinishedGoodsStockRepository FinishedGoodsStockRepository => _finishedGoodsStockRepository;
	public IFinishedGoodsReleaseRepository FinishedGoodsReleaseRepository => _finishedGoodsReleaseRepository;
	public ISupplierCategoryRepository SupplierCategoryRepository => _supplierCategoryRepository;
	public ISupplierRepository SupplierRepository => _supplierRepository;
	public IPurchaseRequestRepository PurchaseRequestRepository => _purchaseRequestRepository;
	public IPurchaseOrderRepository PurchaseOrderRepository => _purchaseOrderRepository;
	public IPurchaseReceiptRepository PurchaseReceiptRepository => _purchaseReceiptRepository;
	public ISupplierPriceHistoryRepository SupplierPriceHistoryRepository => _supplierPriceHistoryRepository;
	public ICustomerRepository CustomerRepository => _customerRepository;
	public ISalesOrderRepository SalesOrderRepository => _salesOrderRepository;
	public ISalesFulfillmentRepository SalesFulfillmentRepository => _salesFulfillmentRepository;
	public IInvoiceRepository InvoiceRepository => _invoiceRepository;
	public IPaymentRepository PaymentRepository => _paymentRepository;

	// Phase 16: Accounting & General Ledger
	public IAccountRepository AccountRepository => _accountRepository;
	public IAccountingPeriodRepository AccountingPeriodRepository => _accountingPeriodRepository;
	public IJournalEntryRepository JournalEntryRepository => _journalEntryRepository;
	public ISupplierPaymentRepository SupplierPaymentRepository => _supplierPaymentRepository;
	public IAccountingSettingRepository AccountingSettingRepository => _accountingSettingRepository;

	public async Task SaveAsync()
	{
		await _context.SaveChangesAsync();
	}
}
