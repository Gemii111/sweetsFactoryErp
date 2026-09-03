using FactoryX.Domain.Entities;

namespace FactoryX.Application.DTOs;

public class FinishedGoodsStockDto
{
    public int Id { get; set; }

    public int ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductSKU { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string? ProductArabicName { get; set; }
    public string? ProductCategoryName { get; set; }

    public int ProductionBatchId { get; set; }
    public string BatchNumber { get; set; } = string.Empty;

    public int WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public string WarehouseCode { get; set; } = string.Empty;

    public int? LocationId { get; set; }
    public string? LocationName { get; set; }
    public string? LocationCode { get; set; }
    public string? LocationSection { get; set; }

    public decimal Quantity { get; set; }
    public string Unit { get; set; } = "KG";

    public DateTime ProductionDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public bool IsExpired => DateTime.UtcNow > ExpiryDate;
    public int DaysUntilExpiry => (int)(ExpiryDate - DateTime.UtcNow).TotalDays;

    public decimal UnitCost { get; set; }
    public decimal TotalCost { get; set; }

    // Traceability Linkages
    public int? QCInspectionId { get; set; }
    public string? QCInspectionNumber { get; set; }
    public string? QCStatus { get; set; }

    public int? PackagingOrderId { get; set; }
    public string? PackagingOrderNumber { get; set; }
    public string? PackagingBOMName { get; set; }

    public int? WorkOrderId { get; set; }
    public string? WorkOrderNumber { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class FinishedGoodsStockSummaryDto
{
    public decimal TotalQuantity { get; set; }
    public decimal TotalValue { get; set; }
    public int TotalBatchesCount { get; set; }
    public int TotalProductsCount { get; set; }
    public int ExpiringSoonCount { get; set; }
    public int ExpiredCount { get; set; }
}

public class FinishedGoodsMovementDto
{
    public int Id { get; set; }
    public InventoryTransactionType TransactionType { get; set; }
    public string TransactionTypeName => TransactionType switch
    {
        InventoryTransactionType.FinishedGoodsReceipt => "استلام منتج تام (FG Receipt)",
        InventoryTransactionType.FinishedGoodsAdjustment => "تسوية جردية (FG Adjustment)",
        InventoryTransactionType.FinishedGoodsTransfer => "نقل داخلي بين المستودعات (FG Transfer)",
        _ => TransactionType.ToString()
    };

    public DateTime TransactionDate { get; set; }
    public int? ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductCode { get; set; } = string.Empty;
    public string BatchNumber { get; set; } = string.Empty;

    public int WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public int? SourceLocationId { get; set; }
    public string? SourceLocationName { get; set; }
    public int? DestinationLocationId { get; set; }
    public string? DestinationLocationName { get; set; }

    public decimal Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public decimal UnitCost { get; set; }
    public decimal TotalCost { get; set; }

    public string ReferenceDocumentNumber { get; set; } = string.Empty;
    public int? UserId { get; set; }
    public string? UserName { get; set; }
    public string Notes { get; set; } = string.Empty;
}

public class FinishedGoodsAdjustmentRequest
{
    public int WarehouseId { get; set; }
    public int? LocationId { get; set; }
    public int ProductId { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public decimal ActualQuantity { get; set; }
    public string Unit { get; set; } = "KG";
    public string Reason { get; set; } = string.Empty;
}

public class FinishedGoodsTransferRequest
{
    public int ProductId { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public int SourceWarehouseId { get; set; }
    public int? SourceLocationId { get; set; }
    public int DestinationWarehouseId { get; set; }
    public int? DestinationLocationId { get; set; }
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = "KG";
    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }
}
