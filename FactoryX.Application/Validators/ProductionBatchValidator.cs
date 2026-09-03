using FactoryX.Application.DTOs;
using FluentValidation;

namespace FactoryX.Application.Validators;

public class CreateProductionBatchRequestValidator : AbstractValidator<CreateProductionBatchRequest>
{
    public CreateProductionBatchRequestValidator()
    {
        RuleFor(x => x.WorkOrderId)
            .GreaterThan(0).WithMessage("يجب تحديد أمر إنتاج معتمد ومطلق لتشغيل الدفعة.");

        RuleFor(x => x.PlannedQuantity)
            .GreaterThan(0).WithMessage("الكمية المخططة للتشغيلة يجب أن تكون أكبر من الصفر.");

        RuleFor(x => x.OutputUnit)
            .NotEmpty().WithMessage("وحدة الإنتاج مطلوبة.")
            .MaximumLength(30).WithMessage("وحدة الإنتاج يجب ألا تتجاوز 30 حرفاً.");

        RuleFor(x => x.ProductionDate)
            .NotEmpty().WithMessage("تاريخ إنتاج الدفعة مطلوب.");

        RuleFor(x => x.BatchNumber)
            .MaximumLength(50).WithMessage("رقم الدفعة يجب ألا يتجاوز 50 حرفاً.");

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("الملاحظات يجب ألا تتجاوز 500 حرف.");
    }
}

public class StartBatchRequestValidator : AbstractValidator<StartBatchRequest>
{
    public StartBatchRequestValidator()
    {
        RuleFor(x => x.BatchId)
            .GreaterThan(0).WithMessage("معرف دفعة الإنتاج غير صالح.");

        RuleFor(x => x.Consumptions)
            .NotEmpty().WithMessage("يجب تحديد خامات التشغيل ومستودعات الصرف لبدء التشغيل.");

        RuleForEach(x => x.Consumptions).ChildRules(item =>
        {
            item.RuleFor(c => c.MaterialId)
                .GreaterThan(0).WithMessage("معرف الخامة غير صالح.");

            item.RuleFor(c => c.WarehouseId)
                .GreaterThan(0).WithMessage("يجب اختيار مستودع الصرف لكل خامة.");

            item.RuleFor(c => c.ActualQuantity)
                .GreaterThan(0).WithMessage("كمية الصرف للخامة يجب أن تكون أكبر من الصفر.");
        });
    }
}

public class CompleteBatchRequestValidator : AbstractValidator<CompleteBatchRequest>
{
    public CompleteBatchRequestValidator()
    {
        RuleFor(x => x.BatchId)
            .GreaterThan(0).WithMessage("معرف دفعة الإنتاج غير صالح.");

        RuleFor(x => x.ActualOutputQuantity)
            .GreaterThan(0).WithMessage("الكمية الفعلية المنتجة يجب أن تكون أكبر من الصفر.");
    }
}

public class CancelBatchRequestValidator : AbstractValidator<CancelBatchRequest>
{
    public CancelBatchRequestValidator()
    {
        RuleFor(x => x.BatchId)
            .GreaterThan(0).WithMessage("معرف دفعة الإنتاج غير صالح.");

        RuleFor(x => x.CancellationReason)
            .NotEmpty().WithMessage("سبب إلغاء الدفعة مطلوب.")
            .MaximumLength(500).WithMessage("سبب الإلغاء يجب ألا يتجاوز 500 حرف.");
    }
}
