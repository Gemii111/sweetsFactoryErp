using FactoryX.Application.DTOs;
using FluentValidation;

namespace FactoryX.Application.Validators;

public class CreateRecipeRequestValidator : AbstractValidator<CreateRecipeRequest>
{
    public CreateRecipeRequestValidator()
    {
        RuleFor(x => x.ProductId)
            .GreaterThan(0).WithMessage("يجب اختيار منتج تام صالح للوصفة.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("كود الوصفة مطلوب ولا يمكن تركه فارغاً.")
            .MaximumLength(50).WithMessage("كود الوصفة يجب ألا يتجاوز 50 حرفاً.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("اسم الوصفة مطلوب ولا يمكن تركه فارغاً.")
            .MaximumLength(150).WithMessage("اسم الوصفة يجب ألا يتجاوز 150 حرفاً.");

        RuleFor(x => x.ArabicName)
            .MaximumLength(150).WithMessage("الاسم العربي للوصفة يجب ألا يتجاوز 150 حرفاً.");

        RuleFor(x => x.BaseOutputQuantity)
            .GreaterThan(0).WithMessage("كمية التشغيلة المعيارية للوصفة يجب أن تكون أكبر من الصفر.");
    }
}

public class UpdateRecipeRequestValidator : AbstractValidator<UpdateRecipeRequest>
{
    public UpdateRecipeRequestValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("معرف الوصفة غير صالح.");

        RuleFor(x => x.ProductId)
            .GreaterThan(0).WithMessage("يجب اختيار منتج تام صالح للوصفة.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("كود الوصفة مطلوب ولا يمكن تركه فارغاً.")
            .MaximumLength(50).WithMessage("كود الوصفة يجب ألا يتجاوز 50 حرفاً.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("اسم الوصفة مطلوب ولا يمكن تركه فارغاً.")
            .MaximumLength(150).WithMessage("اسم الوصفة يجب ألا يتجاوز 150 حرفاً.");

        RuleFor(x => x.ArabicName)
            .MaximumLength(150).WithMessage("الاسم العربي للوصفة يجب ألا يتجاوز 150 حرفاً.");

        RuleFor(x => x.BaseOutputQuantity)
            .GreaterThan(0).WithMessage("كمية التشغيلة المعيارية للوصفة يجب أن تكون أكبر من الصفر.");
    }
}

public class RecipeItemRequestValidator : AbstractValidator<RecipeItemRequest>
{
    public RecipeItemRequestValidator()
    {
        RuleFor(x => x.MaterialId)
            .GreaterThan(0).WithMessage("يجب اختيار مادة خام صالحة ونشطة.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("كمية المادة الخام في الوصفة يجب أن تكون أكبر من الصفر.");
    }
}

public class CreateRecipeVersionRequestValidator : AbstractValidator<CreateRecipeVersionRequest>
{
    public CreateRecipeVersionRequestValidator()
    {
        RuleFor(x => x.RecipeId)
            .GreaterThan(0).WithMessage("معرف الوصفة غير صالح.");

        RuleFor(x => x.VersionNumber)
            .NotEmpty().WithMessage("رقم الإصدار مطلوب (مثال: V1.0, V2.0).")
            .MaximumLength(50).WithMessage("رقم الإصدار يجب ألا يتجاوز 50 حرفاً.");

        RuleFor(x => x.EffectiveFrom)
            .NotEmpty().WithMessage("تاريخ بدء سريان الإصدار مطلوب.");

        RuleFor(x => x.EffectiveTo)
            .GreaterThanOrEqualTo(x => x.EffectiveFrom).When(x => x.EffectiveTo.HasValue)
            .WithMessage("تاريخ انتهاء السريان يجب أن يكون لاحقاً أو مساوياً لتاريخ البدء.");

        RuleFor(x => x.ExpectedOutput)
            .GreaterThan(0).WithMessage("الكمية المستهدفة من تشغيلة الوصفة يجب أن تكون أكبر من الصفر.");

        RuleFor(x => x.ExpectedWastePercentage)
            .InclusiveBetween(0, 100).WithMessage("نسبة الهالك المتوقع يجب أن تكون بين 0% و 100%.");

        RuleFor(x => x.LaborCost)
            .GreaterThanOrEqualTo(0).WithMessage("تكلفة العمالة التقديرية يجب أن تكون صفراً أو قيمة موجبة.");

        RuleFor(x => x.MachineCost)
            .GreaterThanOrEqualTo(0).WithMessage("تكلفة تشغيل الماكينات يجب أن تكون صفراً أو قيمة موجبة.");

        RuleFor(x => x.OverheadCost)
            .GreaterThanOrEqualTo(0).WithMessage("المصاريف غير المباشرة يجب أن تكون صفراً أو قيمة موجبة.");

        RuleForEach(x => x.Items).SetValidator(new RecipeItemRequestValidator());
    }
}

public class UpdateRecipeVersionRequestValidator : AbstractValidator<UpdateRecipeVersionRequest>
{
    public UpdateRecipeVersionRequestValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("معرف إصدار الوصفة غير صالح.");

        RuleFor(x => x.RecipeId)
            .GreaterThan(0).WithMessage("معرف الوصفة غير صالح.");

        RuleFor(x => x.VersionNumber)
            .NotEmpty().WithMessage("رقم الإصدار مطلوب (مثال: V1.0, V2.0).")
            .MaximumLength(50).WithMessage("رقم الإصدار يجب ألا يتجاوز 50 حرفاً.");

        RuleFor(x => x.EffectiveFrom)
            .NotEmpty().WithMessage("تاريخ بدء سريان الإصدار مطلوب.");

        RuleFor(x => x.EffectiveTo)
            .GreaterThanOrEqualTo(x => x.EffectiveFrom).When(x => x.EffectiveTo.HasValue)
            .WithMessage("تاريخ انتهاء السريان يجب أن يكون لاحقاً أو مساوياً لتاريخ البدء.");

        RuleFor(x => x.ExpectedOutput)
            .GreaterThan(0).WithMessage("الكمية المستهدفة من تشغيلة الوصفة يجب أن تكون أكبر من الصفر.");

        RuleFor(x => x.ExpectedWastePercentage)
            .InclusiveBetween(0, 100).WithMessage("نسبة الهالك المتوقع يجب أن تكون بين 0% و 100%.");

        RuleFor(x => x.LaborCost)
            .GreaterThanOrEqualTo(0).WithMessage("تكلفة العمالة التقديرية يجب أن تكون صفراً أو قيمة موجبة.");

        RuleFor(x => x.MachineCost)
            .GreaterThanOrEqualTo(0).WithMessage("تكلفة تشغيل الماكينات يجب أن تكون صفراً أو قيمة موجبة.");

        RuleFor(x => x.OverheadCost)
            .GreaterThanOrEqualTo(0).WithMessage("المصاريف غير المباشرة يجب أن تكون صفراً أو قيمة موجبة.");

        RuleForEach(x => x.Items).SetValidator(new RecipeItemRequestValidator());
    }
}
