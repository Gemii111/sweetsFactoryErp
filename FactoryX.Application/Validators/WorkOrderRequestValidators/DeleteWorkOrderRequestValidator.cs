using FactoryX.Application.DTOs.Requests.WorkOrderRequests;
using FluentValidation;

namespace FactoryX.Application.Validators.WorkOrderRequestValidators;

public class DeleteWorkOrderRequestValidator : AbstractValidator<DeleteWorkOrderRequest>
{
	public DeleteWorkOrderRequestValidator()
	{
		RuleFor(x => x.Id)
			.NotEmpty().WithMessage("Work Order ID is required.")
			.GreaterThan(0).WithMessage("Work Order ID must be greater than zero.");
	}
}
