using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using FactoryX.Application.DTOs;
using FactoryX.Application.Services.Abstracts;
using FactoryX.Domain.Entities;
using FactoryX.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FactoryX.Application.Services.Concretes;

public class SettingsService : ISettingsService
{
    private readonly AppDbContext _context;
    private readonly IAuditService _auditService;
    private static readonly ConcurrentDictionary<string, string> _settingsCache = new();
    private static readonly object _lock = new();

    public SettingsService(AppDbContext context, IAuditService auditService)
    {
        _context = context;
        _auditService = auditService;
    }

    #region Generic Typed Settings
    public async Task<string?> GetSettingValueAsync(string key)
    {
        if (string.IsNullOrWhiteSpace(key)) return null;

        if (_settingsCache.TryGetValue(key, out var cachedValue))
        {
            return cachedValue;
        }

        var setting = await _context.SystemSettings.AsNoTracking().FirstOrDefaultAsync(s => s.Key == key && s.IsActive);
        if (setting != null)
        {
            _settingsCache[key] = setting.Value;
            return setting.Value;
        }

        return null;
    }

    public async Task<T> GetSettingValueAsync<T>(string key, T defaultValue)
    {
        var val = await GetSettingValueAsync(key);
        if (val == null) return defaultValue;

        try
        {
            var targetType = typeof(T);
            var nullableUnderlying = Nullable.GetUnderlyingType(targetType);
            if (nullableUnderlying != null)
            {
                targetType = nullableUnderlying;
            }

            if (targetType == typeof(string)) return (T)(object)val;
            if (targetType == typeof(int)) return (T)(object)int.Parse(val);
            if (targetType == typeof(long)) return (T)(object)long.Parse(val);
            if (targetType == typeof(decimal)) return (T)(object)decimal.Parse(val);
            if (targetType == typeof(double)) return (T)(object)double.Parse(val);
            if (targetType == typeof(bool)) return (T)(object)bool.Parse(val);
            if (targetType == typeof(DateTime)) return (T)(object)DateTime.Parse(val);
            if (targetType.IsEnum) return (T)Enum.Parse(targetType, val);

            return (T)Convert.ChangeType(val, targetType);
        }
        catch
        {
            return defaultValue;
        }
    }

    public async Task SetSettingValueAsync(string key, string value, string updatedBy, SettingDataType dataType = SettingDataType.String, SettingCategory category = SettingCategory.General, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Setting key cannot be empty", nameof(key));

        var setting = await _context.SystemSettings.FirstOrDefaultAsync(s => s.Key == key);
        string? oldValue = setting?.Value;

        if (setting == null)
        {
            setting = new SystemSetting
            {
                Key = key,
                Value = value ?? string.Empty,
                DataType = dataType,
                Category = category,
                Description = description,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                UpdatedBy = updatedBy
            };
            _context.SystemSettings.Add(setting);
        }
        else
        {
            setting.Value = value ?? string.Empty;
            setting.DataType = dataType;
            setting.Category = category;
            if (description != null) setting.Description = description;
            setting.UpdatedAt = DateTime.UtcNow;
            setting.UpdatedBy = updatedBy;
        }

        await _context.SaveChangesAsync();
        _settingsCache[key] = value ?? string.Empty;

        // Log audit
        await _auditService.LogActivityAsync(
            userId: null,
            username: updatedBy,
            action: "UpdateSetting",
            module: "Settings",
            entityType: "SystemSetting",
            entityId: setting.Id.ToString(),
            entityNumber: key,
            description: $"تحديث إعداد النظام [{key}] إلى [{value}]",
            oldValues: oldValue != null ? JsonSerializer.Serialize(new { Key = key, Value = oldValue }) : null,
            newValues: JsonSerializer.Serialize(new { Key = key, Value = value, DataType = dataType.ToString(), Category = category.ToString() })
        );
    }

    public async Task<Dictionary<string, string>> GetSettingsByCategoryAsync(SettingCategory category)
    {
        var settings = await _context.SystemSettings.AsNoTracking()
            .Where(s => s.Category == category && s.IsActive)
            .ToListAsync();

        var dict = new Dictionary<string, string>();
        foreach (var s in settings)
        {
            dict[s.Key] = s.Value;
            _settingsCache[s.Key] = s.Value;
        }
        return dict;
    }

    public async Task<IEnumerable<SystemSettingDto>> GetAllSettingsAsync()
    {
        var settings = await _context.SystemSettings.AsNoTracking()
            .OrderBy(s => s.Category)
            .ThenBy(s => s.Key)
            .ToListAsync();

        return settings.Select(s => new SystemSettingDto(
            s.Id,
            s.Key,
            s.Value,
            s.DataType,
            s.Category,
            s.Description,
            s.IsActive,
            s.UpdatedAt,
            s.UpdatedBy
        ));
    }
    #endregion

