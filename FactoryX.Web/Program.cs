using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using FactoryX.Infrastructure;
using FactoryX.Web.Services.Concretes;
using FactoryX.Web.Services.Abstracts;
using FactoryX.Web.Middlewares;
using FactoryX.Application.Services.Concretes;
using FactoryX.Application.Services.Abstracts;
using FactoryX.Application.Mappings;
using System.Diagnostics;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using FactoryX.Web.Services.Health;

using FluentValidation;
using FactoryX.Application.Validators;

var builder = WebApplication.CreateBuilder(args);

// MVC Services
builder.Services.AddControllersWithViews();

// Phase 20: Health Checks & Production Diagnostics
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy("Application process is running"), tags: new[] { "live" })
    .AddCheck<DatabaseHealthCheck>("database", tags: new[] { "ready" });


// Database Configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddInfrastructure(connectionString ?? "");
builder.Services.AddAutoMapper(typeof(MappingProfile));
builder.Services.AddValidatorsFromAssemblyContaining<CreateMaterialRequestValidator>();

// Application Services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IMachineService, MachineService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IProductCategoryService, ProductCategoryService>();
builder.Services.AddScoped<IWorkOrderService, WorkOrderService>();
builder.Services.AddScoped<IOperatorService, OperatorService>();
builder.Services.AddScoped<IProductionRecordService, ProductionRecordService>();
builder.Services.AddScoped<IShiftService, ShiftService>();
builder.Services.AddScoped<IWarehouseService, WarehouseService>();
builder.Services.AddScoped<IWarehouseLocationService, WarehouseLocationService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IMaterialCategoryService, MaterialCategoryService>();
builder.Services.AddScoped<IMaterialService, MaterialService>();
builder.Services.AddScoped<IRecipeService, RecipeService>();
builder.Services.AddScoped<IRecipeCostService, RecipeCostService>();
builder.Services.AddScoped<IProductionPlanningService, ProductionPlanningService>();
builder.Services.AddScoped<IProductionBatchService, ProductionBatchService>();
builder.Services.AddScoped<IProductionExecutionService, ProductionExecutionService>();
builder.Services.AddScoped<IWasteService, WasteService>();
builder.Services.AddScoped<IWasteReasonService, WasteReasonService>();
builder.Services.AddScoped<IQualityTemplateService, QualityTemplateService>();
builder.Services.AddScoped<IQualityInspectionService, QualityInspectionService>();
builder.Services.AddScoped<IQualityGateService, QualityGateService>();
builder.Services.AddScoped<IPackagingCostService, PackagingCostService>();
builder.Services.AddScoped<IPackagingBOMService, PackagingBOMService>();
builder.Services.AddScoped<IPackagingOrderService, PackagingOrderService>();
builder.Services.AddScoped<IFinishedGoodsService, FinishedGoodsService>();
builder.Services.AddScoped<IFinishedGoodsReleaseService, FinishedGoodsReleaseService>();
builder.Services.AddScoped<ISupplierService, SupplierService>();
builder.Services.AddScoped<IPurchaseRequestService, PurchaseRequestService>();
builder.Services.AddScoped<IPurchaseOrderService, PurchaseOrderService>();
builder.Services.AddScoped<IPurchaseReceiptService, PurchaseReceiptService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<ISalesOrderService, SalesOrderService>();
builder.Services.AddScoped<ISalesFulfillmentService, SalesFulfillmentService>();
builder.Services.AddScoped<IInvoiceService, InvoiceService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<ICustomerStatementService, CustomerStatementService>();

// Phase 16: Accounting & General Ledger
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IAccountingPeriodService, AccountingPeriodService>();
builder.Services.AddScoped<IJournalEntryService, JournalEntryService>();
builder.Services.AddScoped<IAccountingPostingService, AccountingPostingService>();
builder.Services.AddScoped<IGeneralLedgerService, GeneralLedgerService>();
builder.Services.AddScoped<ISupplierPaymentService, SupplierPaymentService>();
builder.Services.AddScoped<IAccountingDashboardService, AccountingDashboardService>();

// Phase 17: Reporting & Analytics
builder.Services.AddScoped<IReportingService, ReportingService>();

// Phase 18: Security, RBAC & Audit Trail
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<IWarehouseAccessService, WarehouseAccessService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IUserAdminService, UserAdminService>();
builder.Services.AddScoped<IRoleService, RoleService>();

