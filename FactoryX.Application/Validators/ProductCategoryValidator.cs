using FactoryX.Application.DTOs;
using FluentValidation;

namespace FactoryX.Application.Validators;

public class CreateProductCategoryRequestValidator : AbstractValidator<CreateProductCategoryRequest>
{
    public CreateProductCategoryRequestValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("كود التصنيف مطلوب ولا يمكن تركه فارغاً.")
            .MaximumLength(50).WithMessage("كود التصنيف يجب ألا يتجاوز 50 حرفاً.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("اسم التصنيف مطلوب ولا يمكن تركه فارغاً.")
            .MaximumLength(150).WithMessage("اسم التصنيف يجب ألا يتجاوز 150 حرفاً.");

        RuleFor(x => x.ArabicName)
            .MaximumLength(150).WithMessage("الاسم العربي للتصنيف يجب ألا يتجاوز 150 حرفاً.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("الوصف يجب ألا يتجاوز 500 حرف.");
    }
}

public class UpdateProductCategoryRequestValidator : AbstractValidator<UpdateProductCategoryRequest>
{
    public UpdateProductCategoryRequestValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("معرف التصنيف غير صالح.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("كود التصنيف مطلوب ولا يمكن تركه فارغاً.")
            .MaximumLength(50).WithMessage("كود التصنيف يجب ألا يتجاوز 50 حرفاً.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("اسم التصنيف مطلوب ولا يمكن تركه فارغاً.")
            .MaximumLength(150).WithMessage("اسم التصنيف يجب ألا يتجاوز 150 حرفاً.");

        RuleFor(x => x.ArabicName)
            .MaximumLength(150).WithMessage("الاسم العربي للتصنيف يجب ألا يتجاوز 150 حرفاً.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("الوصف يجب ألا يتجاوز 500 حرف.");
    }
}
