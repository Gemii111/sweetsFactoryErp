using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FactoryX.Domain.Entities;

namespace FactoryX.Application.DTOs;

public record SystemSettingDto(
    int Id,
    string Key,
    string Value,
    SettingDataType DataType,
    SettingCategory Category,
    string? Description,
    bool IsActive,
    DateTime UpdatedAt,
    string? UpdatedBy);

public class SystemSettingUpdateDto
{
    [Required(ErrorMessage = "مفتاح الإعداد مطلوب")]
    public string Key { get; set; } = string.Empty;

    public string Value { get; set; } = string.Empty;
    public SettingDataType DataType { get; set; } = SettingDataType.String;
    public SettingCategory Category { get; set; } = SettingCategory.General;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

public class CompanyProfileDto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "اسم الشركة التجاري مطلوب")]
    [Display(Name = "اسم المصنع / الشركة")]
    public string CompanyName { get; set; } = "مصنع حلويات المولد الفاخرة";

    [Display(Name = "الاسم القانوني للشركة")]
    public string? LegalName { get; set; } = "شركة حلويات المولد للصناعات الغذائية ذ.م.م";

    [Display(Name = "رقم السجل التجاري")]
    public string? CommercialRegistration { get; set; } = "CR-104829";

    [Display(Name = "رقم التسجيل الضريبي")]
    public string? TaxRegistrationNumber { get; set; } = "TRN-948-284-110";

    [Display(Name = "العنوان")]
    public string? Address { get; set; } = "المنطقة الصناعية الثانية، قطعة 44";

    [Display(Name = "المدينة")]
    public string? City { get; set; } = "مدينة السادس من أكتوبر";

    [Display(Name = "الدولة")]
    public string? Country { get; set; } = "جمهورية مصر العربية";

    [Display(Name = "رقم الهاتف")]
    public string? Phone { get; set; } = "+20 2 38330000";

    [EmailAddress(ErrorMessage = "صيغة البريد الإلكتروني غير صحيحة")]
    [Display(Name = "البريد الإلكتروني")]
    public string? Email { get; set; } = "info@mawlidsweets.com";

    [Display(Name = "الموقع الإلكتروني")]
    public string? Website { get; set; } = "https://www.mawlidsweets.com";

    public string? LogoUrl { get; set; }

    [Required(ErrorMessage = "العملة الافتراضية مطلوبة")]
    [Display(Name = "العملة الافتراضية")]
    public string DefaultCurrency { get; set; } = "EGP";

    [Required(ErrorMessage = "المنطقة الزمنية مطلوبة")]
    [Display(Name = "المنطقة الزمنية")]
    public string DefaultTimeZone { get; set; } = "Egypt Standard Time";

    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}

public class TaxSettingDto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "اسم الضريبة مطلوب")]
    [Display(Name = "اسم الضريبة")]
    public string Name { get; set; } = "ضريبة القيمة المضافة";

    [Required(ErrorMessage = "رمز الضريبة مطلوب")]
    [Display(Name = "رمز الضريبة")]
    public string Code { get; set; } = "VAT_14";

    [Range(0, 100, ErrorMessage = "نسبة الضريبة يجب أن تكون بين 0% و 100%")]
    [Display(Name = "النسبة المئوية (%)")]
    public decimal Rate { get; set; } = 14.00m;

    [Required(ErrorMessage = "تاريخ بدء السريان مطلوب")]
    [Display(Name = "تاريخ السريان")]
    public DateTime EffectiveFrom { get; set; } = new DateTime(2020, 1, 1);

    [Display(Name = "تاريخ الانتهاء (اختياري)")]
    public DateTime? EffectiveTo { get; set; }

    [Display(Name = "نشط")]
    public bool IsActive { get; set; } = true;

    [Display(Name = "الضريبة الافتراضية")]
    public bool IsDefault { get; set; } = true;

    [Display(Name = "الوصف")]
    public string? Description { get; set; }

    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}

