using FactoryX.Application.DTOs.Requests.ProductRequests;
using FluentValidation;

namespace FactoryX.Application.Validators.ProductRequestValidators;

public class UpdateProductRequestValidator : AbstractValidator<UpdateProductRequest>
{
	public UpdateProductRequestValidator()
	{
		RuleFor(x => x.Name)
			.NotEmpty().WithMessage("Product name is required.")
			.MaximumLength(100).WithMessage("Product name must not exceed 100 characters.");
		RuleFor(x => x.Code)
			.NotEmpty().WithMessage("Product code is required.")
			.MaximumLength(50).WithMessage("Product code must not exceed 50 characters.");
		RuleFor(x => x.Description)
			.NotEmpty().WithMessage("Product description is required.")
			.MaximumLength(500).WithMessage("Product description must not exceed 500 characters.");
	}
}