    #region Company Profile
    public async Task<CompanyProfileDto> GetCompanyProfileAsync()
    {
        var profile = await _context.CompanyProfiles.AsNoTracking().FirstOrDefaultAsync();
        if (profile == null)
        {
            return new CompanyProfileDto
            {
                CompanyName = "مصنع حلويات المولد الفاخرة",
                LegalName = "شركة حلويات المولد للصناعات الغذائية ذ.م.م",
                CommercialRegistration = "CR-104829",
                TaxRegistrationNumber = "TRN-948-284-110",
                Address = "المنطقة الصناعية الثانية، قطعة 44",
                City = "مدينة السادس من أكتوبر",
                Country = "جمهورية مصر العربية",
                Phone = "+20 2 38330000",
                Email = "info@mawlidsweets.com",
                Website = "https://www.mawlidsweets.com",
                DefaultCurrency = "EGP",
                DefaultTimeZone = "Egypt Standard Time"
            };
        }

        return new CompanyProfileDto
        {
            Id = profile.Id,
            CompanyName = profile.CompanyName,
            LegalName = profile.LegalName,
            CommercialRegistration = profile.CommercialRegistration,
            TaxRegistrationNumber = profile.TaxRegistrationNumber,
            Address = profile.Address,
            City = profile.City,
            Country = profile.Country,
            Phone = profile.Phone,
            Email = profile.Email,
            Website = profile.Website,
            LogoUrl = profile.LogoUrl,
            DefaultCurrency = profile.DefaultCurrency,
            DefaultTimeZone = profile.DefaultTimeZone,
            UpdatedAt = profile.UpdatedAt,
            UpdatedBy = profile.UpdatedBy
        };
    }

    public async Task UpdateCompanyProfileAsync(CompanyProfileDto dto, string updatedBy)
    {
        if (string.IsNullOrWhiteSpace(dto.CompanyName))
            throw new InvalidOperationException("اسم الشركة التجاري مطلوب");

        var profile = await _context.CompanyProfiles.FirstOrDefaultAsync();
        string? oldState = profile != null ? JsonSerializer.Serialize(profile) : null;

        if (profile == null)
        {
            profile = new CompanyProfile
            {
                CompanyName = dto.CompanyName,
                LegalName = dto.LegalName,
                CommercialRegistration = dto.CommercialRegistration,
                TaxRegistrationNumber = dto.TaxRegistrationNumber,
                Address = dto.Address,
                City = dto.City,
                Country = dto.Country,
                Phone = dto.Phone,
                Email = dto.Email,
                Website = dto.Website,
                LogoUrl = dto.LogoUrl,
                DefaultCurrency = string.IsNullOrWhiteSpace(dto.DefaultCurrency) ? "EGP" : dto.DefaultCurrency,
                DefaultTimeZone = string.IsNullOrWhiteSpace(dto.DefaultTimeZone) ? "Egypt Standard Time" : dto.DefaultTimeZone,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                UpdatedBy = updatedBy
            };
            _context.CompanyProfiles.Add(profile);
        }
        else
        {
            profile.CompanyName = dto.CompanyName;
            profile.LegalName = dto.LegalName;
            profile.CommercialRegistration = dto.CommercialRegistration;
            profile.TaxRegistrationNumber = dto.TaxRegistrationNumber;
            profile.Address = dto.Address;
            profile.City = dto.City;
            profile.Country = dto.Country;
            profile.Phone = dto.Phone;
            profile.Email = dto.Email;
            profile.Website = dto.Website;
            if (!string.IsNullOrWhiteSpace(dto.LogoUrl)) profile.LogoUrl = dto.LogoUrl;
            profile.DefaultCurrency = string.IsNullOrWhiteSpace(dto.DefaultCurrency) ? "EGP" : dto.DefaultCurrency;
            profile.DefaultTimeZone = string.IsNullOrWhiteSpace(dto.DefaultTimeZone) ? "Egypt Standard Time" : dto.DefaultTimeZone;
            profile.UpdatedAt = DateTime.UtcNow;
            profile.UpdatedBy = updatedBy;
        }

        await _context.SaveChangesAsync();

        // Also sync generic settings for currency & timezone
        await SetSettingValueAsync("General.CurrencyCode", profile.DefaultCurrency, updatedBy, SettingDataType.String, SettingCategory.General, "Default Currency Code");
        await SetSettingValueAsync("General.SystemTimeZone", profile.DefaultTimeZone, updatedBy, SettingDataType.String, SettingCategory.General, "Default System Time Zone");

        await _auditService.LogActivityAsync(
            userId: null,
            username: updatedBy,
            action: "UpdateCompanyProfile",
            module: "Settings",
            entityType: "CompanyProfile",
            entityId: profile.Id.ToString(),
            entityNumber: profile.CompanyName,
            description: $"تحديث الملف التعريفي للشركة [{profile.CompanyName}]",
            oldValues: oldState,
            newValues: JsonSerializer.Serialize(dto)
        );
    }
    #endregion

    #region General & Regional Settings
    public async Task<GeneralSettingsDto> GetGeneralSettingsAsync()
    {
        return new GeneralSettingsDto
        {
            CurrencyCode = await GetSettingValueAsync("General.CurrencyCode", "EGP"),
            CurrencyName = await GetSettingValueAsync("General.CurrencyName", "الجنيه المصري"),
            CurrencySymbol = await GetSettingValueAsync("General.CurrencySymbol", "ج.م"),
            CurrencyDecimalPrecision = await GetSettingValueAsync("General.CurrencyDecimalPrecision", 2),
            SystemTimeZone = await GetSettingValueAsync("General.SystemTimeZone", "Egypt Standard Time"),
            DateDisplayFormat = await GetSettingValueAsync("General.DateDisplayFormat", "yyyy-MM-dd"),
            TimeDisplayFormat = await GetSettingValueAsync("General.TimeDisplayFormat", "HH:mm:ss"),
            FirstDayOfWeek = await GetSettingValueAsync("General.FirstDayOfWeek", "Saturday")
        };
    }

