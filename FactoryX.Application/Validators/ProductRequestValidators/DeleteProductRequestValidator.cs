using FactoryX.Application.DTOs.Requests.ProductRequests;
using FluentValidation;

namespace FactoryX.Application.Validators.ProductRequestValidators;

public class DeleteProductRequestValidator : AbstractValidator<DeleteProductRequest>
{
	public DeleteProductRequestValidator()
	{
		RuleFor(x => x.Id)
			.NotEmpty().WithMessage("Product ID is required.")
			.GreaterThan(0).WithMessage("Product ID must be greater than zero.");
	}
}
