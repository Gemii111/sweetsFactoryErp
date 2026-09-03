using FactoryX.Application.DTOs.Requests.AuthenticationRequests;
using FluentValidation;

namespace FactoryX.Application.Validators.AuthenticationValidators;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
	public LoginRequestValidator()
	{
		RuleFor(x => x.Username)
			.NotEmpty().WithMessage("Username is required.")
			.MaximumLength(50).WithMessage("Username must not exceed 50 characters.");

		RuleFor(x => x.Password)
			.NotEmpty().WithMessage("Password is required.")
			.MinimumLength(6).WithMessage("Password must be at least 6 characters long.")
			.MaximumLength(100).WithMessage("Password must not exceed 100 characters.");
	}
}
