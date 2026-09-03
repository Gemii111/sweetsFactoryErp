using FactoryX.Application.DTOs;
using FactoryX.Application.Services.Abstracts;
using FactoryX.Domain.Entities;
using FactoryX.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FactoryX.Application.Services.Concretes;

public class PermissionService : IPermissionService
{
    private readonly AppDbContext _context;

    public PermissionService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> HasPermissionAsync(int userId, string permissionCode)
    {
        var user = await _context.Users.AsNoTracking()
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r!.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null || !user.IsActive) return false;

        // Check if locked
        if (user.LockedUntil.HasValue && user.LockedUntil.Value > DateTime.UtcNow) return false;

        // Legacy role check or Super Admin check
        if (user.Role == "Admin" || user.Role == "Super Admin" || user.Role == "SUPER_ADMIN")
            return true;

        if (user.UserRoles != null)
        {
            foreach (var ur in user.UserRoles)
            {
                if (ur.Role == null || !ur.Role.IsActive) continue;
                if (ur.Role.Code == "SUPER_ADMIN" || ur.Role.Name == "Super Admin" || ur.Role.Name == "Admin")
                    return true;

                if (ur.Role.RolePermissions != null)
                {
                    if (ur.Role.RolePermissions.Any(rp => rp.Permission != null && rp.Permission.Code == permissionCode))
                        return true;
                }
            }
        }

