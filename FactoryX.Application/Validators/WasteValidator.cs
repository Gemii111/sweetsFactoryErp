using FactoryX.Application.DTOs;
using FactoryX.Domain.Entities;
using FluentValidation;

namespace FactoryX.Application.Validators;

public class CreateWasteRequestValidator : AbstractValidator<CreateWasteRequest>
{
    public CreateWasteRequestValidator()
    {
        RuleFor(w => w.Quantity)
            .GreaterThan(0).WithMessage("كمية الهالك / المرفوضات يجب أن تكون أكبر من الصفر.");

        RuleFor(w => w.Unit)
            .NotEmpty().WithMessage("وحدة القياس مطلوبة.")
            .MaximumLength(20).WithMessage("وحدة القياس يجب ألا تتجاوز 20 حرفاً.");

        RuleFor(w => w.WasteReasonId)
            .NotNull().WithMessage("يجب اختيار سبب الهالك من القائمة المعتمدة.")
            .GreaterThan(0).WithMessage("يجب اختيار سبب الهالك من القائمة المعتمدة.");

        When(w => w.WasteType == WasteType.RawMaterialWaste, () =>
        {
            RuleFor(w => w.MaterialId)
                .NotNull().WithMessage("يجب اختيار المادة الخام لهالك الخامات.")
                .GreaterThan(0).WithMessage("يجب اختيار المادة الخام لهالك الخامات.");

            RuleFor(w => w.WarehouseId)
                .NotNull().WithMessage("يجب تحديد المستودع لهالك الخامات.")
                .GreaterThan(0).WithMessage("يجب تحديد المستودع لهالك الخامات.");
        });

        When(w => w.WasteType == WasteType.ProductionProcessWaste, () =>
        {
            RuleFor(w => w.ProductionBatchId)
                .NotNull().WithMessage("يجب ربط هالك مراحل التشغيل بدفعة إنتاج.")
                .GreaterThan(0).WithMessage("يجب ربط هالك مراحل التشغيل بدفعة إنتاج.");
        });

        When(w => w.WasteType == WasteType.OutputRejection, () =>
        {
            RuleFor(w => w.ProductionBatchId)
                .NotNull().WithMessage("يجب ربط مرفوضات الإنتاج التام بدفعة الإنتاج.")
                .GreaterThan(0).WithMessage("يجب ربط مرفوضات الإنتاج التام بدفعة الإنتاج.");

            RuleFor(w => w.ProductId)
                .NotNull().WithMessage("يجب اختيار المنتج التام للمرفوضات.")
                .GreaterThan(0).WithMessage("يجب اختيار المنتج التام للمرفوضات.");
        });
    }
}

public class UpdateWasteRequestValidator : AbstractValidator<UpdateWasteRequest>
{
    public UpdateWasteRequestValidator()
    {
        RuleFor(w => w.Id)
            .GreaterThan(0).WithMessage("معرف سجل الهالك غير صالح.");

        RuleFor(w => w.Quantity)
            .GreaterThan(0).WithMessage("كمية الهالك / المرفوضات يجب أن تكون أكبر من الصفر.");

        RuleFor(w => w.Unit)
            .NotEmpty().WithMessage("وحدة القياس مطلوبة.");

        RuleFor(w => w.WasteReasonId)
            .NotNull().WithMessage("يجب اختيار سبب الهالك.")
            .GreaterThan(0).WithMessage("يجب اختيار سبب الهالك.");
    }
}

public class RejectWasteRequestValidator : AbstractValidator<RejectWasteRequest>
{
    public RejectWasteRequestValidator()
    {
        RuleFor(r => r.WasteId)
            .GreaterThan(0).WithMessage("معرف سجل الهالك غير صالح.");

        RuleFor(r => r.RejectionReason)
            .NotEmpty().WithMessage("سبب رفض اعتماد الهالك إلزامي.")
            .MaximumLength(1000).WithMessage("سبب الرفض يجب ألا يتجاوز 1000 حرف.");
    }
}
