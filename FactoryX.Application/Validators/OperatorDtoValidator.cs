using FactoryX.Application.DTOs;
using FluentValidation;

namespace FactoryX.Application.Validators;

public class OperatorDtoValidator : AbstractValidator<OperatorDto>
{
    public OperatorDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.EmployeeNumber).NotEmpty();
    }
}