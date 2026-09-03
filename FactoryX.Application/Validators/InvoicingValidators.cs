using FactoryX.Application.DTOs;
using FluentValidation;

namespace FactoryX.Application.Validators;

public class CreateInvoiceRequestValidator : AbstractValidator<CreateInvoiceRequest>
{
    public CreateInvoiceRequestValidator()
    {
        RuleFor(x => x.CustomerId)
            .GreaterThan(0)
            .WithMessage("يجب اختيار العميل.");

        RuleFor(x => x.SalesOrderId)
            .GreaterThan(0)
            .WithMessage("يجب اختيار أمر البيع المرتبط.");

        RuleFor(x => x.Currency)
            .NotEmpty()
            .MaximumLength(10)
            .WithMessage("يجب تحديد العملة.");

        RuleFor(x => x.Items)
            .NotEmpty()
            .WithMessage("يجب إضافة بند واحد على الأقل في الفاتورة.");

        RuleForEach(x => x.Items)
            .SetValidator(new CreateInvoiceItemRequestValidator());
    }
}

public class CreateInvoiceItemRequestValidator : AbstractValidator<CreateInvoiceItemRequest>
{
    public CreateInvoiceItemRequestValidator()
    {
        RuleFor(x => x.ProductId)
            .GreaterThan(0)
            .WithMessage("يجب اختيار المنتج.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("يجب أن تكون الكمية المفوترة أكبر من الصفر.");

        RuleFor(x => x.UnitPrice)
            .GreaterThanOrEqualTo(0)
            .WithMessage("يجب ألا يكون سعر الوحدة سالباً.");

        RuleFor(x => x.DiscountAmount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("يجب ألا يكون الخصم سالباً.");

        RuleFor(x => x.TaxRate)
            .GreaterThanOrEqualTo(0)
            .WithMessage("يجب ألا تكون نسبة الضريبة سالبة.");
    }
}

public class CreatePaymentRequestValidator : AbstractValidator<CreatePaymentRequest>
{
    public CreatePaymentRequestValidator()
    {
        RuleFor(x => x.InvoiceId)
            .GreaterThan(0)
            .WithMessage("يجب اختيار الفاتورة المراد سدادها.");

        RuleFor(x => x.CustomerId)
            .GreaterThan(0)
            .WithMessage("يجب تحديد العميل.");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("يجب أن يكون مبلغ السداد أكبر من الصفر.");

        RuleFor(x => x.Currency)
            .NotEmpty()
            .MaximumLength(10)
            .WithMessage("يجب تحديد العملة.");

        RuleFor(x => x.PaymentMethod)
            .IsInEnum()
            .WithMessage("طريقة السداد غير صالحة.");
    }
}

public class VoidPaymentRequestValidator : AbstractValidator<VoidPaymentRequest>
{
    public VoidPaymentRequestValidator()
    {
        RuleFor(x => x.PaymentId)
            .GreaterThan(0)
            .WithMessage("يجب تحديد السند المراد إلغاؤه.");

        RuleFor(x => x.Reason)
            .NotEmpty()
            .MaximumLength(500)
            .WithMessage("يجب ذكر سبب إلغاء أو استرداد سند القبض.");
    }
}
