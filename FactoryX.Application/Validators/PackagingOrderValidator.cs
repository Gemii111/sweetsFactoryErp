using FactoryX.Application.DTOs;
using FluentValidation;

namespace FactoryX.Application.Validators;

public class CreatePackagingOrderRequestValidator : AbstractValidator<CreatePackagingOrderRequest>
{
    public CreatePackagingOrderRequestValidator()
    {
        RuleFor(x => x.ProductionBatchId)
            .GreaterThan(0).WithMessage("يجب تحديد دفعة الإنتاج المراد تعبئتها.");

        RuleFor(x => x.PackagingBOMId)
            .GreaterThan(0).WithMessage("يجب تحديد مواصفة التعبئة والتغليف (Packaging BOM).");

        RuleFor(x => x.PlannedQuantity)
            .GreaterThan(0).WithMessage("الكمية المخططة للعبوات يجب أن تكون أكبر من الصفر.");
    }
}

public class ExecutePackagingOrderRequestValidator : AbstractValidator<ExecutePackagingOrderRequest>
{
    public ExecutePackagingOrderRequestValidator()
    {
        RuleFor(x => x.PackagingOrderId)
            .GreaterThan(0).WithMessage("معرف أمر التعبئة غير صالح.");

        RuleFor(x => x.ActualPackagedQuantity)
            .GreaterThan(0).WithMessage("الكمية المعبأة الفعلية يجب أن تكون أكبر من الصفر.");

        RuleFor(x => x.WarehouseId)
            .GreaterThan(0).WithMessage("يجب تحديد مستودع مواد التعبئة والتغليف.");
    }
}
