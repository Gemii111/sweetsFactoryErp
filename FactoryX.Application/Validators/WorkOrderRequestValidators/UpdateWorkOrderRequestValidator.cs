using FactoryX.Application.DTOs.Requests.WorkOrderRequests;
using FluentValidation;

namespace FactoryX.Application.Validators.WorkOrderRequestValidators;

public class UpdateWorkOrderRequestValidator : AbstractValidator<UpdateWorkOrderRequest>
{
	public UpdateWorkOrderRequestValidator()
	{
		RuleFor(x => x.ProductId)
			.GreaterThan(0).WithMessage("Product ID must be greater than 0.");

		RuleFor(x => x.MachineId)
			.GreaterThan(0).WithMessage("Machine ID must be greater than 0.");

		RuleFor(x => x.Quantity)
			.GreaterThan(0).WithMessage("Quantity must be greater than 0.");

		RuleFor(x => x.StartDate)
			.NotEmpty().WithMessage("Start date is required.")
			.Must(startDate => startDate <= DateTime.Now).WithMessage("Start date cannot be in the future.");

		RuleFor(x => x.EndDate)
			.NotEmpty().WithMessage("End date is required.")
			.Must((request, endDate) => endDate > request.StartDate)
			.WithMessage("End date must be greater than start date.");

		RuleFor(x => x.Status)
			.NotEmpty().WithMessage("Status is required.")
			.Must(status => status == "Pending" || status == "InProgress" || status == "Completed")
			.WithMessage("Status must be either 'Pending', 'InProgress', or 'Completed'.");

		RuleFor(x => x.ProductName)
			.MaximumLength(100).WithMessage("Product name must not exceed 100 characters.");

		RuleFor(x => x.MachineName)
			.MaximumLength(100).WithMessage("Machine name must not exceed 100 characters.");
	}
}
