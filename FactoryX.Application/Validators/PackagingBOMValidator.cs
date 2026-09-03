using FactoryX.Application.DTOs;
using FluentValidation;

namespace FactoryX.Application.Validators;

public class CreatePackagingBOMRequestValidator : AbstractValidator<CreatePackagingBOMRequest>
{
    public CreatePackagingBOMRequestValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("كود مواصفة التعبئة مطلوب.")
            .MaximumLength(50).WithMessage("كود مواصفة التعبئة لا يتجاوز 50 حرفاً.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("اسم مواصفة التعبئة والتغليف مطلوب.")
            .MaximumLength(200).WithMessage("اسم مواصفة التعبئة لا يتجاوز 200 حرفاً.");

        RuleFor(x => x.ProductId)
            .GreaterThan(0).WithMessage("يجب تحديد المنتج التام المرتبط بمواصفة التعبئة.");

        RuleFor(x => x.PackSize)
            .GreaterThan(0).WithMessage("حجم العبوة يجب أن يكون أكبر من الصفر.");

        RuleFor(x => x.PackSizeKg)
            .GreaterThan(0).WithMessage("وزن العبوة الصافي بالكيلوجرام يجب أن يكون أكبر من الصفر.");

        RuleForEach(x => x.Items).SetValidator(new PackagingItemRequestValidator());
    }
}

public class UpdatePackagingBOMRequestValidator : AbstractValidator<UpdatePackagingBOMRequest>
{
    public UpdatePackagingBOMRequestValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("معرف مواصفة التعبئة غير صالح.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("كود مواصفة التعبئة مطلوب.")
            .MaximumLength(50).WithMessage("كود مواصفة التعبئة لا يتجاوز 50 حرفاً.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("اسم مواصفة التعبئة والتغليف مطلوب.")
            .MaximumLength(200).WithMessage("اسم مواصفة التعبئة لا يتجاوز 200 حرفاً.");

        RuleFor(x => x.ProductId)
            .GreaterThan(0).WithMessage("يجب تحديد المنتج التام.");

        RuleFor(x => x.PackSize)
            .GreaterThan(0).WithMessage("حجم العبوة يجب أن يكون أكبر من الصفر.");

        RuleFor(x => x.PackSizeKg)
            .GreaterThan(0).WithMessage("وزن العبوة الصافي بالكيلوجرام يجب أن يكون أكبر من الصفر.");

        RuleForEach(x => x.Items).SetValidator(new PackagingItemRequestValidator());
    }
}

public class CreatePackagingBOMVersionRequestValidator : AbstractValidator<CreatePackagingBOMVersionRequest>
{
    public CreatePackagingBOMVersionRequestValidator()
    {
        RuleFor(x => x.PackagingBOMId)
            .GreaterThan(0).WithMessage("يجب تحديد مواصفة التعبئة والتغليف.");

        RuleFor(x => x.VersionName)
            .NotEmpty().WithMessage("اسم أو مسمى إصدار التعبئة مطلوب.")
            .MaximumLength(100).WithMessage("اسم الإصدار لا يتجاوز 100 حرفاً.");

        RuleFor(x => x.EffectiveFrom)
            .NotEmpty().WithMessage("تاريخ بدء سريان الإصدار مطلوب.");

        RuleForEach(x => x.Items).SetValidator(new PackagingItemRequestValidator());
    }
}

public class PackagingItemRequestValidator : AbstractValidator<PackagingItemRequest>
{
    public PackagingItemRequestValidator()
    {
        RuleFor(x => x.MaterialId)
            .GreaterThan(0).WithMessage("يجب اختيار مادة تعبئة وتغليف صالحة.");

        RuleFor(x => x.QuantityRequired)
            .GreaterThan(0).WithMessage("كمية مادة التعبئة للعبوة الواحدة يجب أن تكون أكبر من الصفر.");

        RuleFor(x => x.Unit)
            .NotEmpty().WithMessage("وحدة قياس مادة التعبئة مطلوبة.");
    }
}
