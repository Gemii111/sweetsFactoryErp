using FactoryX.Application.DTOs.Requests.MachineRequests;
using FactoryX.Application.DTOs.Requests.OperatorRequests;
using FluentValidation;

namespace FactoryX.Application.Validators.OperatorRequestValidators;

public class InsertOperatorRequestValidator : AbstractValidator<InsertOperatorRequest>
{
    public InsertOperatorRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Operator name cannot be empty.");

        RuleFor(x => x.EmployeeNumber)
            .NotEmpty()
            .WithMessage("Employee number cannot be empty.");
    }
}