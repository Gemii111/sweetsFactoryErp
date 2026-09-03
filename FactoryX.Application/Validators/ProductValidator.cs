using FactoryX.Application.DTOs;
using FluentValidation;

namespace FactoryX.Application.Validators;

public class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductRequestValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("كود المنتج مطلوب ولا يمكن تركه فارغاً.")
            .MaximumLength(50).WithMessage("كود المنتج يجب ألا يتجاوز 50 حرفاً.");

        RuleFor(x => x.SKU)
            .NotEmpty().WithMessage("رمز التخزين (SKU) مطلوب ولا يمكن تركه فارغاً.")
            .MaximumLength(50).WithMessage("رمز الصنف (SKU) يجب ألا يتجاوز 50 حرفاً.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("اسم المنتج مطلوب ولا يمكن تركه فارغاً.")
            .MaximumLength(150).WithMessage("اسم المنتج يجب ألا يتجاوز 150 حرفاً.");

        RuleFor(x => x.ArabicName)
            .MaximumLength(150).WithMessage("الاسم العربي للمنتج يجب ألا يتجاوز 150 حرفاً.");

        RuleFor(x => x.ProductCategoryId)
            .NotNull().WithMessage("تصنيف المنتج مطلوب.")
            .GreaterThan(0).WithMessage("يجب اختيار تصنيف صالح للمنتج.");

        RuleFor(x => x.Barcode)
            .MaximumLength(100).WithMessage("الباركود يجب ألا يتجاوز 100 حرف.");

        RuleFor(x => x.Weight)
            .GreaterThan(0).WithMessage("وزن المنتج يجب أن يكون أكبر من الصفر.");

        RuleFor(x => x.SellingPrice)
            .GreaterThanOrEqualTo(0).WithMessage("سعر البيع يجب أن يكون صفراً أو قيمة موجبة.");

        RuleFor(x => x.WholesalePrice)
            .GreaterThanOrEqualTo(0).When(x => x.WholesalePrice.HasValue)
            .WithMessage("سعر الجملة يجب أن يكون صفراً أو قيمة موجبة.");

        RuleFor(x => x.DistributorPrice)
            .GreaterThanOrEqualTo(0).When(x => x.DistributorPrice.HasValue)
            .WithMessage("سعر الموزع يجب أن يكون صفراً أو قيمة موجبة.");

        RuleFor(x => x.StandardCost)
            .GreaterThanOrEqualTo(0).WithMessage("التكلفة المعيارية التقديرية يجب أن تكون صفراً أو قيمة موجبة.");

        RuleFor(x => x.MinimumStock)
            .GreaterThanOrEqualTo(0).WithMessage("الحد الأدنى لمخزون الأمان يجب أن يكون صفراً أو قيمة موجبة.");

        RuleFor(x => x.ExpiryPeriod)
            .GreaterThanOrEqualTo(0).WithMessage("مدة الصلاحية يجب أن تكون صفراً أو قيمة موجبة.");
    }
}

public class UpdateProductRequestValidator : AbstractValidator<UpdateProductRequest>
{
    public UpdateProductRequestValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("معرف المنتج غير صالح.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("كود المنتج مطلوب ولا يمكن تركه فارغاً.")
            .MaximumLength(50).WithMessage("كود المنتج يجب ألا يتجاوز 50 حرفاً.");

        RuleFor(x => x.SKU)
            .NotEmpty().WithMessage("رمز التخزين (SKU) مطلوب ولا يمكن تركه فارغاً.")
            .MaximumLength(50).WithMessage("رمز الصنف (SKU) يجب ألا يتجاوز 50 حرفاً.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("اسم المنتج مطلوب ولا يمكن تركه فارغاً.")
            .MaximumLength(150).WithMessage("اسم المنتج يجب ألا يتجاوز 150 حرفاً.");

        RuleFor(x => x.ArabicName)
            .MaximumLength(150).WithMessage("الاسم العربي للمنتج يجب ألا يتجاوز 150 حرفاً.");

        RuleFor(x => x.ProductCategoryId)
            .NotNull().WithMessage("تصنيف المنتج مطلوب.")
            .GreaterThan(0).WithMessage("يجب اختيار تصنيف صالح للمنتج.");

        RuleFor(x => x.Barcode)
            .MaximumLength(100).WithMessage("الباركود يجب ألا يتجاوز 100 حرف.");

        RuleFor(x => x.Weight)
            .GreaterThan(0).WithMessage("وزن المنتج يجب أن يكون أكبر من الصفر.");

        RuleFor(x => x.SellingPrice)
            .GreaterThanOrEqualTo(0).WithMessage("سعر البيع يجب أن يكون صفراً أو قيمة موجبة.");

        RuleFor(x => x.WholesalePrice)
            .GreaterThanOrEqualTo(0).When(x => x.WholesalePrice.HasValue)
            .WithMessage("سعر الجملة يجب أن يكون صفراً أو قيمة موجبة.");

        RuleFor(x => x.DistributorPrice)
            .GreaterThanOrEqualTo(0).When(x => x.DistributorPrice.HasValue)
            .WithMessage("سعر الموزع يجب أن يكون صفراً أو قيمة موجبة.");

        RuleFor(x => x.StandardCost)
            .GreaterThanOrEqualTo(0).WithMessage("التكلفة المعيارية التقديرية يجب أن تكون صفراً أو قيمة موجبة.");

        RuleFor(x => x.MinimumStock)
            .GreaterThanOrEqualTo(0).WithMessage("الحد الأدنى لمخزون الأمان يجب أن يكون صفراً أو قيمة موجبة.");

        RuleFor(x => x.ExpiryPeriod)
            .GreaterThanOrEqualTo(0).WithMessage("مدة الصلاحية يجب أن تكون صفراً أو قيمة موجبة.");
    }
}
