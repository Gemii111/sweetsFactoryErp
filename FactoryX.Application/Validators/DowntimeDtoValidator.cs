using FactoryX.Application.DTOs;
using FluentValidation;

namespace FactoryX.Application.Validators;

public class DowntimeDtoValidator : AbstractValidator<DowntimeDto>
{
    public DowntimeDtoValidator()
    {
        RuleFor(x => x.MachineId).GreaterThan(0);
        RuleFor(x => x.StartTime).LessThan(x => x.EndTime).WithMessage("StartTime must be before EndTime");
        RuleFor(x => x.Reason).NotEmpty();
    }
}