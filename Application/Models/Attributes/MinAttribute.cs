using System.ComponentModel.DataAnnotations;

namespace Application.Models.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Method |
                AttributeTargets.Parameter)]
public sealed class MinAttribute : ValidationAttribute
{
    private readonly double _minValue;

    public MinAttribute(double minValue)
    {
        _minValue = minValue;
    }

    protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null) return ValidationResult.Success;

        if (double.TryParse(value.ToString(), out var numericValue))
        {
            if (numericValue < _minValue)
                return new ValidationResult(
                    $"The field {validationContext.DisplayName} must be greater than or equal to {_minValue}.");

            return ValidationResult.Success;
        }

        return new ValidationResult($"The field {validationContext.DisplayName} is not a valid number.");
    }
}