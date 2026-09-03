using FactoryX.Application.DTOs.Requests.AuthenticationRequests;
using FluentValidation;

namespace FactoryX.Application.Validators.AuthenticationValidators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
	public RegisterRequestValidator()
	{
		RuleFor(x => x.Username)
			.NotEmpty().WithMessage("Username is required.")
			.MinimumLength(3).WithMessage("Username must be at least 3 characters long.")
			.MaximumLength(50).WithMessage("Username must not exceed 50 characters.");

		RuleFor(x => x.Password)
			.NotEmpty().WithMessage("Password is required.")
			.MinimumLength(6).WithMessage("Password must be at least 6 characters long.")
			.MaximumLength(100).WithMessage("Password must not exceed 100 characters.");

		RuleFor(x => x.ConfirmPassword)
			.NotEmpty().WithMessage("Confirm Password is required.")
			.Equal(x => x.Password).WithMessage("Confirm Password must match Password.");
	}
}
