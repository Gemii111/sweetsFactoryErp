using FactoryX.Application.DTOs.Requests.DowntimeRequests;
using FluentValidation;

namespace FactoryX.Application.Validators.DowntimeValidators;

public class UpdateDowntimeRequestValidator : AbstractValidator<UpdateDowntimeRequest>
{
	public UpdateDowntimeRequestValidator()
	{
		RuleFor(x => x.StartTime)
			.NotEmpty().WithMessage("Start time is required.")
			.Must(start => start < DateTime.Now).WithMessage("Start time must be in the past.");
		RuleFor(x => x.EndTime)
			.NotEmpty().WithMessage("End time is required.")
			.Must((request, end) => end > request.StartTime).WithMessage("End time must be after start time.");
		RuleFor(x => x.Reason)
			.NotEmpty().WithMessage("Reason is required.")
			.MaximumLength(200).WithMessage("Reason must not exceed 200 characters.");
	}
}
