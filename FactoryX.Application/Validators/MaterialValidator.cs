using FluentValidation;
using FactoryX.Application.DTOs;

namespace FactoryX.Application.Validators;

public class CreateMaterialRequestValidator : AbstractValidator<CreateMaterialRequest>
{
    public CreateMaterialRequestValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Material code is required.")
            .MaximumLength(50).WithMessage("Material code cannot exceed 50 characters.");

        RuleFor(x => x.SKU)
            .NotEmpty().WithMessage("SKU is required.")
            .MaximumLength(50).WithMessage("SKU cannot exceed 50 characters.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Material name is required.")
            .MaximumLength(150).WithMessage("Material name cannot exceed 150 characters.");

        RuleFor(x => x.ArabicName)
            .MaximumLength(150).WithMessage("Arabic name cannot exceed 150 characters.");

        RuleFor(x => x.Unit)
            .NotEmpty().WithMessage("Stock unit is required.")
            .MaximumLength(30).WithMessage("Stock unit cannot exceed 30 characters.");

        RuleFor(x => x.PurchaseUnit)
            .MaximumLength(30).WithMessage("Purchase unit cannot exceed 30 characters.");

        RuleFor(x => x.ConversionFactor)
            .GreaterThan(0).WithMessage("Conversion factor must be greater than 0.");

        RuleFor(x => x.MinimumStock)
            .GreaterThanOrEqualTo(0).WithMessage("Minimum stock must be greater than or equal to 0.");

        RuleFor(x => x.ReorderLevel)
            .GreaterThanOrEqualTo(x => x.MinimumStock).WithMessage("Reorder level must be greater than or equal to minimum stock.");

        RuleFor(x => x.MaximumStock)
            .GreaterThanOrEqualTo(x => x.ReorderLevel).WithMessage("Maximum stock must be greater than or equal to reorder level.");

        RuleFor(x => x.StandardCost)
            .GreaterThanOrEqualTo(0).WithMessage("Standard cost must be greater than or equal to 0.");

        RuleFor(x => x.CurrentCost)
            .GreaterThanOrEqualTo(0).WithMessage("Current cost must be greater than or equal to 0.");

        RuleFor(x => x.LastPurchaseCost)
            .GreaterThanOrEqualTo(0).WithMessage("Last purchase cost must be greater than or equal to 0.");
    }
}

public class UpdateMaterialRequestValidator : AbstractValidator<UpdateMaterialRequest>
{
    public UpdateMaterialRequestValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Valid material ID is required.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Material code is required.")
            .MaximumLength(50).WithMessage("Material code cannot exceed 50 characters.");

        RuleFor(x => x.SKU)
            .NotEmpty().WithMessage("SKU is required.")
            .MaximumLength(50).WithMessage("SKU cannot exceed 50 characters.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Material name is required.")
            .MaximumLength(150).WithMessage("Material name cannot exceed 150 characters.");

        RuleFor(x => x.ArabicName)
            .MaximumLength(150).WithMessage("Arabic name cannot exceed 150 characters.");

        RuleFor(x => x.Unit)
            .NotEmpty().WithMessage("Stock unit is required.")
            .MaximumLength(30).WithMessage("Stock unit cannot exceed 30 characters.");

        RuleFor(x => x.PurchaseUnit)
            .MaximumLength(30).WithMessage("Purchase unit cannot exceed 30 characters.");

        RuleFor(x => x.ConversionFactor)
            .GreaterThan(0).WithMessage("Conversion factor must be greater than 0.");

        RuleFor(x => x.MinimumStock)
            .GreaterThanOrEqualTo(0).WithMessage("Minimum stock must be greater than or equal to 0.");

        RuleFor(x => x.ReorderLevel)
            .GreaterThanOrEqualTo(x => x.MinimumStock).WithMessage("Reorder level must be greater than or equal to minimum stock.");

        RuleFor(x => x.MaximumStock)
            .GreaterThanOrEqualTo(x => x.ReorderLevel).WithMessage("Maximum stock must be greater than or equal to reorder level.");

        RuleFor(x => x.StandardCost)
            .GreaterThanOrEqualTo(0).WithMessage("Standard cost must be greater than or equal to 0.");

        RuleFor(x => x.CurrentCost)
            .GreaterThanOrEqualTo(0).WithMessage("Current cost must be greater than or equal to 0.");

        RuleFor(x => x.LastPurchaseCost)
            .GreaterThanOrEqualTo(0).WithMessage("Last purchase cost must be greater than or equal to 0.");
    }
}
