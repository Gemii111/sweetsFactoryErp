using System;
using FactoryX.Domain.Common;

namespace FactoryX.Domain.Entities;

public enum SettingDataType
{
    String = 1,
    Integer = 2,
    Decimal = 3,
    Boolean = 4,
    Date = 5,
    Time = 6,
    Json = 7
}

public enum SettingCategory
{
    General = 1,
    Company = 2,
    Tax = 3,
    DocumentNumbering = 4,
    Inventory = 5,
    Production = 6,
    Purchasing = 7,
    Sales = 8,
    Packaging = 9,
    Waste = 10,
    Quality = 11,
    Accounting = 12
}

public class SystemSetting : EntityBase
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public SettingDataType DataType { get; set; } = SettingDataType.String;
    public SettingCategory Category { get; set; } = SettingCategory.General;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public string? UpdatedBy { get; set; }
}

public class CompanyProfile : EntityBase
{
    public string CompanyName { get; set; } = "مصنع حلويات المولد الفاخرة";
    public string? LegalName { get; set; } = "شركة حلويات المولد للصناعات الغذائية ذ.م.م";
    public string? CommercialRegistration { get; set; } = "CR-104829";
    public string? TaxRegistrationNumber { get; set; } = "TRN-948-284-110";
    public string? Address { get; set; } = "المنطقة الصناعية الثانية، قطعة 44";
    public string? City { get; set; } = "مدينة السادس من أكتوبر";
    public string? Country { get; set; } = "جمهورية مصر العربية";
    public string? Phone { get; set; } = "+20 2 38330000";
    public string? Email { get; set; } = "info@mawlidsweets.com";
    public string? Website { get; set; } = "https://www.mawlidsweets.com";
    public string? LogoUrl { get; set; }
    public string DefaultCurrency { get; set; } = "EGP";
    public string DefaultTimeZone { get; set; } = "Egypt Standard Time";
    public string? UpdatedBy { get; set; }
}

public class TaxSetting : EntityBase
{
    public string Name { get; set; } = "ضريبة القيمة المضافة";
    public string Code { get; set; } = "VAT_14";
    public decimal Rate { get; set; } = 14.00m;
    public DateTime EffectiveFrom { get; set; } = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    public DateTime? EffectiveTo { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDefault { get; set; } = true;
    public string? Description { get; set; } = "الضريبة العامة على القيمة المضافة المقررة قانوناً 14%";
    public string? UpdatedBy { get; set; }
}

public class DocumentNumberSetting : EntityBase
{
    public string DocumentType { get; set; } = string.Empty; // e.g. "SalesInvoice", "PurchaseOrder", "JournalEntry"
    public string DocumentTypeNameArabic { get; set; } = string.Empty; // e.g. "فاتورة مبيعات"
    public string Prefix { get; set; } = string.Empty; // e.g. "INV", "PO", "JE"
    public string DateFormat { get; set; } = "yyyyMMdd"; // e.g. "yyyyMMdd", "yyyyMM", "yyyy", ""
    public int SequenceWidth { get; set; } = 4; // e.g. 4 -> "0001"
    public int NextSequenceValue { get; set; } = 1;
    public string Delimiter { get; set; } = "-";
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public string? UpdatedBy { get; set; }
}
