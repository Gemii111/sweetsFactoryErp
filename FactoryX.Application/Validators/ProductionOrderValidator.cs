using FactoryX.Application.DTOs;
using FluentValidation;

namespace FactoryX.Application.Validators;

public class CreateProductionOrderRequestValidator : AbstractValidator<CreateProductionOrderRequest>
{
    public CreateProductionOrderRequestValidator()
    {
        RuleFor(x => x.ProductId)
            .GreaterThan(0).WithMessage("يجب اختيار منتج تام صالح لأمر الإنتاج.");

        RuleFor(x => x.RecipeVersionId)
            .GreaterThan(0).WithMessage("يجب اختيار إصدار وصفة نشط وسارٍ للمنتج.");

        RuleFor(x => x.PlannedQuantity)
            .GreaterThan(0).WithMessage("الكمية المخطط إنتاجها يجب أن تكون أكبر من الصفر.");

        RuleFor(x => x.OutputUnit)
            .NotEmpty().WithMessage("وحدة الإنتاج مطلوبة.")
            .MaximumLength(30).WithMessage("وحدة الإنتاج يجب ألا تتجاوز 30 حرفاً.");

        RuleFor(x => x.PlannedDate)
            .NotEmpty().WithMessage("تاريخ الإنتاج المخطط مطلوب.");

        RuleFor(x => x.DueDate)
            .GreaterThanOrEqualTo(x => x.PlannedDate).When(x => x.DueDate.HasValue)
            .WithMessage("تاريخ الاستحقاق (Due Date) يجب أن يكون مساوياً أو لاحقاً لتاريخ الإنتاج المخطط.");

        RuleFor(x => x.OrderNumber)
            .MaximumLength(50).WithMessage("رقم أمر الإنتاج يجب ألا يتجاوز 50 حرفاً.");

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("الملاحظات يجب ألا تتجاوز 500 حرف.");
    }
}

public class UpdateProductionOrderRequestValidator : AbstractValidator<UpdateProductionOrderRequest>
{
    public UpdateProductionOrderRequestValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("معرف أمر الإنتاج غير صالح.");

        RuleFor(x => x.OrderNumber)
            .NotEmpty().WithMessage("رقم أمر الإنتاج مطلوب.")
            .MaximumLength(50).WithMessage("رقم أمر الإنتاج يجب ألا يتجاوز 50 حرفاً.");

        RuleFor(x => x.ProductId)
            .GreaterThan(0).WithMessage("يجب اختيار منتج تام صالح لأمر الإنتاج.");

        RuleFor(x => x.RecipeVersionId)
            .GreaterThan(0).WithMessage("يجب اختيار إصدار وصفة نشط وسارٍ للمنتج.");

        RuleFor(x => x.PlannedQuantity)
            .GreaterThan(0).WithMessage("الكمية المخطط إنتاجها يجب أن تكون أكبر من الصفر.");

        RuleFor(x => x.OutputUnit)
            .NotEmpty().WithMessage("وحدة الإنتاج مطلوبة.")
            .MaximumLength(30).WithMessage("وحدة الإنتاج يجب ألا تتجاوز 30 حرفاً.");

        RuleFor(x => x.PlannedDate)
            .NotEmpty().WithMessage("تاريخ الإنتاج المخطط مطلوب.");

        RuleFor(x => x.DueDate)
            .GreaterThanOrEqualTo(x => x.PlannedDate).When(x => x.DueDate.HasValue)
            .WithMessage("تاريخ الاستحقاق (Due Date) يجب أن يكون مساوياً أو لاحقاً لتاريخ الإنتاج المخطط.");

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("الملاحظات يجب ألا تتجاوز 500 حرف.");
    }
}
