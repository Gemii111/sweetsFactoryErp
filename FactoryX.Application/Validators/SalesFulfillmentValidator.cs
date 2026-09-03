using FactoryX.Application.DTOs;
using FluentValidation;

namespace FactoryX.Application.Validators;

public class CreateSalesFulfillmentItemValidator : AbstractValidator<CreateSalesFulfillmentItemRequest>
{
    public CreateSalesFulfillmentItemValidator()
    {
        RuleFor(i => i.ProductId)
            .GreaterThan(0).WithMessage("يجب اختيار المنتج التام.");

        RuleFor(i => i.ShippedQuantity)
            .GreaterThan(0).WithMessage("الكمية المصروفة يجب أن تكون أكبر من الصفر.");

        RuleFor(i => i.BatchNumber)
            .NotEmpty().WithMessage("رقم تشغيلة المنتج التام مطلوب للصرف والتسليم.");

        RuleFor(i => i.WarehouseId)
            .GreaterThan(0).WithMessage("يجب تحديد مستودع الصرف.");
    }
}

public class CreateSalesFulfillmentValidator : AbstractValidator<CreateSalesFulfillmentRequest>
{
    public CreateSalesFulfillmentValidator()
    {
        RuleFor(sf => sf.SalesOrderId)
            .GreaterThan(0).WithMessage("يجب ربط الصرف بأمر بيع معتمد.");

        RuleFor(sf => sf.CustomerId)
            .GreaterThan(0).WithMessage("يجب تحديد العميل.");

        RuleFor(sf => sf.WarehouseId)
            .GreaterThan(0).WithMessage("يجب تحديد مستودع المنتجات التامة.");

        RuleFor(sf => sf.FulfillmentDate)
            .NotEmpty().WithMessage("تاريخ الصرف والتسليم مطلوب.");

        RuleFor(sf => sf.Items)
            .NotEmpty().WithMessage("يجب تحديد بنود الصرف والتسليم.");

        RuleForEach(sf => sf.Items)
            .SetValidator(new CreateSalesFulfillmentItemValidator());
    }
}
