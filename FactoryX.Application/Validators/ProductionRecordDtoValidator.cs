using FactoryX.Application.DTOs;
using FluentValidation;

namespace FactoryX.Application.Validators;

public class ProductionRecordDtoValidator : AbstractValidator<ProductionRecordDto>
{
    public ProductionRecordDtoValidator()
    {
        RuleFor(x => x.WorkOrderId).GreaterThan(0);
        RuleFor(x => x.OperatorId).GreaterThan(0);
        RuleFor(x => x.QuantityProduced).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Timestamp).NotEmpty();
    }
}