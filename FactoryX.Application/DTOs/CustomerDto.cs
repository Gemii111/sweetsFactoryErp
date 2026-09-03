using FactoryX.Domain.Entities;

namespace FactoryX.Application.DTOs;

public class CustomerDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? ArabicName { get; set; }
    public CustomerType Type { get; set; }
    public string TypeName => Type switch
    {
        CustomerType.Wholesale => "جملة (Wholesale)",
        CustomerType.Retail => "تجزئة (Retail)",
        CustomerType.Distributor => "موزع معتمد (Distributor)",
        CustomerType.Supermarket => "هايبر / سوبرماركت (Supermarket)",
        CustomerType.Corporate => "شركات ومؤسسات (Corporate)",
        _ => "أخرى (Other)"
    };

    public string? ContactPerson { get; set; }
    public string? Phone { get; set; }
    public string? Mobile { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? TaxNumber { get; set; }
    public string? Notes { get; set; }
    public decimal CreditLimit { get; set; }
    public decimal CurrentBalance { get; set; }
    public bool IsActive { get; set; } = true;

    public int SalesOrdersCount { get; set; }
    public int SalesFulfillmentsCount { get; set; }
    public decimal TotalSalesValue { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public List<SalesOrderDto> RecentSalesOrders { get; set; } = new();
}

public class CreateCustomerRequest
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? ArabicName { get; set; }
    public CustomerType Type { get; set; } = CustomerType.Wholesale;
    public string? ContactPerson { get; set; }
    public string? Phone { get; set; }
    public string? Mobile { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? TaxNumber { get; set; }
    public string? Notes { get; set; }
    public decimal CreditLimit { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateCustomerRequest
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? ArabicName { get; set; }
    public CustomerType Type { get; set; }
    public string? ContactPerson { get; set; }
    public string? Phone { get; set; }
    public string? Mobile { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? TaxNumber { get; set; }
    public string? Notes { get; set; }
    public decimal CreditLimit { get; set; }
    public bool IsActive { get; set; }
}

public class CustomerSummaryDto
{
    public int TotalCustomers { get; set; }
    public int ActiveCustomers { get; set; }
    public int InactiveCustomers { get; set; }
    public int WholesaleCount { get; set; }
    public int RetailCount { get; set; }
    public int DistributorCount { get; set; }
}
