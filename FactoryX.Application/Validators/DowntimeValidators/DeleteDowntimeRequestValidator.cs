using FactoryX.Application.DTOs.Requests.DowntimeRequests;
using FluentValidation;

namespace FactoryX.Application.Validators.DowntimeValidators;

public class DeleteDowntimeRequestValidator : AbstractValidator<DeleteDowntimeRequest>
{
	public DeleteDowntimeRequestValidator()
	{
		RuleFor(x => x.Id)
			.GreaterThan(0)
			.WithMessage("Downtime ID must be greater than 0.");
		RuleFor(x => x.MachineId)
			.GreaterThan(0)
			.WithMessage("Machine ID must be greater than 0.");
	}
}
