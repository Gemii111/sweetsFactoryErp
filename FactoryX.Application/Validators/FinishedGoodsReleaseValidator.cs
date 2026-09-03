using FactoryX.Application.DTOs;
using FluentValidation;

namespace FactoryX.Application.Validators;

public class FinishedGoodsReleaseValidator : AbstractValidator<CreateFinishedGoodsReleaseRequest>
{
    public FinishedGoodsReleaseValidator()
    {
        RuleFor(x => x.ProductionBatchId)
            .GreaterThan(0)
            .WithMessage("يجب تحديد دفعة الإنتاج المستهدفة للإفراج.");

        RuleFor(x => x.WarehouseId)
            .GreaterThan(0)
            .WithMessage("يجب تحديد مستودع المنتجات التامة.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("كمية الإفراج للمنتج التام يجب أن تكون أكبر من الصفر.");

        RuleFor(x => x.Notes)
            .MaximumLength(500)
            .WithMessage("الملاحظات يجب ألا تتجاوز 500 حرف.");
    }
}
