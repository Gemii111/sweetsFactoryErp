using FactoryX.Application.DTOs;
using FluentValidation;

namespace FactoryX.Application.Validators;

public class CreateCustomerValidator : AbstractValidator<CreateCustomerRequest>
{
    public CreateCustomerValidator()
    {
        RuleFor(c => c.Code)
            .NotEmpty().WithMessage("كود العميل مطلوب.")
            .MaximumLength(50).WithMessage("كود العميل لا يمكن أن يتجاوز 50 حرفاً.");

        RuleFor(c => c.Name)
            .NotEmpty().WithMessage("اسم العميل مطلوب.")
            .MaximumLength(200).WithMessage("اسم العميل لا يمكن أن يتجاوز 200 حرف.");

        RuleFor(c => c.ArabicName)
            .MaximumLength(200).WithMessage("الاسم العربي لا يمكن أن يتجاوز 200 حرف.");

        RuleFor(c => c.Phone)
            .MaximumLength(50).WithMessage("رقم الهاتف لا يمكن أن يتجاوز 50 حرفاً.");

        RuleFor(c => c.Mobile)
            .MaximumLength(50).WithMessage("رقم الجوال لا يمكن أن يتجاوز 50 حرفاً.");

        RuleFor(c => c.Email)
            .EmailAddress().When(c => !string.IsNullOrWhiteSpace(c.Email))
            .WithMessage("صيغة البريد الإلكتروني غير صحيحة.")
            .MaximumLength(150).WithMessage("البريد الإلكتروني لا يمكن أن يتجاوز 150 حرفاً.");

        RuleFor(c => c.CreditLimit)
            .GreaterThanOrEqualTo(0).WithMessage("الحد الائتماني يجب أن يكون صفراً أو أكثر.");
    }
}

public class UpdateCustomerValidator : AbstractValidator<UpdateCustomerRequest>
{
    public UpdateCustomerValidator()
    {
        RuleFor(c => c.Id)
            .GreaterThan(0).WithMessage("معرف العميل غير صالح.");

        RuleFor(c => c.Code)
            .NotEmpty().WithMessage("كود العميل مطلوب.")
            .MaximumLength(50).WithMessage("كود العميل لا يمكن أن يتجاوز 50 حرفاً.");

        RuleFor(c => c.Name)
            .NotEmpty().WithMessage("اسم العميل مطلوب.")
            .MaximumLength(200).WithMessage("اسم العميل لا يمكن أن يتجاوز 200 حرف.");

        RuleFor(c => c.CreditLimit)
            .GreaterThanOrEqualTo(0).WithMessage("الحد الائتماني يجب أن يكون صفراً أو أكثر.");
    }
}