// Phase 19: System Administration & Configuration
builder.Services.AddScoped<ISettingsService, SettingsService>();

builder.Services.AddScoped<IServiceManager, ServiceManager>();

// Web Services
builder.Services.AddScoped<IFirstVisitService, FirstVisitService>();

// Session Configuration
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment() ? CookieSecurePolicy.None : CookieSecurePolicy.Always;
});

// Authentication and Authorization Configuration
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login";
        options.LogoutPath = "/Logout";
        options.AccessDeniedPath = "/Login";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Auto-migrate database on startup if database server is available
using (var scope = app.Services.CreateScope())
{
    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        try
        {
            dbContext.Database.Migrate();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Database Migration Notice] {ex.Message}");
        }

        // Seed Phase 18 Permissions & Roles
        var permService = scope.ServiceProvider.GetRequiredService<IPermissionService>();
        permService.SeedDefaultPermissionsAndRolesAsync().GetAwaiter().GetResult();

        // Seed Phase 19 Default System Configuration
        var settingsService = scope.ServiceProvider.GetRequiredService<ISettingsService>();
        settingsService.SeedDefaultConfigurationAsync().GetAwaiter().GetResult();


        // Seed Standard Waste Reasons if empty
        if (!dbContext.WasteReasons.Any())
        {
            dbContext.WasteReasons.AddRange(
                new FactoryX.Domain.Entities.WasteReason { Code = "WR-SPIL", Reason = "انسكاب أو تلف أثناء التداول", Description = "فقد أو تلف الخامات أثناء النقل اليدوي والتداول في صالة الإنتاج", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new FactoryX.Domain.Entities.WasteReason { Code = "WR-BURN", Reason = "احتراق أو زيادة طهي السكر والعسل", Description = "تغير في قوام أو احتراق خلطة الحلاوة أثناء الغلي والطهي بالقدور", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new FactoryX.Domain.Entities.WasteReason { Code = "WR-EQFL", Reason = "عطل في المعدات أو الماكينات", Description = "توقف مفاجئ في ماكينات التشكيل أو الفرد أو الدرافيل أثناء التشغيل", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new FactoryX.Domain.Entities.WasteReason { Code = "WR-EXPR", Reason = "انتهاء فترة الصلاحية", Description = "خامات مخزنة تجاوزت تاريخ الصلاحية المقرر", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new FactoryX.Domain.Entities.WasteReason { Code = "WR-CONT", Reason = "تلوث أو شوائب بالخامات", Description = "وجود شوائب أو تغير في الخواص الظاهرية للخامات قبل أو أثناء التصنيع", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new FactoryX.Domain.Entities.WasteReason { Code = "WR-PRDL", Reason = "فاقد تصنيع طبيعي ومتبقيات", Description = "متبقيات طبيعية على جدران القدور ومعدات الفرد والتقطيع", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new FactoryX.Domain.Entities.WasteReason { Code = "WR-SHPE", Reason = "عيوب في القولبة والتقطيع", Description = "قطع حلويات مكسورة أو غير مطابقة للوزن والشكل القياسي", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new FactoryX.Domain.Entities.WasteReason { Code = "WR-OTHR", Reason = "أسباب تشغيلية أخرى", Description = "أسباب متنوعة غير مدرجة في التصنيفات القياسية", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
            );
            dbContext.SaveChanges();
        }

        // Seed Standard Quality Control Templates if empty
        if (!dbContext.QualityTemplates.Any())
        {
            var sesameTemplate = new FactoryX.Domain.Entities.QualityTemplate
            {
                Code = "SESAME-QC-01",
                Name = "المواصفة القياسية لجودة السمسمية الفاخرة",
                Description = "قالب الفحص المعتمد لتقييم دفعات إنتاج حلاوة السمسمية وتحديد مطابقتها للإفراج",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Items = new List<FactoryX.Domain.Entities.QualityTemplateItem>
                {
                    new() { SpecificationName = "الوزن الصافي للقطعة / العبوة", Description = "وزن القطعة الواحدة بالجرام", Sequence = 1, IsRequired = true, DataType = FactoryX.Domain.Entities.InspectionDataType.Number, MinValue = 480m, MaxValue = 520m, TargetValue = 500m, Unit = "G", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new() { SpecificationName = "المظهر واللون والقرمشة", Description = "لون ذهبي متجانس ولمعان السكر بدون احتراق", Sequence = 2, IsRequired = true, DataType = FactoryX.Domain.Entities.InspectionDataType.PassFail, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new() { SpecificationName = "القوام والصلابة (Texture)", Description = "قوام مقرمش غير صلب بشكل مفرط وغير لاصق", Sequence = 3, IsRequired = true, DataType = FactoryX.Domain.Entities.InspectionDataType.PassFail, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new() { SpecificationName = "الطعم والنكهة والرائحة", Description = "طعم السمسم المحمص الطازج وخلوه من التزنخ", Sequence = 4, IsRequired = true, DataType = FactoryX.Domain.Entities.InspectionDataType.PassFail, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new() { SpecificationName = "نسبة الرطوبة (Moisture %)", Description = "نسبة الرطوبة في المنتج النهائي", Sequence = 5, IsRequired = false, DataType = FactoryX.Domain.Entities.InspectionDataType.Number, MinValue = 1m, MaxValue = 6m, TargetValue = 3.5m, Unit = "%", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new() { SpecificationName = "فحص الشوائب والأجسام الغريبة", Description = "خلو تام من أي شوائب أو مواد غير غذائية", Sequence = 6, IsRequired = true, DataType = FactoryX.Domain.Entities.InspectionDataType.PassFail, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
                }
            };

            var malbanTemplate = new FactoryX.Domain.Entities.QualityTemplate
            {
                Code = "MALBAN-QC-01",
                Name = "المواصفة القياسية لجودة الملبن وعين الجمل",
                Description = "قالب الفحص المعتمد لتقييم دفعات حبل الملبن والملبن السادة والمحشو",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Items = new List<FactoryX.Domain.Entities.QualityTemplateItem>
                {
                    new() { SpecificationName = "الوزن الصافي للقطعة", Description = "وزن القطعة القياسي بالجرام", Sequence = 1, IsRequired = true, DataType = FactoryX.Domain.Entities.InspectionDataType.Number, MinValue = 240m, MaxValue = 260m, TargetValue = 250m, Unit = "G", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new() { SpecificationName = "مرونة ونعومة الملبن (Elasticity)", Description = "قوام مطاطي ناعم غير جاف وبدون تكتلات نشا", Sequence = 2, IsRequired = true, DataType = FactoryX.Domain.Entities.InspectionDataType.PassFail, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new() { SpecificationName = "الشفافية والتوزيع الداخلي للمكسرات", Description = "توزيع متجانس لعين الجمل والمكسرات داخل حبل الملبن", Sequence = 3, IsRequired = true, DataType = FactoryX.Domain.Entities.InspectionDataType.PassFail, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new() { SpecificationName = "درجة السكر ومذاق ماء الورد", Description = "حلاوة متوازنة مع نكهة ماء الورد الطبيعي", Sequence = 4, IsRequired = true, DataType = FactoryX.Domain.Entities.InspectionDataType.PassFail, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
                }
            };

            dbContext.QualityTemplates.AddRange(sesameTemplate, malbanTemplate);
            dbContext.SaveChanges();
        }

        // Seed Packaging Materials & Standard BOMs if not exist
        if (!dbContext.PackagingBOMs.Any())
        {
            var pkgCategory = dbContext.MaterialCategories.FirstOrDefault(c => c.CategoryType == FactoryX.Domain.Entities.MaterialCategoryType.PackagingMaterial || c.Name.Contains("تعبئة"));
            if (pkgCategory == null)
            {
                pkgCategory = new FactoryX.Domain.Entities.MaterialCategory
                {
                    Name = "مواد التعبئة والتغليف",
                    Code = "CAT-PKG",
                    Description = "خامات ومواد التعبئة والتغليف والعلب والملصقات",
                    CategoryType = FactoryX.Domain.Entities.MaterialCategoryType.PackagingMaterial,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                dbContext.MaterialCategories.Add(pkgCategory);
                dbContext.SaveChanges();
            }

            var defaultWarehouse = dbContext.Warehouses.FirstOrDefault(w => w.IsActive) ?? new FactoryX.Domain.Entities.Warehouse { Name = "مستودع الخامات والتعبئة الرئيسي", Code = "WH-PKG-01", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
            if (defaultWarehouse.Id == 0)
            {
                dbContext.Warehouses.Add(defaultWarehouse);
                dbContext.SaveChanges();
            }

            // Packaging Materials
            var box500 = dbContext.Materials.FirstOrDefault(m => m.Code == "PKG-BOX-500") ?? new FactoryX.Domain.Entities.Material
            {
                Name = "علبة كرتون حلويات 500 جرام فاخرة",
                ArabicName = "علبة كرتون فاخرة 500 جم",
                Code = "PKG-BOX-500",
                SKU = "SKU-PKG-BOX-500",
                MaterialCategoryId = pkgCategory.Id,
                IsPackagingMaterial = true,
                PackagingType = FactoryX.Domain.Entities.PackagingMaterialType.Box,
                Unit = "Pcs",
                PurchaseUnit = "Box",
                ConversionFactor = 1,
                CurrentStock = 5000,
                StandardCost = 3.50m,
                CurrentCost = 3.50m,
                WarehouseId = defaultWarehouse.Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var bagInner = dbContext.Materials.FirstOrDefault(m => m.Code == "PKG-BAG-INN") ?? new FactoryX.Domain.Entities.Material
            {
                Name = "كيس سلوفان غذائي داخلي معقم",
                ArabicName = "كيس سلوفان داخلي",
                Code = "PKG-BAG-INN",
                SKU = "SKU-PKG-BAG-INN",
                MaterialCategoryId = pkgCategory.Id,
                IsPackagingMaterial = true,
                PackagingType = FactoryX.Domain.Entities.PackagingMaterialType.PlasticBag,
                Unit = "Pcs",
                PurchaseUnit = "Pack",
                ConversionFactor = 1,
                CurrentStock = 10000,
                StandardCost = 0.75m,
                CurrentCost = 0.75m,
                WarehouseId = defaultWarehouse.Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var label = dbContext.Materials.FirstOrDefault(m => m.Code == "PKG-LBL-PRD") ?? new FactoryX.Domain.Entities.Material
            {
                Name = "ملصق بيانات المنتج الغذائي والباركود",
                ArabicName = "ملصق بيانات وتغذية",
                Code = "PKG-LBL-PRD",
                SKU = "SKU-PKG-LBL-PRD",
                MaterialCategoryId = pkgCategory.Id,
                IsPackagingMaterial = true,
                PackagingType = FactoryX.Domain.Entities.PackagingMaterialType.Label,
                Unit = "Pcs",
                PurchaseUnit = "Roll",
                ConversionFactor = 1,
                CurrentStock = 10000,
                StandardCost = 0.35m,
                CurrentCost = 0.35m,
                WarehouseId = defaultWarehouse.Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var sticker = dbContext.Materials.FirstOrDefault(m => m.Code == "PKG-STK-SEC") ?? new FactoryX.Domain.Entities.Material
            {
                Name = "استيكر ضمان الجودة والغلق الأمني",
                ArabicName = "استيكر أمان وضمان",
                Code = "PKG-STK-SEC",
                SKU = "SKU-PKG-STK-SEC",
                MaterialCategoryId = pkgCategory.Id,
                IsPackagingMaterial = true,
                PackagingType = FactoryX.Domain.Entities.PackagingMaterialType.Sticker,
                Unit = "Pcs",
                PurchaseUnit = "Roll",
                ConversionFactor = 1,
                CurrentStock = 10000,
                StandardCost = 0.20m,
                CurrentCost = 0.20m,
                WarehouseId = defaultWarehouse.Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            if (box500.Id == 0) dbContext.Materials.Add(box500);
            if (bagInner.Id == 0) dbContext.Materials.Add(bagInner);
            if (label.Id == 0) dbContext.Materials.Add(label);
            if (sticker.Id == 0) dbContext.Materials.Add(sticker);
            dbContext.SaveChanges();

            // Seed initial StockBalances for packaging materials if empty
            if (!dbContext.StockBalances.Any(sb => sb.MaterialId == box500.Id))
            {
                dbContext.StockBalances.AddRange(
                    new FactoryX.Domain.Entities.StockBalance { WarehouseId = defaultWarehouse.Id, MaterialId = box500.Id, Quantity = 5000, Unit = "Pcs", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new FactoryX.Domain.Entities.StockBalance { WarehouseId = defaultWarehouse.Id, MaterialId = bagInner.Id, Quantity = 10000, Unit = "Pcs", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new FactoryX.Domain.Entities.StockBalance { WarehouseId = defaultWarehouse.Id, MaterialId = label.Id, Quantity = 10000, Unit = "Pcs", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new FactoryX.Domain.Entities.StockBalance { WarehouseId = defaultWarehouse.Id, MaterialId = sticker.Id, Quantity = 10000, Unit = "Pcs", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
                );
                dbContext.SaveChanges();
            }

            var sesameProduct = dbContext.Products.FirstOrDefault(p => p.Code == "PRD-SES-001" || p.Name.Contains("سمسمية")) ?? dbContext.Products.FirstOrDefault();
            if (sesameProduct != null)
            {
                var bomSesame = new FactoryX.Domain.Entities.PackagingBOM
                {
                    Code = "SES-500-PKG",
                    Name = "مواصفة تعبئة وتغليف علبة سمسمية فاخرة 500 جم",
                    ProductId = sesameProduct.Id,
                    PackSize = 0.50m,
                    PackSizeKg = 0.50m,
                    PackUnit = "Box",
                    OutputProductQuantity = 0.50m,
                    Unit = "Box",
                    Description = "تعبئة السمسمية في كيس سلوفان ثم علبة كرتون 500 جم مع ملصق التغذية واستيكر الأمان",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Versions = new List<FactoryX.Domain.Entities.PackagingBOMVersion>
                    {
                        new()
                        {
                            VersionNumber = 1,
                            VersionName = "الإصدار القياسي v1",
                            EffectiveFrom = DateTime.UtcNow.AddYears(-1),
                            Status = FactoryX.Domain.Entities.PackagingBOMStatus.Active,
                            Notes = "المواصفة المعتمدة لتعبئة السمسمية 500 جم",
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow,
                            Items = new List<FactoryX.Domain.Entities.PackagingItem>
                            {
                                new() { MaterialId = box500.Id, QuantityRequired = 1, Unit = "Pcs", Sequence = 1, IsOptional = false, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                                new() { MaterialId = bagInner.Id, QuantityRequired = 1, Unit = "Pcs", Sequence = 2, IsOptional = false, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                                new() { MaterialId = label.Id, QuantityRequired = 1, Unit = "Pcs", Sequence = 3, IsOptional = false, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                                new() { MaterialId = sticker.Id, QuantityRequired = 1, Unit = "Pcs", Sequence = 4, IsOptional = false, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
                            }
                        }
                    }
                };

                dbContext.PackagingBOMs.Add(bomSesame);
                dbContext.SaveChanges();
            }
        }

        // Seed Finished Goods Warehouses & Locations if not present
        var fgWh = dbContext.Warehouses.FirstOrDefault(w => w.Type == FactoryX.Domain.Entities.WarehouseType.FinishedGoods);
        if (fgWh == null)
        {
            fgWh = new FactoryX.Domain.Entities.Warehouse
            {
                Code = "WH-FG-01",
                Name = "المستودع الرئيسي للمنتجات التامة",
                Type = FactoryX.Domain.Entities.WarehouseType.FinishedGoods,
                Description = "المبنى الرئيسي - الجناح الشرقي - صالة المنتجات التامة",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Locations = new List<FactoryX.Domain.Entities.WarehouseLocation>
                {
                    new() { Code = "LOC-FG-A1", Name = "قسم الحلويات الجافة - استاند A1", Section = "الجناح الشرقي", Description = "سعة تخزينية 10,000 كجم", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new() { Code = "LOC-FG-B1", Name = "قسم الملبن والمحشوات - استاند B1", Section = "الجناح الشمالي", Description = "سعة تخزينية 10,000 كجم", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
                }
            };
            dbContext.Warehouses.Add(fgWh);
            dbContext.SaveChanges();
        }

        // Seed Supplier Categories
        if (!dbContext.SupplierCategories.Any())
        {
            var catRaw = new FactoryX.Domain.Entities.SupplierCategory { Code = "CAT-SUP-RAW", Name = "موردو الخامات والمواد الأولية", Description = "موردو السكر، المكسرات، الجلوكوز، السمسم، والحبوب", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
            var catPkg = new FactoryX.Domain.Entities.SupplierCategory { Code = "CAT-SUP-PKG", Name = "موردو مواد التعبئة والتغليف", Description = "موردو الأكياس، الكراتين، الأشرطة، والعوازل", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
            var catLoc = new FactoryX.Domain.Entities.SupplierCategory { Code = "CAT-SUP-LOC", Name = "موردون محليون", Description = "موردون من السوق المحلي المصري", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
            var catImp = new FactoryX.Domain.Entities.SupplierCategory { Code = "CAT-SUP-IMP", Name = "موردو استيراد", Description = "شركات وموردون دوليون للمكسرات والخامات المستوردة", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };

            dbContext.SupplierCategories.AddRange(catRaw, catPkg, catLoc, catImp);
            dbContext.SaveChanges();

            if (!dbContext.Suppliers.Any())
            {
                dbContext.Suppliers.AddRange(
                    new FactoryX.Domain.Entities.Supplier
                    {
                        Code = "SUP-0001",
                        Name = "شركة الدلتا للصناعات السكرية والخامات الغذائية",
                        ArabicName = "شركة الدلتا للسكر",
                        ContactPerson = "م. حسام البحيري",
                        Phone = "01012345678",
                        Email = "delta.sugar@example.com",
                        Address = "المنطقة الصناعية، العاشر من رمضان",
                        TaxNumber = "100-234-567",
                        CategoryId = catRaw.Id,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new FactoryX.Domain.Entities.Supplier
                    {
                        Code = "SUP-0002",
                        Name = "الشركة الوطنية للحبوب والزيوت والمكسرات",
                        ArabicName = "الوطنية للمكسرات والخامات",
                        ContactPerson = "أ. عصام غنيم",
                        Phone = "01234567890",
                        Email = "nuts.national@example.com",
                        Address = "طريق مصر الإسكندرية الصحراوي",
                        TaxNumber = "200-345-678",
                        CategoryId = catRaw.Id,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new FactoryX.Domain.Entities.Supplier
                    {
                        Code = "SUP-0003",
                        Name = "مؤسسة الأهرام لطباعة ومواد التعبئة والتغليف",
                        ArabicName = "الأهرام لمواد التعبئة",
                        ContactPerson = "أ. مروان شريف",
                        Phone = "01198765432",
                        Email = "ahram.pack@example.com",
                        Address = "المنطقة الصناعية الثالثة، 6 أكتوبر",
                        TaxNumber = "300-456-789",
                        CategoryId = catPkg.Id,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    }
                );
                dbContext.SaveChanges();
            }

            // Seed Phase 16 Chart of Accounts & Open Period
            var accountService = scope.ServiceProvider.GetRequiredService<IAccountService>();
            accountService.SeedDefaultChartOfAccountsAsync().GetAwaiter().GetResult();

            var periodService = scope.ServiceProvider.GetRequiredService<IAccountingPeriodService>();
            periodService.EnsureOpenPeriodExistsAsync().GetAwaiter().GetResult();
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Database Migration Notice] Database migration could not be run automatically: {ex.Message}");
    }
}

// Exception Handling
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

//Security & Static Files
app.UseHttpsRedirection();
app.UseStaticFiles();

// Routing
app.UseRouting();

// Session & Authentication
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<FirstVisitMiddleware>();

// Phase 20: Health Check Endpoints
app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("live"),
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json; charset=utf-8";
        var uptime = Process.GetCurrentProcess().StartTime;
        var response = new
        {
            status = report.Status.ToString(),
            application = FactoryX.Application.Common.SystemVersionInfo.ReleaseName,
            version = FactoryX.Application.Common.SystemVersionInfo.Version,
            timestampUtc = DateTime.UtcNow.ToString("o"),
            uptimeSeconds = Math.Round((DateTime.UtcNow - uptime.ToUniversalTime()).TotalSeconds, 1)
        };
        await context.Response.WriteAsJsonAsync(response);
    }
});

app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json; charset=utf-8";
        var response = new
        {
            status = report.Status.ToString(),
            application = FactoryX.Application.Common.SystemVersionInfo.ReleaseName,
            version = FactoryX.Application.Common.SystemVersionInfo.Version,
            timestampUtc = DateTime.UtcNow.ToString("o"),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                durationMs = Math.Round(e.Value.Duration.TotalMilliseconds, 1),
                data = e.Value.Data
            })
        };
        await context.Response.WriteAsJsonAsync(response);
    }
});

// Custom Routes
app.MapControllerRoute(
    name: "login",
    pattern: "login",
    defaults: new { controller = "Account", action = "Login" });


app.MapControllerRoute(
    name: "register",
    pattern: "register",
    defaults: new { controller = "Account", action = "Register" });

// Default Route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();