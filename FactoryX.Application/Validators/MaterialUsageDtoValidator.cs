using FactoryX.Application.DTOs;
using FluentValidation;

namespace FactoryX.Application.Validators;

public class MaterialUsageDtoValidator : AbstractValidator<MaterialUsageDto>
{
    public MaterialUsageDtoValidator()
    {
        RuleFor(x => x.WorkOrderId).GreaterThan(0);
        RuleFor(x => x.MaterialId).GreaterThan(0);
        RuleFor(x => x.Quantity).GreaterThan(0);
    }
}