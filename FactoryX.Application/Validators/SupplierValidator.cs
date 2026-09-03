using FactoryX.Application.DTOs;
using FluentValidation;

namespace FactoryX.Application.Validators;

public class CreateSupplierRequestValidator : AbstractValidator<CreateSupplierRequest>
{
    public CreateSupplierRequestValidator()
    {
        RuleFor(s => s.Code)
            .NotEmpty().WithMessage("كود المورد مطلوب.")
            .MaximumLength(50).WithMessage("كود المورد لا يمكن أن يتجاوز 50 حرفاً.");

        RuleFor(s => s.Name)
            .NotEmpty().WithMessage("اسم المورد مطلوب.")
            .MaximumLength(200).WithMessage("اسم المورد لا يمكن أن يتجاوز 200 حرف.");

        RuleFor(s => s.Phone)
            .MaximumLength(50).WithMessage("رقم الهاتف لا يمكن أن يتجاوز 50 حرفاً.");

        RuleFor(s => s.Email)
            .EmailAddress().When(s => !string.IsNullOrEmpty(s.Email)).WithMessage("البريد الإلكتروني غير صالح.")
            .MaximumLength(150).WithMessage("البريد الإلكتروني لا يمكن أن يتجاوز 150 حرفاً.");
    }
}

public class UpdateSupplierRequestValidator : AbstractValidator<UpdateSupplierRequest>
{
    public UpdateSupplierRequestValidator()
    {
        RuleFor(s => s.Id)
            .GreaterThan(0).WithMessage("معرف المورد غير صالح.");

        RuleFor(s => s.Code)
            .NotEmpty().WithMessage("كود المورد مطلوب.")
            .MaximumLength(50).WithMessage("كود المورد لا يمكن أن يتجاوز 50 حرفاً.");

        RuleFor(s => s.Name)
            .NotEmpty().WithMessage("اسم المورد مطلوب.")
            .MaximumLength(200).WithMessage("اسم المورد لا يمكن أن يتجاوز 200 حرف.");

        RuleFor(s => s.Email)
            .EmailAddress().When(s => !string.IsNullOrEmpty(s.Email)).WithMessage("البريد الإلكتروني غير صالح.");
    }
}

public class CreateSupplierCategoryRequestValidator : AbstractValidator<CreateSupplierCategoryRequest>
{
    public CreateSupplierCategoryRequestValidator()
    {
        RuleFor(c => c.Code)
            .NotEmpty().WithMessage("كود تصنيف الموردين مطلوب.")
            .MaximumLength(50).WithMessage("كود التصنيف لا يمكن أن يتجاوز 50 حرفاً.");

        RuleFor(c => c.Name)
            .NotEmpty().WithMessage("اسم تصنيف الموردين مطلوب.")
            .MaximumLength(150).WithMessage("اسم التصنيف لا يمكن أن يتجاوز 150 حرفاً.");
    }
}
