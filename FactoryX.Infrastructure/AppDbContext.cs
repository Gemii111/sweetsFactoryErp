using Microsoft.EntityFrameworkCore;
using FactoryX.Domain.Entities;

namespace FactoryX.Infrastructure;

public class AppDbContext : DbContext
{
    // Existing Entities
    public DbSet<Machine> Machines => Set<Machine>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();
    public DbSet<WorkOrderMaterialRequirement> WorkOrderMaterialRequirements => Set<WorkOrderMaterialRequirement>();
    public DbSet<Operator> Operators => Set<Operator>();
    public DbSet<ProductionRecord> ProductionRecords => Set<ProductionRecord>();
    public DbSet<Shift> Shifts => Set<Shift>();
    public DbSet<Downtime> Downtimes => Set<Downtime>();
    public DbSet<Material> Materials => Set<Material>();
    public DbSet<MaterialUsage> MaterialUsages => Set<MaterialUsage>();
    public DbSet<User> Users => Set<User>();

    // Factory Setup
    public DbSet<Factory> Factories => Set<Factory>();
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<WarehouseLocation> WarehouseLocations => Set<WarehouseLocation>();
    public DbSet<ProductionArea> ProductionAreas => Set<ProductionArea>();
    public DbSet<ProductionLine> ProductionLines => Set<ProductionLine>();
    public DbSet<WorkCenter> WorkCenters => Set<WorkCenter>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Employee> Employees => Set<Employee>();

    // Categories
    public DbSet<MaterialCategory> MaterialCategories => Set<MaterialCategory>();
    public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();

    // Recipes & BOM
    public DbSet<Recipe> Recipes => Set<Recipe>();
    public DbSet<RecipeVersion> RecipeVersions => Set<RecipeVersion>();
    public DbSet<RecipeItem> RecipeItems => Set<RecipeItem>();

    // Production & Batches
    public DbSet<ProductionBatch> ProductionBatches => Set<ProductionBatch>();
    public DbSet<ProductionConsumption> ProductionConsumptions => Set<ProductionConsumption>();

    // Waste & Quality Control
    public DbSet<WasteReason> WasteReasons => Set<WasteReason>();
    public DbSet<Waste> Wastes => Set<Waste>();
    public DbSet<QualityTemplate> QualityTemplates => Set<QualityTemplate>();
    public DbSet<QualityTemplateItem> QualityTemplateItems => Set<QualityTemplateItem>();
    public DbSet<QualityInspection> QualityInspections => Set<QualityInspection>();
    public DbSet<QualityInspectionItem> QualityInspectionItems => Set<QualityInspectionItem>();

    // Packaging
    public DbSet<PackagingBOM> PackagingBOMs => Set<PackagingBOM>();
    public DbSet<PackagingBOMVersion> PackagingBOMVersions => Set<PackagingBOMVersion>();
    public DbSet<PackagingItem> PackagingItems => Set<PackagingItem>();
    public DbSet<PackagingOrder> PackagingOrders => Set<PackagingOrder>();
    public DbSet<PackagingConsumption> PackagingConsumptions => Set<PackagingConsumption>();

    // Finished Goods
    public DbSet<FinishedGoodsStock> FinishedGoodsStocks => Set<FinishedGoodsStock>();
    public DbSet<FinishedGoodsRelease> FinishedGoodsReleases => Set<FinishedGoodsRelease>();

    // Inventory
    public DbSet<InventoryTransaction> InventoryTransactions => Set<InventoryTransaction>();
    public DbSet<StockBalance> StockBalances => Set<StockBalance>();

    // Purchasing
    public DbSet<SupplierCategory> SupplierCategories => Set<SupplierCategory>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<PurchaseRequest> PurchaseRequests => Set<PurchaseRequest>();
    public DbSet<PurchaseRequestItem> PurchaseRequestItems => Set<PurchaseRequestItem>();
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<PurchaseOrderItem> PurchaseOrderItems => Set<PurchaseOrderItem>();
    public DbSet<PurchaseReceipt> PurchaseReceipts => Set<PurchaseReceipt>();
    public DbSet<PurchaseReceiptItem> PurchaseReceiptItems => Set<PurchaseReceiptItem>();
    public DbSet<SupplierPriceHistory> SupplierPriceHistories => Set<SupplierPriceHistory>();

    // Sales
    // Sales & Invoicing (Phase 14 & Phase 15)
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<SalesOrder> SalesOrders => Set<SalesOrder>();
    public DbSet<SalesOrderItem> SalesOrderItems => Set<SalesOrderItem>();
    public DbSet<SalesFulfillment> SalesFulfillments => Set<SalesFulfillment>();
    public DbSet<SalesFulfillmentItem> SalesFulfillmentItems => Set<SalesFulfillmentItem>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceItem> InvoiceItems => Set<InvoiceItem>();
    public DbSet<Payment> Payments => Set<Payment>();

    // Accounting & General Ledger (Phase 16)
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<AccountingPeriod> AccountingPeriods => Set<AccountingPeriod>();
    public DbSet<JournalEntry> JournalEntries => Set<JournalEntry>();
    public DbSet<JournalEntryLine> JournalEntryLines => Set<JournalEntryLine>();
    public DbSet<SupplierPayment> SupplierPayments => Set<SupplierPayment>();
    public DbSet<AccountingSetting> AccountingSettings => Set<AccountingSetting>();

    // RBAC & Security (Phase 18)
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<UserWarehouse> UserWarehouses => Set<UserWarehouse>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<SecurityEvent> SecurityEvents => Set<SecurityEvent>();

    // Phase 19: System Administration & Configuration
    public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();
    public DbSet<CompanyProfile> CompanyProfiles => Set<CompanyProfile>();
    public DbSet<TaxSetting> TaxSettings => Set<TaxSetting>();
    public DbSet<DocumentNumberSetting> DocumentNumberSettings => Set<DocumentNumberSetting>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<decimal>().HavePrecision(18, 2);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        modelBuilder.Entity<SystemSetting>(entity =>
        {
            entity.HasIndex(e => e.Key).IsUnique();
        });

        modelBuilder.Entity<TaxSetting>(entity =>
        {
            entity.HasIndex(e => e.Code).IsUnique();
        });

        modelBuilder.Entity<DocumentNumberSetting>(entity =>
        {
            entity.HasIndex(e => e.DocumentType).IsUnique();
        });

        // Prevent SQL Server Error 1785 (multiple cascade paths) and protect ERP data integrity
        foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
        {
            relationship.DeleteBehavior = DeleteBehavior.Restrict;
        }
    }
}