    public async Task UpdateGeneralSettingsAsync(GeneralSettingsDto dto, string updatedBy)
    {
        var oldState = JsonSerializer.Serialize(await GetGeneralSettingsAsync());

        await SetSettingValueAsync("General.CurrencyCode", dto.CurrencyCode, updatedBy, SettingDataType.String, SettingCategory.General, "رمز العملة الأساسية");
        await SetSettingValueAsync("General.CurrencyName", dto.CurrencyName, updatedBy, SettingDataType.String, SettingCategory.General, "اسم العملة");
        await SetSettingValueAsync("General.CurrencySymbol", dto.CurrencySymbol, updatedBy, SettingDataType.String, SettingCategory.General, "رمز العملة المختصر");
        await SetSettingValueAsync("General.CurrencyDecimalPrecision", dto.CurrencyDecimalPrecision.ToString(), updatedBy, SettingDataType.Integer, SettingCategory.General, "عدد الخانات العشرية للمبالغ");
        await SetSettingValueAsync("General.SystemTimeZone", dto.SystemTimeZone, updatedBy, SettingDataType.String, SettingCategory.General, "المنطقة الزمنية للنظام");
        await SetSettingValueAsync("General.DateDisplayFormat", dto.DateDisplayFormat, updatedBy, SettingDataType.String, SettingCategory.General, "تنسيق عرض التاريخ");
        await SetSettingValueAsync("General.TimeDisplayFormat", dto.TimeDisplayFormat, updatedBy, SettingDataType.String, SettingCategory.General, "تنسيق عرض الوقت");
        await SetSettingValueAsync("General.FirstDayOfWeek", dto.FirstDayOfWeek, updatedBy, SettingDataType.String, SettingCategory.General, "اليوم الأول في أسبوع العمل");

        await _auditService.LogActivityAsync(
            userId: null,
            username: updatedBy,
            action: "UpdateGeneralSettings",
            module: "Settings",
            entityType: "GeneralSettings",
            entityId: "General",
            entityNumber: "General",
            description: "تحديث الإعدادات العامة والإقليمية للنظام",
            oldValues: oldState,
            newValues: JsonSerializer.Serialize(dto)
        );
    }
    #endregion

    #region Tax Settings
    public async Task<TaxSettingDto> GetCurrentTaxSettingAsync()
    {
        var tax = await _context.TaxSettings.AsNoTracking()
            .Where(t => t.IsActive && t.IsDefault)
            .OrderByDescending(t => t.EffectiveFrom)
            .FirstOrDefaultAsync();

        if (tax == null)
        {
            tax = await _context.TaxSettings.AsNoTracking()
                .Where(t => t.IsActive)
                .OrderByDescending(t => t.EffectiveFrom)
                .FirstOrDefaultAsync();
        }

        if (tax == null)
        {
            return new TaxSettingDto
            {
                Name = "ضريبة القيمة المضافة القياسية",
                Code = "VAT_14",
                Rate = 14.00m,
                EffectiveFrom = new DateTime(2020, 1, 1),
                IsActive = true,
                IsDefault = true,
                Description = "ضريبة القيمة المضافة الافتراضية 14%"
            };
        }

        return new TaxSettingDto
        {
            Id = tax.Id,
            Name = tax.Name,
            Code = tax.Code,
            Rate = tax.Rate,
            EffectiveFrom = tax.EffectiveFrom,
            EffectiveTo = tax.EffectiveTo,
            IsActive = tax.IsActive,
            IsDefault = tax.IsDefault,
            Description = tax.Description,
            UpdatedAt = tax.UpdatedAt,
            UpdatedBy = tax.UpdatedBy
        };
    }

    public async Task<IEnumerable<TaxSettingDto>> GetAllTaxSettingsAsync()
    {
        var list = await _context.TaxSettings.AsNoTracking()
            .OrderByDescending(t => t.IsDefault)
            .ThenByDescending(t => t.IsActive)
            .ThenBy(t => t.Code)
            .ToListAsync();

        return list.Select(t => new TaxSettingDto
        {
            Id = t.Id,
            Name = t.Name,
            Code = t.Code,
            Rate = t.Rate,
            EffectiveFrom = t.EffectiveFrom,
            EffectiveTo = t.EffectiveTo,
            IsActive = t.IsActive,
            IsDefault = t.IsDefault,
            Description = t.Description,
            UpdatedAt = t.UpdatedAt,
            UpdatedBy = t.UpdatedBy
        });
    }

