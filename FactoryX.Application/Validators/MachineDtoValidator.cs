using FactoryX.Application.DTOs;
using FluentValidation;

namespace FactoryX.Application.Validators;

public class MachineDtoValidator : AbstractValidator<MachineDto>
{
    public MachineDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Status).NotEmpty();
        RuleFor(x => x.Capacity).GreaterThan(0);
    }
}