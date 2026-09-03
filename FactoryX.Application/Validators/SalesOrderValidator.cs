using FactoryX.Application.DTOs;
using FluentValidation;

namespace FactoryX.Application.Validators;

public class CreateSalesOrderItemValidator : AbstractValidator<CreateSalesOrderItemRequest>
{
    public CreateSalesOrderItemValidator()
    {
        RuleFor(i => i.ProductId)
            .GreaterThan(0).WithMessage("يجب اختيار منتج تام صالح.");

        RuleFor(i => i.OrderedQuantity)
            .GreaterThan(0).WithMessage("الكمية المطلوبة يجب أن تكون أكبر من الصفر.");

        RuleFor(i => i.UnitPrice)
            .GreaterThanOrEqualTo(0).WithMessage("سعر الوحدة يجب أن يكون صفراً أو أكثر.");

        RuleFor(i => i.DiscountAmount)
            .GreaterThanOrEqualTo(0).WithMessage("مبلغ الخصم لا يمكن أن يكون سالباً.");

        RuleFor(i => i.TaxAmount)
            .GreaterThanOrEqualTo(0).WithMessage("مبلغ الضريبة لا يمكن أن يكون سالباً.");
    }
}

public class CreateSalesOrderValidator : AbstractValidator<CreateSalesOrderRequest>
{
    public CreateSalesOrderValidator()
    {
        RuleFor(so => so.CustomerId)
            .GreaterThan(0).WithMessage("يجب اختيار العميل.");

        RuleFor(so => so.WarehouseId)
            .GreaterThan(0).WithMessage("يجب اختيار مستودع المنتجات التامة.");

        RuleFor(so => so.OrderDate)
            .NotEmpty().WithMessage("تاريخ أمر البيع مطلوب.");

        RuleFor(so => so.Items)
            .NotEmpty().WithMessage("يجب إضافة منتج واحد على الأقل في أمر البيع.");

        RuleForEach(so => so.Items)
            .SetValidator(new CreateSalesOrderItemValidator());
    }
}

public class UpdateSalesOrderValidator : AbstractValidator<UpdateSalesOrderRequest>
{
    public UpdateSalesOrderValidator()
    {
        RuleFor(so => so.Id)
            .GreaterThan(0).WithMessage("معرف أمر البيع غير صالح.");

        RuleFor(so => so.CustomerId)
            .GreaterThan(0).WithMessage("يجب اختيار العميل.");

        RuleFor(so => so.WarehouseId)
            .GreaterThan(0).WithMessage("يجب اختيار مستودع المنتجات التامة.");

        RuleFor(so => so.Items)
            .NotEmpty().WithMessage("يجب إضافة منتج واحد على الأقل في أمر البيع.");

        RuleForEach(so => so.Items)
            .SetValidator(new CreateSalesOrderItemValidator());
    }
}
