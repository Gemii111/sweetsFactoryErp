using FactoryX.Application.DTOs;
using FluentValidation;

namespace FactoryX.Application.Validators;

public class MaterialDtoValidator : AbstractValidator<MaterialDto>
{
    public MaterialDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Code).NotEmpty();
        RuleFor(x => x.Unit).NotEmpty();
    }
}