using FactoryX.Application.DTOs;
using FluentValidation;

namespace FactoryX.Application.Validators;

public class FinishedGoodsAdjustmentValidator : AbstractValidator<FinishedGoodsAdjustmentRequest>
{
    public FinishedGoodsAdjustmentValidator()
    {
        RuleFor(x => x.WarehouseId)
            .GreaterThan(0)
            .WithMessage("يجب تحديد مستودع المنتجات التامة.");

        RuleFor(x => x.ProductId)
            .GreaterThan(0)
            .WithMessage("يجب تحديد المنتج التام.");

        RuleFor(x => x.BatchNumber)
            .NotEmpty()
            .WithMessage("رقم تشغيلة/دفعة المنتج مطلوب.");

        RuleFor(x => x.ActualQuantity)
            .GreaterThanOrEqualTo(0)
            .WithMessage("الكمية الفعلية لا يمكن أن تكون سالبة.");

        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage("سبب التسوية الجردية مطلوب.")
            .MaximumLength(250)
            .WithMessage("سبب التسوية يجب ألا يتجاوز 250 حرف.");
    }
}

public class FinishedGoodsTransferValidator : AbstractValidator<FinishedGoodsTransferRequest>
{
    public FinishedGoodsTransferValidator()
    {
        RuleFor(x => x.ProductId)
            .GreaterThan(0)
            .WithMessage("يجب تحديد المنتج التام.");

        RuleFor(x => x.BatchNumber)
            .NotEmpty()
            .WithMessage("رقم تشغيلة/دفعة المنتج مطلوب.");

        RuleFor(x => x.SourceWarehouseId)
            .GreaterThan(0)
            .WithMessage("يجب تحديد مستودع المصدر.");

        RuleFor(x => x.DestinationWarehouseId)
            .GreaterThan(0)
            .WithMessage("يجب تحديد مستودع الوجهة.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("كمية النقل يجب أن تكون أكبر من الصفر.");

        RuleFor(x => x)
            .Must(x => x.SourceWarehouseId != x.DestinationWarehouseId || x.SourceLocationId != x.DestinationLocationId)
            .WithMessage("لا يمكن النقل إلى نفس المستودع والموقع معاً.");
    }
}
