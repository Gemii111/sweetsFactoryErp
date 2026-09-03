using FactoryX.Application.DTOs;
using FluentValidation;

namespace FactoryX.Application.Validators;

public class CreateQualityTemplateRequestValidator : AbstractValidator<CreateQualityTemplateRequest>
{
    public CreateQualityTemplateRequestValidator()
    {
        RuleFor(t => t.Code)
            .NotEmpty().WithMessage("كود قالب الفحص مطلوب.")
            .MaximumLength(50).WithMessage("كود قالب الفحص يجب ألا يتجاوز 50 حرفاً.");

        RuleFor(t => t.Name)
            .NotEmpty().WithMessage("اسم قالب الفحص مطلوب.")
            .MaximumLength(200).WithMessage("اسم قالب الفحص يجب ألا يتجاوز 200 حرف.");

        RuleFor(t => t.Description)
            .MaximumLength(500).WithMessage("الوصف يجب ألا يتجاوز 500 حرف.");

        RuleFor(t => t.Items)
            .NotEmpty().WithMessage("يجب إضافة معيار فحص واحد على الأقل في القالب.");

        RuleForEach(t => t.Items).SetValidator(new CreateQualityTemplateItemRequestValidator());
    }
}

public class CreateQualityTemplateItemRequestValidator : AbstractValidator<CreateQualityTemplateItemRequest>
{
    public CreateQualityTemplateItemRequestValidator()
    {
        RuleFor(i => i.SpecificationName)
            .NotEmpty().WithMessage("اسم معيار / مواصفة الفحص مطلوب.")
            .MaximumLength(200).WithMessage("اسم معيار الفحص يجب ألا يتجاوز 200 حرف.");

        RuleFor(i => i.Sequence)
            .GreaterThan(0).WithMessage("ترتيب المعيار يجب أن يكون أكبر من الصفر.");

        When(i => i.MinValue.HasValue && i.MaxValue.HasValue, () =>
        {
            RuleFor(i => i.MinValue)
                .LessThanOrEqualTo(i => i.MaxValue!.Value)
                .WithMessage("الحد الأدنى للمعيار يجب أن يكون أقل من أو يساوي الحد الأقصى.");
        });
    }
}

public class UpdateQualityTemplateRequestValidator : AbstractValidator<UpdateQualityTemplateRequest>
{
    public UpdateQualityTemplateRequestValidator()
    {
        RuleFor(t => t.Id)
            .GreaterThan(0).WithMessage("معرف قالب الفحص غير صالح.");

        RuleFor(t => t.Code)
            .NotEmpty().WithMessage("كود قالب الفحص مطلوب.")
            .MaximumLength(50).WithMessage("كود قالب الفحص يجب ألا يتجاوز 50 حرفاً.");

        RuleFor(t => t.Name)
            .NotEmpty().WithMessage("اسم قالب الفحص مطلوب.")
            .MaximumLength(200).WithMessage("اسم قالب الفحص يجب ألا يتجاوز 200 حرف.");

        RuleFor(t => t.Items)
            .NotEmpty().WithMessage("يجب أن يحتوي القالب على معيار فحص واحد على الأقل.");

        RuleForEach(t => t.Items).SetValidator(new CreateQualityTemplateItemRequestValidator());
    }
}