    public async Task SaveTaxSettingAsync(TaxSettingDto dto, string updatedBy)
    {
        if (dto.Rate < 0 || dto.Rate > 100)
            throw new InvalidOperationException("نسبة الضريبة يجب أن تكون بين 0% و 100%");

        if (string.IsNullOrWhiteSpace(dto.Code))
            throw new InvalidOperationException("رمز الضريبة مطلوب");

        var duplicateCode = await _context.TaxSettings.AnyAsync(t => t.Code == dto.Code && t.Id != dto.Id);
        if (duplicateCode)
            throw new InvalidOperationException($"رمز الضريبة [{dto.Code}] مسجل مسبقاً، يرجى اختيار رمز فريد.");

        TaxSetting? tax;
        string? oldState = null;

        if (dto.Id > 0)
        {
            tax = await _context.TaxSettings.FirstOrDefaultAsync(t => t.Id == dto.Id);
            if (tax == null) throw new InvalidOperationException("الضريبة غير موجودة");
            oldState = JsonSerializer.Serialize(tax);
        }
        else
        {
            tax = new TaxSetting
            {
                CreatedAt = DateTime.UtcNow
            };
            _context.TaxSettings.Add(tax);
        }

        tax.Name = dto.Name;
        tax.Code = dto.Code;
        tax.Rate = dto.Rate;
        tax.EffectiveFrom = dto.EffectiveFrom;
        tax.EffectiveTo = dto.EffectiveTo;
        tax.IsActive = dto.IsActive;
        tax.IsDefault = dto.IsDefault;
        tax.Description = dto.Description;
        tax.UpdatedAt = DateTime.UtcNow;
        tax.UpdatedBy = updatedBy;

        // If marked as default, unset other defaults
        if (dto.IsDefault)
        {
            var otherDefaults = await _context.TaxSettings.Where(t => t.Id != dto.Id && t.IsDefault).ToListAsync();
            foreach (var other in otherDefaults)
            {
                other.IsDefault = false;
                other.UpdatedAt = DateTime.UtcNow;
            }
        }

        await _context.SaveChangesAsync();

        await _auditService.LogActivityAsync(
            userId: null,
            username: updatedBy,
            action: dto.Id > 0 ? "UpdateTaxSetting" : "CreateTaxSetting",
            module: "Settings",
            entityType: "TaxSetting",
            entityId: tax.Id.ToString(),
            entityNumber: tax.Code,
            description: $"حفظ إعداد الضريبة [{tax.Name}] بنسبة {tax.Rate}%",
            oldValues: oldState,
            newValues: JsonSerializer.Serialize(dto)
        );
    }

    public async Task ToggleTaxSettingStatusAsync(int id, bool isActive, string updatedBy)
    {
        var tax = await _context.TaxSettings.FirstOrDefaultAsync(t => t.Id == id);
        if (tax == null) throw new InvalidOperationException("الضريبة غير موجودة");

        if (!isActive && tax.IsDefault)
        {
            throw new InvalidOperationException("لا يمكن تعطيل الضريبة الافتراضية. يرجى تعيين ضريبة افتراضية أخرى أولاً.");
        }

        var oldState = JsonSerializer.Serialize(tax);
        tax.IsActive = isActive;
        tax.UpdatedAt = DateTime.UtcNow;
        tax.UpdatedBy = updatedBy;

        await _context.SaveChangesAsync();

        await _auditService.LogActivityAsync(
            userId: null,
            username: updatedBy,
            action: "ToggleTaxSettingStatus",
            module: "Settings",
            entityType: "TaxSetting",
            entityId: tax.Id.ToString(),
            entityNumber: tax.Code,
            description: $"تغيير حالة الضريبة [{tax.Code}] إلى [{(isActive ? "نشط" : "معطل")}]",
            oldValues: oldState,
            newValues: JsonSerializer.Serialize(new { tax.Id, tax.Code, IsActive = isActive })
        );
    }
    #endregion

    #region Document Numbering
    public async Task<IEnumerable<DocumentNumberSettingDto>> GetDocumentNumberSettingsAsync()
    {
        var list = await _context.DocumentNumberSettings.AsNoTracking()
            .OrderBy(d => d.DocumentType)
            .ToListAsync();

        return list.Select(d => new DocumentNumberSettingDto
        {
            Id = d.Id,
            DocumentType = d.DocumentType,
            DocumentTypeNameArabic = d.DocumentTypeNameArabic,
            Prefix = d.Prefix,
            DateFormat = d.DateFormat,
            SequenceWidth = d.SequenceWidth,
            NextSequenceValue = d.NextSequenceValue,
            Delimiter = d.Delimiter,
            Description = d.Description,
            IsActive = d.IsActive,
            UpdatedAt = d.UpdatedAt,
            UpdatedBy = d.UpdatedBy
        });
    }

    public async Task<DocumentNumberSettingDto?> GetDocumentNumberSettingByTypeAsync(string documentType)
    {
        var d = await _context.DocumentNumberSettings.AsNoTracking()
            .FirstOrDefaultAsync(x => x.DocumentType == documentType && x.IsActive);

        if (d == null) return null;

        return new DocumentNumberSettingDto
        {
            Id = d.Id,
            DocumentType = d.DocumentType,
            DocumentTypeNameArabic = d.DocumentTypeNameArabic,
            Prefix = d.Prefix,
            DateFormat = d.DateFormat,
            SequenceWidth = d.SequenceWidth,
            NextSequenceValue = d.NextSequenceValue,
            Delimiter = d.Delimiter,
            Description = d.Description,
            IsActive = d.IsActive,
            UpdatedAt = d.UpdatedAt,
            UpdatedBy = d.UpdatedBy
        };
    }

