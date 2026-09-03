using FactoryX.Application.DTOs;
using FluentValidation;

namespace FactoryX.Application.Validators;

public class CreatePurchaseRequestItemValidator : AbstractValidator<CreatePurchaseRequestItemRequest>
{
    public CreatePurchaseRequestItemValidator()
    {
        RuleFor(i => i.MaterialId)
            .GreaterThan(0).WithMessage("يجب تحديد المادة الخام المطلوبة.");

        RuleFor(i => i.RequestedQuantity)
            .GreaterThan(0).WithMessage("الكمية المطلوبة يجب أن تكون أكبر من الصفر.");

        RuleFor(i => i.Unit)
            .NotEmpty().WithMessage("وحدة القياس مطلوبة.");

        RuleFor(i => i.EstimatedUnitPrice)
            .GreaterThanOrEqualTo(0).WithMessage("السعر التقديري لا يمكن أن يكون سالباً.");
    }
}

public class CreatePurchaseRequestValidator : AbstractValidator<CreatePurchaseRequest>
{
    public CreatePurchaseRequestValidator()
    {
        RuleFor(r => r.RequestDate)
            .NotEmpty().WithMessage("تاريخ طلب الشراء مطلوب.");

        RuleFor(r => r.Priority)
            .NotEmpty().WithMessage("أولوية طلب الشراء مطلوبة.");

        RuleFor(r => r.Items)
            .NotEmpty().WithMessage("يجب إضافة بند واحد على الأقل لطلب الشراء.");

        RuleForEach(r => r.Items)
            .SetValidator(new CreatePurchaseRequestItemValidator());
    }
}
