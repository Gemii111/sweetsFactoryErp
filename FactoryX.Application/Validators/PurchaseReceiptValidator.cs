using FactoryX.Application.DTOs;
using FluentValidation;

namespace FactoryX.Application.Validators;

public class CreatePurchaseReceiptItemValidator : AbstractValidator<CreatePurchaseReceiptItemRequest>
{
    public CreatePurchaseReceiptItemValidator()
    {
        RuleFor(i => i.MaterialId)
            .GreaterThan(0).WithMessage("يجب تحديد المادة الخام.");

        RuleFor(i => i.ReceivedQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("الكمية المستلمة لا يمكن أن تكون سالبة.");

        RuleFor(i => i.AcceptedQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("الكمية المقبولة لا يمكن أن تكون سالبة.");

        RuleFor(i => i.RejectedQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("الكمية المرفوضة لا يمكن أن تكون سالبة.");

        RuleFor(i => i)
            .Must(i => (i.AcceptedQuantity + i.RejectedQuantity) <= i.ReceivedQuantity)
            .WithMessage("مجموع الكمية المقبولة والمرفوضة لا يمكن أن يتجاوز إجمالي الكمية المستلمة.");

        RuleFor(i => i.Unit)
            .NotEmpty().WithMessage("وحدة القياس مطلوبة.");

        RuleFor(i => i.UnitPrice)
            .GreaterThanOrEqualTo(0).WithMessage("سعر الوحدة لا يمكن أن يكون سالباً.");

        RuleFor(i => i.WarehouseId)
            .GreaterThan(0).WithMessage("يجب تحديد مستودع التخزين للبند.");
    }
}

public class CreatePurchaseReceiptValidator : AbstractValidator<CreatePurchaseReceiptRequest>
{
    public CreatePurchaseReceiptValidator()
    {
        RuleFor(r => r.PurchaseOrderId)
            .GreaterThan(0).WithMessage("يجب تحديد أمر الشراء المرجعي.");

        RuleFor(r => r.SupplierId)
            .GreaterThan(0).WithMessage("يجب تحديد المورد.");

        RuleFor(r => r.WarehouseId)
            .GreaterThan(0).WithMessage("يجب تحديد المستودع الرئيسي للاستلام.");

        RuleFor(r => r.ReceiptDate)
            .NotEmpty().WithMessage("تاريخ الاستلام وسند الإدخال مطلوب.");

        RuleFor(r => r.Items)
            .NotEmpty().WithMessage("يجب تضمين بند واحد على الأقل في محضر وسند الاستلام.");

        RuleFor(r => r)
            .Must(r => r.Items.Any(i => i.AcceptedQuantity > 0 || i.ReceivedQuantity > 0))
            .WithMessage("يجب استلام كمية واحدة على الأقل في سند الإدخال المخزني.");

        RuleForEach(r => r.Items)
            .SetValidator(new CreatePurchaseReceiptItemValidator());
    }
}
