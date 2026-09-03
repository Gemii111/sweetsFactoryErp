using FactoryX.Application.DTOs;
using FluentValidation;

namespace FactoryX.Application.Validators;

public class CreateWasteReasonRequestValidator : AbstractValidator<CreateWasteReasonRequest>
{
    public CreateWasteReasonRequestValidator()
    {
        RuleFor(r => r.Code)
            .NotEmpty().WithMessage("كود سبب الهالك مطلوب.")
            .MaximumLength(50).WithMessage("كود سبب الهالك يجب ألا يتجاوز 50 حرفاً.");

        RuleFor(r => r.Reason)
            .NotEmpty().WithMessage("اسم / وصف سبب الهالك مطلوب.")
            .MaximumLength(200).WithMessage("اسم سبب الهالك يجب ألا يتجاوز 200 حرف.");

        RuleFor(r => r.Description)
            .MaximumLength(500).WithMessage("الوصف التفصيلي يجب ألا يتجاوز 500 حرف.");
    }
}

public class UpdateWasteReasonRequestValidator : AbstractValidator<UpdateWasteReasonRequest>
{
    public UpdateWasteReasonRequestValidator()
    {
        RuleFor(r => r.Id)
            .GreaterThan(0).WithMessage("معرف سبب الهالك غير صالح.");

        RuleFor(r => r.Code)
            .NotEmpty().WithMessage("كود سبب الهالك مطلوب.")
            .MaximumLength(50).WithMessage("كود سبب الهالك يجب ألا يتجاوز 50 حرفاً.");

        RuleFor(r => r.Reason)
            .NotEmpty().WithMessage("اسم / وصف سبب الهالك مطلوب.")
            .MaximumLength(200).WithMessage("اسم سبب الهالك يجب ألا يتجاوز 200 حرف.");

        RuleFor(r => r.Description)
            .MaximumLength(500).WithMessage("الوصف التفصيلي يجب ألا يتجاوز 500 حرف.");
    }
}
