using FactoryX.Application.DTOs.Requests.OperatorRequests;
using FluentValidation;

namespace FactoryX.Application.Validators.OperatorRequestValidators;

public class DeleteOperatorRequestValidator : AbstractValidator<DeleteOperatorRequest>
{
    public DeleteOperatorRequestValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Operator ID must be greater than 0.");
    }
}