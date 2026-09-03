using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FactoryX.Application.DTOs;
using FactoryX.Domain.Entities;

namespace FactoryX.Application.Services.Abstracts;

public interface ISettingsService
{
    // Generic typed settings
    Task<string?> GetSettingValueAsync(string key);
    Task<T> GetSettingValueAsync<T>(string key, T defaultValue);
    Task SetSettingValueAsync(string key, string value, string updatedBy, SettingDataType dataType = SettingDataType.String, SettingCategory category = SettingCategory.General, string? description = null);
    Task<Dictionary<string, string>> GetSettingsByCategoryAsync(SettingCategory category);
    Task<IEnumerable<SystemSettingDto>> GetAllSettingsAsync();

    // Company Profile
    Task<CompanyProfileDto> GetCompanyProfileAsync();
    Task UpdateCompanyProfileAsync(CompanyProfileDto dto, string updatedBy);

    // General & Regional Settings
    Task<GeneralSettingsDto> GetGeneralSettingsAsync();
    Task UpdateGeneralSettingsAsync(GeneralSettingsDto dto, string updatedBy);

    // Tax Settings
    Task<TaxSettingDto> GetCurrentTaxSettingAsync();
    Task<IEnumerable<TaxSettingDto>> GetAllTaxSettingsAsync();
    Task SaveTaxSettingAsync(TaxSettingDto dto, string updatedBy);
    Task ToggleTaxSettingStatusAsync(int id, bool isActive, string updatedBy);

    // Document Numbering
    Task<IEnumerable<DocumentNumberSettingDto>> GetDocumentNumberSettingsAsync();
    Task<DocumentNumberSettingDto?> GetDocumentNumberSettingByTypeAsync(string documentType);
    Task SaveDocumentNumberSettingAsync(DocumentNumberSettingDto dto, string updatedBy);
    Task<string> GenerateDocumentNumberAsync(string documentType, DateTime? date = null);

    // Operational Defaults (Warehouses, Stock Controls, Tolerances)
    Task<OperationalDefaultsDto> GetOperationalDefaultsAsync();
    Task SaveOperationalDefaultsAsync(OperationalDefaultsDto dto, string updatedBy);

    // Account Mappings (GL Integration)
    Task<IEnumerable<AccountingSettingDto>> GetAccountMappingsAsync();
    Task SaveAccountMappingAsync(AccountingSettingUpdateDto dto, string updatedBy);

    // Configuration Audit History
    Task<IEnumerable<AuditLogItemDto>> GetConfigurationHistoryAsync(int limit = 100);

    // Initialization & Seeding
    Task SeedDefaultConfigurationAsync();
}

