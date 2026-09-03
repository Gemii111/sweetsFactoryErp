using FactoryX.Application.DTOs.Requests.MachineRequests;
using FluentValidation;

namespace FactoryX.Application.Validators.MachineRequestValidators;

public class DeleteMachineRequestValidator : AbstractValidator<DeleteMachineRequest>
{
    public DeleteMachineRequestValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Machine ID must be greater than 0.");
    }
}