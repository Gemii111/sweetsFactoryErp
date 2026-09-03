namespace FactoryX.Infrastructure.Contracts;

public interface IRepositoryManager
{
    IMachineRepository MachineRepository { get; }
    IOperatorRepository OperatorRepository { get; }
    IWorkOrderRepository WorkOrderRepository { get; }
    IShiftRepository ShiftRepository { get; }
    IDowntimeRepository DowntimeRepository { get; }
    IUserRepository UserRepository { get; }
    IProductRepository ProductRepository { get; }
    IProductCategoryRepository ProductCategoryRepository { get; }
    IMaterialRepository MaterialRepository { get; }
    IMaterialCategoryRepository MaterialCategoryRepository { get; }
    IMaterialUsageRepository MaterialUsageRepository { get; }
    IProductionRecordRepository ProductionRecordRepository { get; }
    IWarehouseRepository WarehouseRepository { get; }
    IWarehouseLocationRepository WarehouseLocationRepository { get; }
    IStockBalanceRepository StockBalanceRepository { get; }
    IInventoryTransactionRepository InventoryTransactionRepository { get; }
    IRecipeRepository RecipeRepository { get; }
    IRecipeVersionRepository RecipeVersionRepository { get; }
    IRecipeItemRepository RecipeItemRepository { get; }
    IProductionBatchRepository ProductionBatchRepository { get; }
    IWasteRepository WasteRepository { get; }
    IWasteReasonRepository WasteReasonRepository { get; }
    IQualityTemplateRepository QualityTemplateRepository { get; }
    IQualityInspectionRepository QualityInspectionRepository { get; }
    IPackagingBOMRepository PackagingBOMRepository { get; }
    IPackagingOrderRepository PackagingOrderRepository { get; }
    IFinishedGoodsStockRepository FinishedGoodsStockRepository { get; }
    IFinishedGoodsReleaseRepository FinishedGoodsReleaseRepository { get; }
    ISupplierCategoryRepository SupplierCategoryRepository { get; }
    ISupplierRepository SupplierRepository { get; }
    IPurchaseRequestRepository PurchaseRequestRepository { get; }
    IPurchaseOrderRepository PurchaseOrderRepository { get; }
    IPurchaseReceiptRepository PurchaseReceiptRepository { get; }
    ISupplierPriceHistoryRepository SupplierPriceHistoryRepository { get; }
    ICustomerRepository CustomerRepository { get; }
    ISalesOrderRepository SalesOrderRepository { get; }
    ISalesFulfillmentRepository SalesFulfillmentRepository { get; }
    IInvoiceRepository InvoiceRepository { get; }
    IPaymentRepository PaymentRepository { get; }

    // Phase 16: Accounting & General Ledger
    IAccountRepository AccountRepository { get; }
    IAccountingPeriodRepository AccountingPeriodRepository { get; }
    IJournalEntryRepository JournalEntryRepository { get; }
    ISupplierPaymentRepository SupplierPaymentRepository { get; }
    IAccountingSettingRepository AccountingSettingRepository { get; }

    Task SaveAsync();
}