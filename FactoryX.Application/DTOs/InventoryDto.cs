using FactoryX.Domain.Entities;

namespace FactoryX.Application.DTOs;

public class StockBalanceDto
{
    public int Id { get; set; }
    public int WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public int? LocationId { get; set; }
    public string LocationName { get; set; } = string.Empty;
    
    public int? MaterialId { get; set; }
    public string MaterialName { get; set; } = string.Empty;
    public string MaterialCode { get; set; } = string.Empty;

    public int? ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductCode { get; set; } = string.Empty;

    public string ItemName => !string.IsNullOrEmpty(MaterialName) ? MaterialName : ProductName;
    public string ItemCode => !string.IsNullOrEmpty(MaterialCode) ? MaterialCode : ProductCode;
    
    public string BatchNumber { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public DateTime? ManufacturingDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
}

public class InventoryTransactionDto
{
    public int Id { get; set; }
    public InventoryTransactionType TransactionType { get; set; }
    public string TransactionTypeName => TransactionType.ToString();
    public DateTime TransactionDate { get; set; }

    public int? MaterialId { get; set; }
    public string MaterialName { get; set; } = string.Empty;
    public int? ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    
    public string ItemName => !string.IsNullOrEmpty(MaterialName) ? MaterialName : ProductName;
    public string BatchNumber { get; set; } = string.Empty;

    public int WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public int? SourceLocationId { get; set; }
    public string SourceLocationName { get; set; } = string.Empty;
    public int? DestinationLocationId { get; set; }
    public string DestinationLocationName { get; set; } = string.Empty;

    public decimal Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public decimal UnitCost { get; set; }
    public decimal TotalCost { get; set; }

    public string ReferenceDocumentNumber { get; set; } = string.Empty;
    public int? UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}

public record StockTransferRequest(
    int SourceWarehouseId,
    int? SourceLocationId,
    int DestinationWarehouseId,
    int? DestinationLocationId,
    int? MaterialId,
    int? ProductId,
    string BatchNumber,
    decimal Quantity,
    string Unit,
    string ReferenceNumber,
    string Notes);

public record StockAdjustmentRequest(
    int WarehouseId,
    int? LocationId,
    int? MaterialId,
    int? ProductId,
    string BatchNumber,
    decimal ActualQuantity, // Physical count
    string Unit,
    string Reason,
    string Notes);