        return false;
    }

    public async Task<List<string>> GetUserPermissionCodesAsync(int userId)
    {
        var user = await _context.Users.AsNoTracking()
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r!.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null || !user.IsActive) return new List<string>();

        if (user.Role == "Admin" || user.Role == "Super Admin" || user.Role == "SUPER_ADMIN" ||
            user.UserRoles.Any(ur => ur.Role != null && (ur.Role.Code == "SUPER_ADMIN" || ur.Role.Name == "Super Admin")))
        {
            return await _context.Permissions.Select(p => p.Code).ToListAsync();
        }

        var codes = user.UserRoles
            .Where(ur => ur.Role != null && ur.Role.IsActive)
            .SelectMany(ur => ur.Role!.RolePermissions)
            .Where(rp => rp.Permission != null)
            .Select(rp => rp.Permission!.Code)
            .Distinct()
            .ToList();

        return codes;
    }

    public async Task<List<PermissionItemDto>> GetAllPermissionsAsync()
    {
        return await _context.Permissions.AsNoTracking()
            .OrderBy(p => p.Module).ThenBy(p => p.Code)
            .Select(p => new PermissionItemDto
            {
                Id = p.Id,
                Code = p.Code,
                Name = p.Name,
                Module = p.Module,
                Action = p.Action,
                Description = p.Description
            }).ToListAsync();
    }

    public async Task<RolePermissionMatrixDto> GetRolePermissionMatrixAsync(int roleId)
    {
        var role = await _context.Roles.AsNoTracking()
            .Include(r => r.RolePermissions)
            .FirstOrDefaultAsync(r => r.Id == roleId);

        if (role == null) throw new InvalidOperationException("Role not found");

        var assignedPermissionIds = role.RolePermissions.Select(rp => rp.PermissionId).ToHashSet();
        var allPermissions = await _context.Permissions.AsNoTracking().ToListAsync();

        var matrix = new RolePermissionMatrixDto
        {
            RoleId = role.Id,
            RoleName = role.Name,
            RoleDisplayName = string.IsNullOrWhiteSpace(role.DisplayName) ? role.Name : role.DisplayName,
            RoleCode = role.Code
        };

        var grouped = allPermissions
            .GroupBy(p => p.Module)
            .ToDictionary(
                g => g.Key,
                g => g.Select(p => new PermissionItemDto
                {
                    Id = p.Id,
                    Code = p.Code,
                    Name = p.Name,
                    Module = p.Module,
                    Action = p.Action,
                    Description = p.Description,
                    IsAssigned = assignedPermissionIds.Contains(p.Id)
                }).ToList()
            );

        matrix.PermissionsByModule = grouped;
        return matrix;
    }

    public async Task UpdateRolePermissionsAsync(int roleId, List<int> permissionIds)
    {
        var role = await _context.Roles
            .Include(r => r.RolePermissions)
            .FirstOrDefaultAsync(r => r.Id == roleId);

        if (role == null) throw new InvalidOperationException("Role not found");

        _context.RolePermissions.RemoveRange(role.RolePermissions);

        var validPermissionIds = await _context.Permissions
            .Where(p => permissionIds.Contains(p.Id))
            .Select(p => p.Id)
            .ToListAsync();

        foreach (var pid in validPermissionIds)
        {
            _context.RolePermissions.Add(new RolePermission
            {
                RoleId = role.Id,
                PermissionId = pid,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync();
    }

    public async Task SeedDefaultPermissionsAndRolesAsync()
    {
        var permissionsCatalog = GetSystemPermissionsCatalog();
        foreach (var p in permissionsCatalog)
        {
            var existing = await _context.Permissions.FirstOrDefaultAsync(x => x.Code == p.Code);
            if (existing == null)
            {
                _context.Permissions.Add(new Permission
                {
                    Code = p.Code,
                    Name = p.Name,
                    Module = p.Module,
                    Action = p.Action,
                    Description = p.Description,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }
            else
            {
                existing.Name = p.Name;
                existing.Module = p.Module;
                existing.Action = p.Action;
                existing.Description = p.Description;
            }
        }
        await _context.SaveChangesAsync();

        var rolesCatalog = GetDefaultRolesCatalog();
        foreach (var r in rolesCatalog)
        {
            var existingRole = await _context.Roles.Include(x => x.RolePermissions).FirstOrDefaultAsync(x => x.Code == r.Code || x.Name == r.Name);
            if (existingRole == null)
            {
                existingRole = new Role
                {
                    Name = r.Name,
                    Code = r.Code,
                    DisplayName = r.DisplayName,
                    Description = r.Description,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.Roles.Add(existingRole);
                await _context.SaveChangesAsync();
            }
            else
            {
                existingRole.DisplayName = r.DisplayName;
                existingRole.Description = r.Description;
                existingRole.IsActive = true;
                await _context.SaveChangesAsync();
            }

            // Assign default permissions if empty or Super Admin
            if (!existingRole.RolePermissions.Any())
            {
                var grantedCodes = r.DefaultPermissionCodes;
                var pids = await _context.Permissions
                    .Where(p => grantedCodes.Contains(p.Code) || r.Code == "SUPER_ADMIN")
                    .Select(p => p.Id)
                    .ToListAsync();

                foreach (var pid in pids)
                {
                    _context.RolePermissions.Add(new RolePermission
                    {
                        RoleId = existingRole.Id,
                        PermissionId = pid,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                }
                await _context.SaveChangesAsync();
            }
        }

        // Ensure Admin user has SUPER_ADMIN role and valid password hash
        var adminUser = await _context.Users.Include(u => u.UserRoles).FirstOrDefaultAsync(u => u.Username == "testadmin" || u.Username == "admin");
        var superAdminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Code == "SUPER_ADMIN");
        if (adminUser == null)
        {
            adminUser = new User
            {
                Username = "testadmin",
                FullName = "مدير النظام العام",
                Email = "admin@factoryx.com",
                PasswordHash = FactoryX.Application.Helpers.PasswordHasher.HashPassword("Password123!"),
                Role = "Super Admin",
                IsActive = true,
                IsAllWarehousesAllowed = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.Users.Add(adminUser);
            await _context.SaveChangesAsync();
        }
        else
        {
            adminUser.IsActive = true;
            adminUser.FailedLoginCount = 0;
            adminUser.LockedUntil = null;
            adminUser.PasswordHash = FactoryX.Application.Helpers.PasswordHasher.HashPassword("Password123!");
            adminUser.Role = "Super Admin";
            adminUser.IsAllWarehousesAllowed = true;
        }

        if (superAdminRole != null && !adminUser.UserRoles.Any(ur => ur.RoleId == superAdminRole.Id))
        {
            adminUser.UserRoles.Add(new UserRole
            {
                UserId = adminUser.Id,
                RoleId = superAdminRole.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }
        await _context.SaveChangesAsync();
    }

    private static List<PermissionDefinition> GetSystemPermissionsCatalog()
    {
        return new List<PermissionDefinition>
        {
            // Sales
            new("Sales.Order.View", "عرض أوامر البيع", "المبيعات", "View", "عرض واستعراض أوامر المبيعات"),
            new("Sales.Order.Create", "إنشاء أمر بيع", "المبيعات", "Create", "إضافة طلب بيع جديد"),
            new("Sales.Order.Edit", "تعديل أمر بيع مسودة", "المبيعات", "Edit", "تعديل بنود أمر البيع المسودة"),
            new("Sales.Order.Confirm", "تأكيد أمر البيع", "المبيعات", "Approve", "تأكيد أمر البيع وحجز المخزون"),
            new("Sales.Fulfillment.Create", "إنشاء سند تسليم وشحن", "المبيعات", "Create", "إنشاء إذن تسليم مبيعات"),
            new("Sales.Fulfillment.Execute", "تنفيذ الشحن والخصم المخزني", "المبيعات", "Execute", "صرف بضاعة تامة للعميل"),
            new("Sales.Customer.Manage", "إدارة العملاء", "المبيعات", "Manage", "إضافة وتعديل بيانات العملاء والحد الائتماني"),

            // Purchasing
            new("Purchasing.PR.Create", "إنشاء طلب شراء", "المشتريات", "Create", "تقديم طلب شراء خامات ومواد تعبئة"),
            new("Purchasing.PO.Create", "إنشاء أمر شراء", "المشتريات", "Create", "إصدار أمر شراء رسمي للمورد"),
            new("Purchasing.PO.Approve", "اعتماد أمر الشراء", "المشتريات", "Approve", "اعتماد أمر الشراء والالتزام المالي"),
            new("Purchasing.Receipt.Create", "استلام فحص وتوريد (GRN)", "المشتريات", "Execute", "تسجيل سند استلام وتوريد مخزني"),
            new("Purchasing.Supplier.Manage", "إدارة الموردين", "المشتريات", "Manage", "إضافة وتعديل بيانات الموردين والأسعار"),

            // Inventory
            new("Inventory.Stock.View", "عرض أرصدة المخزون", "المخزون", "View", "استعراض كميات وأرصدة المستودعات"),
            new("Inventory.Stock.Adjust", "تسوية مخزنية", "المخزون", "Execute", "تسجيل تسوية جردية بالزيادة أو العجز"),
            new("Inventory.Transfer.Create", "تحويل مخزني", "المخزون", "Execute", "نقل بضاعة بين المستودعات والمواقع"),
            new("Inventory.Valuation.View", "عرض تقييم المخزون المالي", "المخزون", "View", "الاطلاع على القيمة المالية للمخزون"),
            new("Inventory.Warehouse.Manage", "إدارة المستودعات والمواقع", "المخزون", "Manage", "تهيئة المستودعات وأماكن التخزين"),

            // Production
            new("Production.Order.Create", "إنشاء أمر إنتاج وتخطيط", "الإنتاج", "Create", "تخطيط وجدولة أوامر الإنتاج"),
            new("Production.Order.Release", "إطلاق أمر الإنتاج للتشغيل", "الإنتاج", "Approve", "إطلاق أوامر التشغيل لصالة المصنع"),
            new("Production.Batch.Start", "بدء تشغيل الدفعة", "الإنتاج", "Execute", "بدء وصرف خامات تشغيل الدفعة"),
            new("Production.Batch.Complete", "إنهاء وتسجيل إنتاج الدفعة", "الإنتاج", "Execute", "تسجيل الكمية التامة الفعلية"),
            new("Production.Recipe.Manage", "إدارة وصفات المنتجات (BOM)", "الإنتاج", "Manage", "إعداد وتعديل تركيبات الحلويات"),

            // Waste
            new("Waste.Create", "تسجيل هالك وتالف", "الهوالك", "Create", "تسجيل محضر هالك مواد خام أو إنتاج"),
            new("Waste.Approve", "اعتماد وتسوية الهالك", "الهوالك", "Approve", "اعتماد خصم الهالك محاسبياً ومخزنياً"),
            new("Waste.Reject", "رفض محضر الهالك", "الهوالك", "Approve", "رفض محضر الهالك وإلغائه"),

            // QC
            new("QC.Inspection.Create", "إنشاء فحص جودة", "الجودة", "Create", "تسجيل اختبار وفحص عينات الجودة"),
            new("QC.Inspection.Approve", "اعتماد وإفراج جودة", "الجودة", "Approve", "منح الموافقة والإفراج عن التشغيلة"),
            new("QC.Inspection.Reject", "رفض جودة وحظر الدفعة", "الجودة", "Approve", "رفض الدفعة ومنع تداولها"),
            new("QC.Template.Manage", "إدارة قوالب ومعايير الجودة", "الجودة", "Manage", "تحديد معايير ومواصفات الفحص"),

            // Packaging
            new("Packaging.Order.Create", "إنشاء أمر تعبئة وتغليف", "التعبئة", "Create", "إصدار أمر تعبئة وتغليف"),
            new("Packaging.Order.Execute", "تنفيذ وصرف مواد التعبئة", "التعبئة", "Execute", "صرف العلب ومواد التغليف والتعبئة"),
            new("Packaging.BOM.Manage", "إدارة مواصفات التعبئة", "التعبئة", "Manage", "تحديد معايير التعبئة والعلب"),

            // Finished Goods
            new("FinishedGoods.Stock.View", "عرض مخزون الإنتاج التام", "المنتجات التامة", "View", "عرض مخزون الحلوى الجاهزة للبيع"),
            new("FinishedGoods.Release", "إصدار سند إفراج منتج تام", "المنتجات التامة", "Approve", "إفراج ونقل الحلوى لمخزن البيع"),

            // Accounting
            new("Accounting.Journal.View", "عرض القيود المحاسبية", "الحسابات", "View", "الاطلاع على اليومية العامة والأستاذ"),
            new("Accounting.Journal.Create", "إنشاء قيد يدوي", "الحسابات", "Create", "إضافة قيد تسوية أو يومية يدوي"),
            new("Accounting.Journal.Post", "ترحيل القيود للحسابات", "الحسابات", "Post", "ترحيل القيد المالي لدفتر الأستاذ"),
            new("Accounting.Journal.Reverse", "عكس قيد مرحل", "الحسابات", "Reverse", "إلغاء وعكس قيد مالي مرحل"),
            new("Accounting.Period.Manage", "إدارة الفترات المالية", "الحسابات", "Manage", "فتح وإغلاق الفترات المالية"),
            new("Invoice.Create", "إنشاء فاتورة مبيعات", "الحسابات", "Create", "إنشاء فاتورة ضريبية للعميل"),
            new("Invoice.Issue", "إصدار واعتماد الفاتورة", "الحسابات", "Approve", "إصدار رسمي للفاتورة وترحيلها"),
            new("Invoice.Cancel", "إلغاء فاتورة", "الحسابات", "Approve", "إلغاء الفاتورة وعكس أثرها"),
            new("Payment.Create", "تسجيل سند قبض عميل", "الحسابات", "Create", "تحصيل نقدية أو شيكات من عميل"),
            new("SupplierPayment.Create", "تسجيل سند صرف مورد", "الحسابات", "Create", "سداد مستحقات الموردين"),

            // Reports
            new("Reports.View", "عرض التقارير والتحليلات", "التقارير", "View", "استعراض لوحات المؤشرات والتقارير"),
            new("Reports.Export", "تصدير وطباعة التقارير", "التقارير", "Export", "تصدير التقارير بصيغ PDF و Excel"),

            // Security
            new("Users.View", "عرض المستخدمين", "إدارة النظام", "View", "الاطلاع على قائمة المستخدمين والحالات"),
            new("Users.Create", "إضافة مستخدم جديد", "إدارة النظام", "Create", "إنشاء حساب مستخدم جديد"),
            new("Users.Edit", "تعديل بيانات المستخدم", "إدارة النظام", "Edit", "تعديل الصلاحيات والأدوار والمستودعات"),
            new("Roles.Manage", "إدارة الأدوار ومصفوفة الصلاحيات", "إدارة النظام", "Manage", "تعديل وضبط صلاحيات الأدوار"),
            new("Audit.View", "عرض سجل التدقيق وأحداث الأمان", "إدارة النظام", "View", "الاطلاع على السجلات والأحداث الأمنية"),
            new("Security.Dashboard.View", "عرض لوحة الأمان والرقابة", "إدارة النظام", "View", "الاطلاع على مؤشرات الأمان والنشاط"),

            // Phase 19: System Administration & Configuration
            new("Settings.View", "عرض إعدادات النظام", "إعدادات النظام", "View", "الاطلاع على معلمات وإعدادات النظام والشركة والضرائب"),
            new("Settings.Edit", "تعديل إعدادات النظام العامة", "إعدادات النظام", "Edit", "تعديل معلمات النظام وخيارات التشغيل والترقيم"),
            new("Settings.Company.Manage", "إدارة الملف التعريفي للشركة", "إعدادات النظام", "Manage", "تحديث بيانات وهوية الشركة والضرائب والعملة"),
            new("Settings.Tax.Manage", "إدارة الإعدادات الضريبية", "إعدادات النظام", "Manage", "تحديث وتفعيل الضرائب ونسب القيمة المضافة"),
            new("Settings.DocumentNumbering.Manage", "إدارة تسلسلات وبادئات المستندات", "إعدادات النظام", "Manage", "ضبط صيغ ترقيم الفواتير والطلبات والتسلسلات"),
            new("Settings.Inventory.Manage", "إدارة محددات وضوابط المخزون", "إعدادات النظام", "Manage", "تحديد المستودعات الافتراضية وحدود الأمان"),
            new("Settings.Production.Manage", "إدارة محددات وضوابط الإنتاج", "إعدادات النظام", "Manage", "تحديد نسب الهدر ومستودعات التشغيل"),
            new("Settings.Purchasing.Manage", "إدارة محددات وضوابط المشتريات", "إعدادات النظام", "Manage", "ضبط خيارات الشراء والاستلام"),
            new("Settings.Sales.Manage", "إدارة محددات وضوابط المبيعات", "إعدادات النظام", "Manage", "ضبط خيارات البيع والصرف والائتمان"),
            new("Settings.Accounting.Manage", "إدارة ربط الحسابات والدليل المحاسبي", "إعدادات النظام", "Manage", "ضبط وتوجيه ربط العمليات بالدليل المحاسبي"),

            // Phase 20: System Health, Disaster Recovery & Production Readiness
            new("System.Health.View", "عرض جاهزية وصحة النظام والنسخ الاحتياطي", "إدارة النظام", "View", "الاطلاع على لوحة فحص جاهزية النظام، قاعدة البيانات، وسجلات النسخ الاحتياطي")
        };
    }

    private static List<RoleDefinition> GetDefaultRolesCatalog()
    {
        return new List<RoleDefinition>
        {
            new("Super Admin", "SUPER_ADMIN", "مدير النظام الأعلى", "كامل الصلاحيات الفنية والتشغيلية والإدارية للنظام", new List<string>()),
            
            new("General Manager", "GENERAL_MANAGER", "المدير العام", "متابعة شاملة لكافة التقارير والاعتمادات بدون صلاحيات إدارة مستخدمين فنية", new List<string>
            {
                "Sales.Order.View", "Sales.Order.Confirm", "Purchasing.PO.Approve", "Waste.Approve", "QC.Inspection.Approve", "FinishedGoods.Release", "Accounting.Journal.View", "Reports.View", "Reports.Export", "Audit.View", "Security.Dashboard.View", "Inventory.Stock.View", "Inventory.Valuation.View", "Settings.View", "System.Health.View"
            }),



            new("Production Manager", "PRODUCTION_MANAGER", "مدير الإنتاج", "إدارة وتخطيط وجدولة أوامر الإنتاج والتشغيلات والوصفات", new List<string>
            {
                "Production.Order.Create", "Production.Order.Release", "Production.Batch.Start", "Production.Batch.Complete", "Production.Recipe.Manage", "Waste.Create", "Waste.Approve", "Inventory.Stock.View", "Reports.View"
            }),

            new("Warehouse Manager", "WAREHOUSE_MANAGER", "مدير المستودعات", "إدارة المستودعات، أرصدة الخامات، التحويلات والتسويات الجردية", new List<string>
            {
                "Inventory.Stock.View", "Inventory.Stock.Adjust", "Inventory.Transfer.Create", "Inventory.Valuation.View", "Inventory.Warehouse.Manage", "Purchasing.Receipt.Create", "FinishedGoods.Stock.View", "Reports.View"
            }),

            new("Purchasing Officer", "PURCHASING_OFFICER", "مسؤول المشتريات", "إصدار طلبات وأوامر الشراء وإدارة أسعار الموردين", new List<string>
            {
                "Purchasing.PR.Create", "Purchasing.PO.Create", "Purchasing.Receipt.Create", "Purchasing.Supplier.Manage", "Inventory.Stock.View", "Reports.View"
            }),

            new("Sales Officer", "SALES_OFFICER", "مسؤول المبيعات", "إدارة العملاء وأوامر المبيعات والتسليمات", new List<string>
            {
                "Sales.Order.View", "Sales.Order.Create", "Sales.Order.Edit", "Sales.Order.Confirm", "Sales.Fulfillment.Create", "Sales.Fulfillment.Execute", "Sales.Customer.Manage", "Reports.View"
            }),

            new("Accountant", "ACCOUNTANT", "المحاسب المالي", "إدارة الفواتير، التحصيلات، سندات الصرف، القيود والتقارير المالية", new List<string>
            {
                "Accounting.Journal.View", "Accounting.Journal.Create", "Accounting.Journal.Post", "Accounting.Journal.Reverse", "Accounting.Period.Manage", "Invoice.Create", "Invoice.Issue", "Invoice.Cancel", "Payment.Create", "SupplierPayment.Create", "Reports.View", "Reports.Export", "Inventory.Valuation.View"
            }),

            new("Quality Officer", "QUALITY_OFFICER", "مسؤول رقابة الجودة", "إجراء فحوصات واختبارات الجودة، الإفراج، وإدارة القوالب", new List<string>
            {
                "QC.Inspection.Create", "QC.Inspection.Approve", "QC.Inspection.Reject", "QC.Template.Manage", "Reports.View"
            }),

            new("Packaging Officer", "PACKAGING_OFFICER", "مسؤول التعبئة والتغليف", "إصدار وتنفيذ أوامر التعبئة ومتابعة مواد التغليف", new List<string>
            {
                "Packaging.Order.Create", "Packaging.Order.Execute", "Packaging.BOM.Manage", "Inventory.Stock.View", "Reports.View"
            }),

            new("Warehouse Operator", "WAREHOUSE_OPERATOR", "مشغل المستودع", "تسجيل حركات الاستلام والصرف التشغيلية بالمستودع", new List<string>
            {
                "Inventory.Stock.View", "Inventory.Transfer.Create", "Purchasing.Receipt.Create"
            }),

            new("Production Operator", "PRODUCTION_OPERATOR", "مشغل الإنتاج", "تسجيل بدء واستهلاك الخامات في صالة الإنتاج", new List<string>
            {
                "Production.Batch.Start", "Production.Batch.Complete", "Waste.Create"
            }),

            new("Sales Viewer", "SALES_VIEWER", "مشاهد المبيعات", "اطلاع فقط على أوامر المبيعات والعملاء بدون تعديل", new List<string>
            {
                "Sales.Order.View", "Reports.View"
            }),

            new("Report Viewer", "REPORT_VIEWER", "مشاهد التقارير", "اطلاع فقط على لوحات المعلومات والتقارير الإدارية", new List<string>
            {
                "Reports.View"
            })
        };
    }

    private record PermissionDefinition(string Code, string Name, string Module, string Action, string Description);
    private record RoleDefinition(string Name, string Code, string DisplayName, string Description, List<string> DefaultPermissionCodes);
}
