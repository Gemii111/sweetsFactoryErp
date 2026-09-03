using FactoryX.Domain.Entities;

namespace FactoryX.Application.DTOs;

public class PurchaseRequestItemDto
{
    public int Id { get; set; }
    public int PurchaseRequestId { get; set; }
    public int MaterialId { get; set; }
    public string MaterialName { get; set; } = string.Empty;
    public string MaterialCode { get; set; } = string.Empty;
    public decimal RequestedQuantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public decimal EstimatedUnitPrice { get; set; }
    public decimal EstimatedTotalPrice => RequestedQuantity * EstimatedUnitPrice;
    public DateTime? RequiredDate { get; set; }
    public string Notes { get; set; } = string.Empty;
}

public class PurchaseRequestDto
{
    public int Id { get; set; }
    public string RequestNumber { get; set; } = string.Empty;
    public DateTime RequestDate { get; set; }
    public DateTime? RequiredDate { get; set; }
    public int? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public string Priority { get; set; } = "Normal";
    public PurchaseRequestStatus Status { get; set; }
    public string StatusName => Status.ToString();
    public int RequestedByUserId { get; set; }
    public string? RequestedByName { get; set; }
    public int? ApprovedByUserId { get; set; }
    public string? ApprovedByName { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string Notes { get; set; } = string.Empty;
    public decimal TotalEstimatedCost => Items?.Sum(i => i.EstimatedTotalPrice) ?? 0m;
    public List<PurchaseRequestItemDto> Items { get; set; } = new();
}

public class CreatePurchaseRequestItemRequest
{
    public int MaterialId { get; set; }
    public decimal RequestedQuantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public decimal EstimatedUnitPrice { get; set; }
    public DateTime? RequiredDate { get; set; }
    public string Notes { get; set; } = string.Empty;
}

public class CreatePurchaseRequest
{
    public DateTime RequestDate { get; set; } = DateTime.UtcNow;
    public DateTime? RequiredDate { get; set; }
    public int? DepartmentId { get; set; }
    public string Priority { get; set; } = "Normal";
    public string Notes { get; set; } = string.Empty;
    public List<CreatePurchaseRequestItemRequest> Items { get; set; } = new();
}
