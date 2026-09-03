using FluentValidation;
using FactoryX.Application.DTOs;

namespace FactoryX.Application.Validators;

public class StockTransferRequestValidator : AbstractValidator<StockTransferRequest>
{
    public StockTransferRequestValidator()
    {
        RuleFor(x => x.SourceWarehouseId)
            .GreaterThan(0).WithMessage("Source warehouse is required.");

        RuleFor(x => x.DestinationWarehouseId)
            .GreaterThan(0).WithMessage("Destination warehouse is required.")
            .Must((request, destId) => destId != request.SourceWarehouseId)
            .WithMessage("Source and destination warehouse cannot be the same.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Transfer quantity must be greater than zero.");
    }
}

public class StockAdjustmentRequestValidator : AbstractValidator<StockAdjustmentRequest>
{
    public StockAdjustmentRequestValidator()
    {
        RuleFor(x => x.WarehouseId)
            .GreaterThan(0).WithMessage("Warehouse is required.");

        RuleFor(x => x.ActualQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Actual quantity cannot be negative.");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Adjustment reason is required.");
    }
}
