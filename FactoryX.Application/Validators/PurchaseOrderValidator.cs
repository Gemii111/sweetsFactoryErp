using FactoryX.Application.DTOs;
using FluentValidation;

namespace FactoryX.Application.Validators;

public class CreatePurchaseOrderItemValidator : AbstractValidator<CreatePurchaseOrderItemRequest>
{
    public CreatePurchaseOrderItemValidator()
    {
        RuleFor(i => i.MaterialId)
            .GreaterThan(0).WithMessage("يجب تحديد المادة الخام.");

        RuleFor(i => i.OrderedQuantity)
            .GreaterThan(0).WithMessage("الكمية المطلوبة بأمر الشراء يجب أن تكون أكبر من الصفر.");

        RuleFor(i => i.Unit)
            .NotEmpty().WithMessage("وحدة القياس مطلوبة.");

        RuleFor(i => i.UnitPrice)
            .GreaterThanOrEqualTo(0).WithMessage("سعر الوحدة لا يمكن أن يكون سالباً.");

        RuleFor(i => i.DiscountAmount)
            .GreaterThanOrEqualTo(0).WithMessage("قيمة الخصم لا يمكن أن تكون سالبة.");

        RuleFor(i => i.TaxAmount)
            .GreaterThanOrEqualTo(0).WithMessage("قيمة الضريبة لا يمكن أن تكون سالبة.");
    }
}

public class CreatePurchaseOrderValidator : AbstractValidator<CreatePurchaseOrderRequest>
{
    public CreatePurchaseOrderValidator()
    {
        RuleFor(o => o.SupplierId)
            .GreaterThan(0).WithMessage("يجب اختيار المورد.");

        RuleFor(o => o.WarehouseId)
            .GreaterThan(0).WithMessage("يجب اختيار مستودع الاستلام الافتراضي.");

        RuleFor(o => o.OrderDate)
            .NotEmpty().WithMessage("تاريخ أمر الشراء مطلوب.");

        RuleFor(o => o.Items)
            .NotEmpty().WithMessage("يجب إضافة بند واحد على الأقل في أمر الشراء.");

        RuleForEach(o => o.Items)
            .SetValidator(new CreatePurchaseOrderItemValidator());
    }
}