public class DocumentNumberSettingDto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "نوع المستند مطلوب")]
    public string DocumentType { get; set; } = string.Empty;

    public string DocumentTypeNameArabic { get; set; } = string.Empty;

    [Required(ErrorMessage = "بادئة الترقيم مطلوبة")]
    [StringLength(10, MinimumLength = 1, ErrorMessage = "طول البادئة يجب أن يكون بين 1 و 10 أحرف")]
    [Display(Name = "بادئة الترقيم (Prefix)")]
    public string Prefix { get; set; } = string.Empty;

    [Display(Name = "تنسيق التاريخ (Date Format)")]
    public string DateFormat { get; set; } = "yyyyMMdd";

    [Range(1, 10, ErrorMessage = "طول خانات التسلسل يجب أن يكون بين 1 و 10")]
    [Display(Name = "عدد خانات التسلسل (Width)")]
    public int SequenceWidth { get; set; } = 4;

    [Range(1, int.MaxValue, ErrorMessage = "قيمة التسلسل القادمة يجب أن تكون أكبر من 0")]
    [Display(Name = "الرقم التسلسلي التالي (Next Sequence)")]
    public int NextSequenceValue { get; set; } = 1;

    [Display(Name = "الفاصل (Delimiter)")]
    public string Delimiter { get; set; } = "-";

    [Display(Name = "الوصف")]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}

public class OperationalDefaultsDto
{
    [Display(Name = "مستودع الخامات الافتراضي")]
    public int? DefaultRawMaterialWarehouseId { get; set; }

    [Display(Name = "مستودع الإنتاج والتشغيل الافتراضي")]
    public int? DefaultProductionWarehouseId { get; set; }

    [Display(Name = "مستودع التعبئة والتغليف الافتراضي")]
    public int? DefaultPackagingWarehouseId { get; set; }

    [Display(Name = "مستودع المنتجات التامة الافتراضي")]
    public int? DefaultFinishedGoodsWarehouseId { get; set; }

    [Display(Name = "مستودع الحجر والفحص (QC) الافتراضي")]
    public int? DefaultQuarantineWarehouseId { get; set; }

    [Range(0, 1000000, ErrorMessage = "حد انخفاض المخزون يجب أن يكون قيمة موجبة")]
    [Display(Name = "حد التنبيه لانخفاض المخزون (Low Stock Threshold)")]
    public decimal LowStockWarningThreshold { get; set; } = 100m;

    [Range(1, 365, ErrorMessage = "أيام تنبيه الصلاحية يجب أن تكون بين 1 و 365 يوماً")]
    [Display(Name = "أيام التنبيه المسبق لقرب انتهاء الصلاحية")]
    public int ExpiryWarningDays { get; set; } = 30;

    [Display(Name = "السماح بالصرف بالسالب (Negative Stock)")]
    public bool AllowNegativeStock { get; set; } = false;

    [Display(Name = "إلزامية تتبع أرقام التشغيلات (Lot Tracking)")]
    public bool RequireLotTracking { get; set; } = true;

    [Display(Name = "إلزامية اعتماد الفاقد (Waste Approval Requirement)")]
    public bool RequireWasteApproval { get; set; } = true;

    [Range(0, 100, ErrorMessage = "نسبة التسامح في هدر الإنتاج يجب أن تكون بين 0% و 100%")]
    [Display(Name = "الحد الأقصى لنسبة الهدر المسموح بها (%)")]
    public decimal MaxWasteTolerancePercent { get; set; } = 5.0m;
}

public class GeneralSettingsDto
{
    [Required(ErrorMessage = "رمز العملة مطلوب")]
    [Display(Name = "رمز العملة الأساسية")]
    public string CurrencyCode { get; set; } = "EGP";

    [Required(ErrorMessage = "اسم العملة مطلوب")]
    [Display(Name = "اسم العملة")]
    public string CurrencyName { get; set; } = "الجنيه المصري";

    [Required(ErrorMessage = "رمز العملة المختصر مطلوب")]
    [Display(Name = "رمز العملة (Symbol)")]
    public string CurrencySymbol { get; set; } = "ج.م";

    [Range(0, 4, ErrorMessage = "عدد الخانات العشرية يجب أن يكون بين 0 و 4")]
    [Display(Name = "عدد الخانات العشرية للمبالغ")]
    public int CurrencyDecimalPrecision { get; set; } = 2;

    [Required(ErrorMessage = "المنطقة الزمنية مطلوبة")]
    [Display(Name = "المنطقة الزمنية للنظام")]
    public string SystemTimeZone { get; set; } = "Egypt Standard Time";

    [Required(ErrorMessage = "تنسيق عرض التاريخ مطلوب")]
    [Display(Name = "تنسيق عرض التاريخ")]
    public string DateDisplayFormat { get; set; } = "yyyy-MM-dd";

    [Required(ErrorMessage = "تنسيق عرض الوقت مطلوب")]
    [Display(Name = "تنسيق عرض الوقت")]
    public string TimeDisplayFormat { get; set; } = "HH:mm:ss";

    [Display(Name = "اليوم الأول في أسبوع العمل")]
    public string FirstDayOfWeek { get; set; } = "Saturday";
}
