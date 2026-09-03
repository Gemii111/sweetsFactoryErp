using FactoryX.Application.DTOs;
using FluentValidation;

namespace FactoryX.Application.Validators;

public class CreateQualityInspectionRequestValidator : AbstractValidator<CreateQualityInspectionRequest>
{
    public CreateQualityInspectionRequestValidator()
    {
        RuleFor(q => q.ProductionBatchId)
            .GreaterThan(0).WithMessage("يجب تحديد دفعة الإنتاج المراد فحصها.");
    }
}

public class RejectInspectionRequestValidator : AbstractValidator<RejectInspectionRequest>
{
    public RejectInspectionRequestValidator()
    {
        RuleFor(r => r.InspectionId)
            .GreaterThan(0).WithMessage("معرف فحص الجودة غير صالح.");

        RuleFor(r => r.RejectionReason)
            .NotEmpty().WithMessage("سبب رفض دفعة الإنتاج إلزامي.")
            .MaximumLength(1000).WithMessage("سبب الرفض يجب ألا يتجاوز 1000 حرف.");
    }
}

public class HoldInspectionRequestValidator : AbstractValidator<HoldInspectionRequest>
{
    public HoldInspectionRequestValidator()
    {
        RuleFor(h => h.InspectionId)
            .GreaterThan(0).WithMessage("معرف فحص الجودة غير صالح.");

        RuleFor(h => h.HoldReason)
            .NotEmpty().WithMessage("سبب تعليق (Hold) دفعة الإنتاج إلزامي.")
            .MaximumLength(1000).WithMessage("سبب التعليق يجب ألا يتجاوز 1000 حرف.");
    }
}

public class ReinspectRequestValidator : AbstractValidator<ReinspectRequest>
{
    public ReinspectRequestValidator()
    {
        RuleFor(r => r.PreviousInspectionId)
            .GreaterThan(0).WithMessage("معرف الفحص السابق غير صالح.");

        RuleFor(r => r.ReinspectionReason)
            .NotEmpty().WithMessage("سبب إعادة الفحص (Re-inspection) إلزامي.")
            .MaximumLength(500).WithMessage("سبب إعادة الفحص يجب ألا يتجاوز 500 حرف.");
    }
}