    public async Task SaveDocumentNumberSettingAsync(DocumentNumberSettingDto dto, string updatedBy)
    {
        if (string.IsNullOrWhiteSpace(dto.DocumentType))
            throw new InvalidOperationException("نوع المستند مطلوب");

        if (string.IsNullOrWhiteSpace(dto.Prefix))
            throw new InvalidOperationException("بادئة الترقيم مطلوبة");

        if (dto.SequenceWidth < 1 || dto.SequenceWidth > 10)
            throw new InvalidOperationException("طول خانات التسلسل يجب أن يكون بين 1 و 10");

        if (dto.NextSequenceValue < 1)
            throw new InvalidOperationException("قيمة التسلسل القادمة يجب أن تكون أكبر من صفر");

        var setting = await _context.DocumentNumberSettings.FirstOrDefaultAsync(d => d.DocumentType == dto.DocumentType);
        string? oldState = setting != null ? JsonSerializer.Serialize(setting) : null;

        if (setting == null)
        {
            setting = new DocumentNumberSetting
            {
                DocumentType = dto.DocumentType,
                DocumentTypeNameArabic = dto.DocumentTypeNameArabic,
                Prefix = dto.Prefix.Trim().ToUpper(),
                DateFormat = dto.DateFormat?.Trim() ?? "yyyyMMdd",
                SequenceWidth = dto.SequenceWidth,
                NextSequenceValue = dto.NextSequenceValue,
                Delimiter = dto.Delimiter ?? "-",
                Description = dto.Description,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                UpdatedBy = updatedBy
            };
            _context.DocumentNumberSettings.Add(setting);
        }
        else
        {
            setting.DocumentTypeNameArabic = string.IsNullOrWhiteSpace(dto.DocumentTypeNameArabic) ? setting.DocumentTypeNameArabic : dto.DocumentTypeNameArabic;
            setting.Prefix = dto.Prefix.Trim().ToUpper();
            setting.DateFormat = dto.DateFormat?.Trim() ?? "yyyyMMdd";
            setting.SequenceWidth = dto.SequenceWidth;
            setting.NextSequenceValue = dto.NextSequenceValue;
            setting.Delimiter = dto.Delimiter ?? "-";
            setting.Description = dto.Description;
            setting.IsActive = dto.IsActive;
            setting.UpdatedAt = DateTime.UtcNow;
            setting.UpdatedBy = updatedBy;
        }

        await _context.SaveChangesAsync();

        await _auditService.LogActivityAsync(
            userId: null,
            username: updatedBy,
            action: "SaveDocumentNumberSetting",
            module: "Settings",
            entityType: "DocumentNumberSetting",
            entityId: setting.Id.ToString(),
            entityNumber: setting.DocumentType,
            description: $"تحديث صيغة ترقيم مستند [{setting.DocumentTypeNameArabic} ({setting.DocumentType})] إلى البادئة [{setting.Prefix}]",
            oldValues: oldState,
            newValues: JsonSerializer.Serialize(dto)
        );
    }

    public async Task<string> GenerateDocumentNumberAsync(string documentType, DateTime? date = null)
    {
        var currentDate = date ?? DateTime.UtcNow;

        lock (_lock)
        {
            var setting = _context.DocumentNumberSettings.FirstOrDefault(d => d.DocumentType == documentType);
            if (setting == null)
            {
                // Fallback default format if not configured
                var prefix = documentType.ToUpper();
                var fallbackNumber = $"{prefix}-{currentDate:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}";
                return fallbackNumber;
            }

            var currentSeq = setting.NextSequenceValue;
            setting.NextSequenceValue += 1;
            setting.UpdatedAt = DateTime.UtcNow;
            _context.SaveChanges();

            var seqPadded = currentSeq.ToString().PadLeft(setting.SequenceWidth, '0');
            var datePart = !string.IsNullOrWhiteSpace(setting.DateFormat) ? currentDate.ToString(setting.DateFormat) : string.Empty;
            var delimiter = setting.Delimiter ?? "-";

            if (string.IsNullOrWhiteSpace(datePart))
            {
                return $"{setting.Prefix}{delimiter}{seqPadded}";
            }

            return $"{setting.Prefix}{delimiter}{datePart}{delimiter}{seqPadded}";
        }
    }
    #endregion

    #region Operational Defaults
    public async Task<OperationalDefaultsDto> GetOperationalDefaultsAsync()
    {
        return new OperationalDefaultsDto
        {
            DefaultRawMaterialWarehouseId = await GetSettingValueAsync<int?>("Inventory.DefaultRawMaterialWarehouseId", null),
            DefaultProductionWarehouseId = await GetSettingValueAsync<int?>("Inventory.DefaultProductionWarehouseId", null),
            DefaultPackagingWarehouseId = await GetSettingValueAsync<int?>("Inventory.DefaultPackagingWarehouseId", null),
            DefaultFinishedGoodsWarehouseId = await GetSettingValueAsync<int?>("Inventory.DefaultFinishedGoodsWarehouseId", null),
            DefaultQuarantineWarehouseId = await GetSettingValueAsync<int?>("Inventory.DefaultQuarantineWarehouseId", null),
            LowStockWarningThreshold = await GetSettingValueAsync<decimal>("Inventory.LowStockWarningThreshold", 100m),
            ExpiryWarningDays = await GetSettingValueAsync<int>("Inventory.ExpiryWarningDays", 30),
            AllowNegativeStock = await GetSettingValueAsync<bool>("Inventory.AllowNegativeStock", false),
            RequireLotTracking = await GetSettingValueAsync<bool>("Inventory.RequireLotTracking", true),
            RequireWasteApproval = await GetSettingValueAsync<bool>("Waste.RequireWasteApproval", true),
            MaxWasteTolerancePercent = await GetSettingValueAsync<decimal>("Waste.MaxWasteTolerancePercent", 5.0m)
        };
    }

