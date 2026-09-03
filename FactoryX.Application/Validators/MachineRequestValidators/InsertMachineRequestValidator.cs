using FactoryX.Application.DTOs.Requests.MachineRequests;
using FluentValidation;

namespace FactoryX.Application.Validators.MachineRequestValidators;

public class InsertMachineRequestValidator : AbstractValidator<InsertMachineRequest>
{
    public InsertMachineRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Machine name cannot be empty.");

        RuleFor(x => x.Status)
            .NotEmpty()
            .WithMessage("Machine status cannot be empty.");

        RuleFor(x => x.Capacity)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Machine capacity must be greater than or equal to 0.");
    }
}