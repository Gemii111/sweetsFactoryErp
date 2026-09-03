using FactoryX.Application.DTOs;
using FluentValidation;

namespace FactoryX.Application.Validators;

public class AccountCreateDtoValidator : AbstractValidator<AccountCreateDto>
{
    public AccountCreateDtoValidator()
    {
        RuleFor(x => x.AccountCode)
            .NotEmpty().WithMessage("رمز الحساب (Account Code) مطلوب.")
            .MaximumLength(50).WithMessage("رمز الحساب لا يجب أن يتجاوز 50 حرفاً.");

        RuleFor(x => x.AccountName)
            .NotEmpty().WithMessage("اسم الحساب بالإنجليزية مطلوب.")
            .MaximumLength(200);

        RuleFor(x => x.AccountNameAr)
            .NotEmpty().WithMessage("اسم الحساب بالعربية مطلوب.")
            .MaximumLength(200);

        RuleFor(x => x.AccountType)
            .IsInEnum().WithMessage("نوع الحساب غير صالح.");
    }
}

public class AccountUpdateDtoValidator : AbstractValidator<AccountUpdateDto>
{
    public AccountUpdateDtoValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0);

        RuleFor(x => x.AccountCode)
            .NotEmpty().WithMessage("رمز الحساب مطلوب.")
            .MaximumLength(50);

        RuleFor(x => x.AccountName)
            .NotEmpty().WithMessage("اسم الحساب بالإنجليزية مطلوب.");

        RuleFor(x => x.AccountNameAr)
            .NotEmpty().WithMessage("اسم الحساب بالعربية مطلوب.");
    }
}

public class AccountingPeriodCreateDtoValidator : AbstractValidator<AccountingPeriodCreateDto>
{
    public AccountingPeriodCreateDtoValidator()
    {
        RuleFor(x => x.PeriodName)
            .NotEmpty().WithMessage("اسم الفترة المالية مطلوب.")
            .MaximumLength(100);

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("تاريخ بداية الفترة مطلوب.");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("تاريخ نهاية الفترة مطلوب.")
            .GreaterThanOrEqualTo(x => x.StartDate).WithMessage("تاريخ نهاية الفترة يجب أن يكون لاحقاً أو مساوياً لتاريخ البداية.");
    }
}

public class JournalEntryCreateDtoValidator : AbstractValidator<JournalEntryCreateDto>
{
    public JournalEntryCreateDtoValidator()
    {
        RuleFor(x => x.EntryDate)
            .NotEmpty().WithMessage("تاريخ القيد مطلوب.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("شرح / بيان القيد مطلوب.")
            .MaximumLength(500);

        RuleFor(x => x.Lines)
            .NotNull().WithMessage("بنود القيد مطلوبة.")
            .Must(lines => lines != null && lines.Count >= 2)
            .WithMessage("يجب أن يحتوي القيد المحاسبي على طرفين (بندين) على الأقل (طرف مدين وطرف دائن).");

        RuleFor(x => x)
            .Must(entry =>
            {
                if (entry.Lines == null || entry.Lines.Count < 2) return false;
                var totalDebit = entry.Lines.Sum(l => l.Debit);
                var totalCredit = entry.Lines.Sum(l => l.Credit);
                return totalDebit > 0 && Math.Abs(totalDebit - totalCredit) < 0.01m;
            })
            .WithMessage("القيد غير متوازن! يجب أن يتساوى إجمالي المدين مع إجمالي الدائن تماماً، ومجموع الطرفين أكبر من الصفر.");

        RuleForEach(x => x.Lines)
            .SetValidator(new JournalEntryLineCreateDtoValidator());
    }
}

public class JournalEntryLineCreateDtoValidator : AbstractValidator<JournalEntryLineCreateDto>
{
    public JournalEntryLineCreateDtoValidator()
    {
        RuleFor(x => x.AccountId)
            .GreaterThan(0).WithMessage("يجب اختيار الحساب المالي للبند.");

        RuleFor(x => x.Debit)
            .GreaterThanOrEqualTo(0).WithMessage("قيمة المدين لا يمكن أن تكون سالبة.");

        RuleFor(x => x.Credit)
            .GreaterThanOrEqualTo(0).WithMessage("قيمة الدائن لا يمكن أن تكون سالبة.");

        RuleFor(x => x)
            .Must(line => (line.Debit > 0 && line.Credit == 0) || (line.Credit > 0 && line.Debit == 0))
            .WithMessage("لا يمكن للبند الواحد أن يحتوي على قيمتين مدين ودائن معاً في نفس الوقت، ويجب تحديد قيمة في أحدهما.");
    }
}

public class SupplierPaymentCreateDtoValidator : AbstractValidator<SupplierPaymentCreateDto>
{
    public SupplierPaymentCreateDtoValidator()
    {
        RuleFor(x => x.SupplierId)
            .GreaterThan(0).WithMessage("يجب تحديد المورد.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("يجب أن يكون مبلغ السداد أكبر من الصفر.");

        RuleFor(x => x.PaymentDate)
            .NotEmpty().WithMessage("تاريخ السداد مطلوب.");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("العملة مطلوبة.");
    }
}