    public async Task SaveOperationalDefaultsAsync(OperationalDefaultsDto dto, string updatedBy)
    {
        // Validate warehouses if specified
        var warehouseIds = new List<int?>
        {
            dto.DefaultRawMaterialWarehouseId,
            dto.DefaultProductionWarehouseId,
            dto.DefaultPackagingWarehouseId,
            dto.DefaultFinishedGoodsWarehouseId,
            dto.DefaultQuarantineWarehouseId
        }.Where(id => id.HasValue && id.Value > 0).Select(id => id!.Value).Distinct().ToList();

        if (warehouseIds.Any())
        {
            var validCount = await _context.Warehouses.CountAsync(w => warehouseIds.Contains(w.Id) && w.IsActive);
            if (validCount != warehouseIds.Count)
            {
                throw new InvalidOperationException("أحد المستودعات المحددة غير موجود أو غير نشط في النظام.");
            }
        }

        if (dto.MaxWasteTolerancePercent < 0 || dto.MaxWasteTolerancePercent > 100)
        {
            throw new InvalidOperationException("نسبة التسامح في هدر الإنتاج يجب أن تكون بين 0% و 100%.");
        }

        if (dto.ExpiryWarningDays < 1 || dto.ExpiryWarningDays > 365)
        {
            throw new InvalidOperationException("أيام التنبيه المسبق للصلاحية يجب أن تكون بين 1 و 365 يوماً.");
        }

        var oldState = JsonSerializer.Serialize(await GetOperationalDefaultsAsync());

        await SetSettingValueAsync("Inventory.DefaultRawMaterialWarehouseId", dto.DefaultRawMaterialWarehouseId?.ToString() ?? "", updatedBy, SettingDataType.Integer, SettingCategory.Inventory, "مستودع الخامات الافتراضي");
        await SetSettingValueAsync("Inventory.DefaultProductionWarehouseId", dto.DefaultProductionWarehouseId?.ToString() ?? "", updatedBy, SettingDataType.Integer, SettingCategory.Inventory, "مستودع الإنتاج والتشغيل الافتراضي");
        await SetSettingValueAsync("Inventory.DefaultPackagingWarehouseId", dto.DefaultPackagingWarehouseId?.ToString() ?? "", updatedBy, SettingDataType.Integer, SettingCategory.Inventory, "مستودع التعبئة والتغليف الافتراضي");
        await SetSettingValueAsync("Inventory.DefaultFinishedGoodsWarehouseId", dto.DefaultFinishedGoodsWarehouseId?.ToString() ?? "", updatedBy, SettingDataType.Integer, SettingCategory.Inventory, "مستودع المنتجات التامة الافتراضي");
        await SetSettingValueAsync("Inventory.DefaultQuarantineWarehouseId", dto.DefaultQuarantineWarehouseId?.ToString() ?? "", updatedBy, SettingDataType.Integer, SettingCategory.Inventory, "مستودع الحجر والفحص الافتراضي");
        await SetSettingValueAsync("Inventory.LowStockWarningThreshold", dto.LowStockWarningThreshold.ToString(), updatedBy, SettingDataType.Decimal, SettingCategory.Inventory, "حد التنبيه لانخفاض المخزون");
        await SetSettingValueAsync("Inventory.ExpiryWarningDays", dto.ExpiryWarningDays.ToString(), updatedBy, SettingDataType.Integer, SettingCategory.Inventory, "أيام التنبيه المسبق لقرب انتهاء الصلاحية");
        await SetSettingValueAsync("Inventory.AllowNegativeStock", dto.AllowNegativeStock.ToString(), updatedBy, SettingDataType.Boolean, SettingCategory.Inventory, "السماح بالصرف بالسالب");
        await SetSettingValueAsync("Inventory.RequireLotTracking", dto.RequireLotTracking.ToString(), updatedBy, SettingDataType.Boolean, SettingCategory.Inventory, "إلزامية تتبع أرقام التشغيلات");
        await SetSettingValueAsync("Waste.RequireWasteApproval", dto.RequireWasteApproval.ToString(), updatedBy, SettingDataType.Boolean, SettingCategory.Waste, "إلزامية اعتماد الفاقد");
        await SetSettingValueAsync("Waste.MaxWasteTolerancePercent", dto.MaxWasteTolerancePercent.ToString(), updatedBy, SettingDataType.Decimal, SettingCategory.Waste, "الحد الأقصى لنسبة الهدر المسموح بها");

        await _auditService.LogActivityAsync(
            userId: null,
            username: updatedBy,
            action: "SaveOperationalDefaults",
            module: "Settings",
            entityType: "OperationalDefaults",
            entityId: "Defaults",
            entityNumber: "Defaults",
            description: "تحديث المحددات وضوابط التشغيل والمستودعات الافتراضية",
            oldValues: oldState,
            newValues: JsonSerializer.Serialize(dto)
        );
    }
    #endregion

    #region Account Mappings (GL Integration)
    public async Task<IEnumerable<AccountingSettingDto>> GetAccountMappingsAsync()
    {
        var settings = await _context.AccountingSettings.AsNoTracking()
            .Include(s => s.Account)
            .OrderBy(s => s.Role)
            .ToListAsync();

        return settings.Select(s => new AccountingSettingDto
        {
            Id = s.Id,
            Role = s.Role,
            AccountId = s.AccountId,
            AccountCode = s.Account?.AccountCode ?? string.Empty,
            AccountName = s.Account?.AccountName ?? string.Empty,
            AccountNameAr = s.Account?.AccountNameAr ?? string.Empty,
            Description = s.Description
        });
    }

    public async Task SaveAccountMappingAsync(AccountingSettingUpdateDto dto, string updatedBy)
    {
        var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == dto.AccountId);
        if (account == null)
            throw new InvalidOperationException("الحساب المحدد غير موجود في شجرة الحسابات.");

        if (!account.IsActive)
            throw new InvalidOperationException("لا يمكن ربط حساب معطل أو موقوف.");

        if (account.IsControlAccount)
            throw new InvalidOperationException("لا يمكن ربط حساب تجميعي رئيسي، يجب اختيار حساب فرعي تحليلي.");

        // Compatibility validation
        ValidateAccountRoleCompatibility(dto.Role, account.AccountType);

