using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using FactoryX.Domain.Interfaces;
using FactoryX.Infrastructure.Contracts;
using FactoryX.Infrastructure.Repositories;

namespace FactoryX.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IMachineRepository, MachineRepository>();
        services.AddScoped<IOperatorRepository, OperatorRepository>();
		services.AddScoped<IShiftRepository, ShiftRepository>();
		services.AddScoped<IWorkOrderRepository, WorkOrderRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IProductCategoryRepository, ProductCategoryRepository>();
        services.AddScoped<IProductionRecordRepository, ProductionRecordRepository>();
		services.AddScoped<IDowntimeRepository, DowntimeRepository>();
        services.AddScoped<IMaterialRepository, MaterialRepository>();
        services.AddScoped<IMaterialCategoryRepository, MaterialCategoryRepository>();
        services.AddScoped<IMaterialUsageRepository, MaterialUsageRepository>();
        services.AddScoped<IWarehouseRepository, WarehouseRepository>();
        services.AddScoped<IWarehouseLocationRepository, WarehouseLocationRepository>();
        services.AddScoped<IStockBalanceRepository, StockBalanceRepository>();
        services.AddScoped<IInventoryTransactionRepository, InventoryTransactionRepository>();
        services.AddScoped<IRecipeRepository, RecipeRepository>();
        services.AddScoped<IRecipeVersionRepository, RecipeVersionRepository>();
        services.AddScoped<IRecipeItemRepository, RecipeItemRepository>();
        services.AddScoped<IProductionBatchRepository, ProductionBatchRepository>();
        services.AddScoped<IWasteRepository, WasteRepository>();
        services.AddScoped<IWasteReasonRepository, WasteReasonRepository>();
        services.AddScoped<IQualityTemplateRepository, QualityTemplateRepository>();
        services.AddScoped<IQualityInspectionRepository, QualityInspectionRepository>();
        services.AddScoped<IPackagingBOMRepository, PackagingBOMRepository>();
        services.AddScoped<IPackagingOrderRepository, PackagingOrderRepository>();
        services.AddScoped<IFinishedGoodsStockRepository, FinishedGoodsStockRepository>();
        services.AddScoped<IFinishedGoodsReleaseRepository, FinishedGoodsReleaseRepository>();
        services.AddScoped<ISupplierCategoryRepository, SupplierCategoryRepository>();
        services.AddScoped<ISupplierRepository, SupplierRepository>();
        services.AddScoped<IPurchaseRequestRepository, PurchaseRequestRepository>();
        services.AddScoped<IPurchaseOrderRepository, PurchaseOrderRepository>();
        services.AddScoped<IPurchaseReceiptRepository, PurchaseReceiptRepository>();
        services.AddScoped<ISupplierPriceHistoryRepository, SupplierPriceHistoryRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<ISalesOrderRepository, SalesOrderRepository>();
        services.AddScoped<ISalesFulfillmentRepository, SalesFulfillmentRepository>();
        services.AddScoped<IInvoiceRepository, InvoiceRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<IAccountingPeriodRepository, AccountingPeriodRepository>();
        services.AddScoped<IJournalEntryRepository, JournalEntryRepository>();
        services.AddScoped<ISupplierPaymentRepository, SupplierPaymentRepository>();
        services.AddScoped<IAccountingSettingRepository, AccountingSettingRepository>();
		services.AddScoped<IRepositoryManager, RepositoryManager>();

		return services;
    }
}