        var setting = await _context.AccountingSettings.FirstOrDefaultAsync(s => s.Role == dto.Role);
        string? oldState = setting != null ? JsonSerializer.Serialize(setting) : null;

        if (setting == null)
        {
            setting = new AccountingSetting
            {
                Role = dto.Role,
                AccountId = dto.AccountId,
                Description = $"ربط حساب {dto.Role}",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.AccountingSettings.Add(setting);
        }
        else
        {
            setting.AccountId = dto.AccountId;
            setting.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        await _auditService.LogActivityAsync(
            userId: null,
            username: updatedBy,
            action: "SaveAccountMapping",
            module: "Settings",
            entityType: "AccountingSetting",
            entityId: setting.Id.ToString(),
            entityNumber: dto.Role.ToString(),
            description: $"تحديث ربط الحساب المحاسبي لدور [{dto.Role}] إلى الحساب [{account.AccountCode} - {account.AccountNameAr}]",
            oldValues: oldState,
            newValues: JsonSerializer.Serialize(new { Role = dto.Role.ToString(), AccountId = dto.AccountId, AccountCode = account.AccountCode, AccountName = account.AccountNameAr })
        );
    }


    private static void ValidateAccountRoleCompatibility(AccountRole role, AccountType accountType)
    {
        bool isCompatible = role switch
        {
            AccountRole.SalesRevenue => accountType == AccountType.Revenue,
            AccountRole.AccountsReceivable => accountType == AccountType.Asset,
            AccountRole.OutputVat => accountType == AccountType.Liability,
            AccountRole.InputVat => accountType == AccountType.Asset || accountType == AccountType.Liability,
            AccountRole.AccountsPayable => accountType == AccountType.Liability,
            AccountRole.RawMaterialInventory => accountType == AccountType.Asset,
            AccountRole.PackagingInventory => accountType == AccountType.Asset,
            AccountRole.FinishedGoodsInventory => accountType == AccountType.Asset,
            AccountRole.CostOfGoodsSold => accountType == AccountType.Expense,
            AccountRole.WasteExpense => accountType == AccountType.Expense,
            AccountRole.Cash => accountType == AccountType.Asset,
            AccountRole.Bank => accountType == AccountType.Asset,
            AccountRole.CardSettlement => accountType == AccountType.Asset,
            AccountRole.ChequesReceivable => accountType == AccountType.Asset,
            AccountRole.ProductionClearing => accountType == AccountType.Asset || accountType == AccountType.Liability || accountType == AccountType.Expense,
            _ => true
        };

        if (!isCompatible)
        {
            throw new InvalidOperationException($"نوع الحساب [{accountType}] غير متوافق مع الدور المالي المطلوب [{role}].");
        }
    }
    #endregion

    #region Configuration Audit History
    public async Task<IEnumerable<AuditLogItemDto>> GetConfigurationHistoryAsync(int limit = 100)
    {
        var logs = await _context.AuditLogs.AsNoTracking()
            .Where(a => a.Module == "Settings" || a.Module == "إعدادات النظام" || a.EntityType.Contains("Setting") || a.EntityType.Contains("CompanyProfile"))
            .OrderByDescending(a => a.Timestamp)
            .Take(limit)
            .ToListAsync();

        return logs.Select(a => new AuditLogItemDto
        {
            Id = a.Id,
            Timestamp = a.Timestamp,
            UserId = a.UserId,
            Username = a.Username,
            Action = a.Action,
            Module = a.Module,
            EntityType = a.EntityType,
            EntityId = a.EntityId,
            EntityNumber = a.EntityNumber,
            Description = a.Description,
            OldValues = a.OldValues,
            NewValues = a.NewValues,
            IpAddress = a.IpAddress,
            CorrelationId = a.CorrelationId
        });
    }
    #endregion

    #region Initialization & Seeding
    public async Task SeedDefaultConfigurationAsync()
    {
        // 1. Company Profile
        if (!await _context.CompanyProfiles.AnyAsync())
        {
            _context.CompanyProfiles.Add(new CompanyProfile
            {
                CompanyName = "مصنع حلويات المولد الفاخرة",
                LegalName = "شركة حلويات المولد للصناعات الغذائية ذ.م.م",
                CommercialRegistration = "CR-104829",
                TaxRegistrationNumber = "TRN-948-284-110",
                Address = "المنطقة الصناعية الثانية، قطعة 44",
                City = "مدينة السادس من أكتوبر",
                Country = "جمهورية مصر العربية",
                Phone = "+20 2 38330000",
                Email = "info@mawlidsweets.com",
                Website = "https://www.mawlidsweets.com",
                DefaultCurrency = "EGP",
                DefaultTimeZone = "Egypt Standard Time",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                UpdatedBy = "SYSTEM"
            });
            await _context.SaveChangesAsync();
        }

        // 2. Tax Settings
        if (!await _context.TaxSettings.AnyAsync())
        {
            _context.TaxSettings.AddRange(
                new TaxSetting
                {
                    Name = "ضريبة القيمة المضافة القياسية 14%",
                    Code = "VAT_14",
                    Rate = 14.00m,
                    EffectiveFrom = new DateTime(2020, 1, 1),
                    IsActive = true,
                    IsDefault = true,
                    Description = "الضريبة العامة المطبقة على كافة مبيعات ومشتريات المصنع",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedBy = "SYSTEM"
                },
                new TaxSetting
                {
                    Name = "معفى من الضريبة (Zero Tax)",
                    Code = "VAT_0",
                    Rate = 0.00m,
                    EffectiveFrom = new DateTime(2020, 1, 1),
                    IsActive = true,
                    IsDefault = false,
                    Description = "سلع معفاة أو صادرات خارجية",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedBy = "SYSTEM"
                }
            );
            await _context.SaveChangesAsync();
        }

        // 3. Document Number Settings
        var standardDocTypes = new List<(string Type, string ArName, string Prefix, int Width, string DateFmt)>
        {
            ("PR", "طلب شراء مواد خام", "PR", 4, "yyyyMMdd"),
            ("PO", "أمر شراء رسمي", "PO", 4, "yyyyMMdd"),
            ("GRN", "سند استلام وتوريد مخزني", "GRN", 4, "yyyyMMdd"),
            ("SO", "أمر بيع عميل", "SO", 4, "yyyyMMdd"),
            ("INV", "فاتورة مبيعات ضريبية", "INV", 5, "yyyyMMdd"),
            ("PAY", "سند تحصيل وقبض عميل", "PAY", 4, "yyyyMMdd"),
            ("SPAY", "سند صرف وسداد مورد", "SPAY", 4, "yyyyMMdd"),
            ("B", "تشغيلة إنتاج (Batch)", "B", 4, "yyyyMMdd"),
            ("W", "محضر هالك وتالف", "W", 4, "yyyyMMdd"),
            ("QC", "تقرير فحص جودة", "QC", 4, "yyyyMMdd"),
            ("PKG", "أمر تعبئة وتغليف", "PKG", 4, "yyyyMMdd"),
            ("FG", "سند إفراج منتج تام", "FG", 4, "yyyyMMdd"),
            ("JE", "قيد يومية محاسبي", "JE", 5, "yyyyMMdd")
        };

        foreach (var (type, arName, prefix, width, dateFmt) in standardDocTypes)
        {
            var exists = await _context.DocumentNumberSettings.AnyAsync(d => d.DocumentType == type);
            if (!exists)
            {
                _context.DocumentNumberSettings.Add(new DocumentNumberSetting
                {
                    DocumentType = type,
                    DocumentTypeNameArabic = arName,
                    Prefix = prefix,
                    DateFormat = dateFmt,
                    SequenceWidth = width,
                    NextSequenceValue = 1,
                    Delimiter = "-",
                    Description = $"الترقيم التلقائي لـ {arName}",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedBy = "SYSTEM"
                });
            }
        }
        await _context.SaveChangesAsync();

        // 4. Default Operational Settings
        var defaultSettings = new List<(string Key, string Value, SettingDataType Type, SettingCategory Cat, string Desc)>
        {
            ("General.CurrencyCode", "EGP", SettingDataType.String, SettingCategory.General, "رمز العملة الأساسية"),
            ("General.CurrencyName", "الجنيه المصري", SettingDataType.String, SettingCategory.General, "اسم العملة"),
            ("General.CurrencySymbol", "ج.م", SettingDataType.String, SettingCategory.General, "رمز العملة المختصر"),
            ("General.CurrencyDecimalPrecision", "2", SettingDataType.Integer, SettingCategory.General, "عدد الخانات العشرية للمبالغ"),
            ("General.SystemTimeZone", "Egypt Standard Time", SettingDataType.String, SettingCategory.General, "المنطقة الزمنية للنظام"),
            ("General.DateDisplayFormat", "yyyy-MM-dd", SettingDataType.String, SettingCategory.General, "تنسيق عرض التاريخ"),
            ("General.TimeDisplayFormat", "HH:mm:ss", SettingDataType.String, SettingCategory.General, "تنسيق عرض الوقت"),
            ("General.FirstDayOfWeek", "Saturday", SettingDataType.String, SettingCategory.General, "اليوم الأول في أسبوع العمل"),

            ("Inventory.LowStockWarningThreshold", "100", SettingDataType.Decimal, SettingCategory.Inventory, "حد التنبيه لانخفاض المخزون"),
            ("Inventory.ExpiryWarningDays", "30", SettingDataType.Integer, SettingCategory.Inventory, "أيام التنبيه المسبق لقرب انتهاء الصلاحية"),
            ("Inventory.AllowNegativeStock", "false", SettingDataType.Boolean, SettingCategory.Inventory, "السماح بالصرف بالسالب"),
            ("Inventory.RequireLotTracking", "true", SettingDataType.Boolean, SettingCategory.Inventory, "إلزامية تتبع أرقام التشغيلات"),

            ("Waste.RequireWasteApproval", "true", SettingDataType.Boolean, SettingCategory.Waste, "إلزامية اعتماد الفاقد"),
            ("Waste.MaxWasteTolerancePercent", "5.0", SettingDataType.Decimal, SettingCategory.Waste, "الحد الأقصى لنسبة الهدر المسموح بها"),

            ("Purchasing.RequirePOApproval", "true", SettingDataType.Boolean, SettingCategory.Purchasing, "إلزامية اعتماد أوامر الشراء"),
            ("Sales.RequireCreditCheck", "true", SettingDataType.Boolean, SettingCategory.Sales, "فحص الحد الائتماني للعميل قبل تأكيد البيع")
        };

        foreach (var (k, v, t, c, d) in defaultSettings)
        {
            var exists = await _context.SystemSettings.AnyAsync(s => s.Key == k);
            if (!exists)
            {
                _context.SystemSettings.Add(new SystemSetting
                {
                    Key = k,
                    Value = v,
                    DataType = t,
                    Category = c,
                    Description = d,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedBy = "SYSTEM"
                });
                _settingsCache[k] = v;
            }
        }
        await _context.SaveChangesAsync();
    }
    #endregion
}
