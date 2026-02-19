using System.ComponentModel.DataAnnotations;

namespace Application.Models.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Method |
                AttributeTargets.Parameter)]
public sealed class MaxAttribute : ValidationAttribute
{
    private readonly double _maxValue;

    public MaxAttribute(double maxValue)
    {
        _maxValue = maxValue;
    }

    protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null) return ValidationResult.Success;

        if (double.TryParse(value.ToString(), out var numericValue))
        {
            if (numericValue > _maxValue)
                return new ValidationResult(
                    $"The field {validationContext.DisplayName} must be less than or equal to {_maxValue}.");

            return ValidationResult.Success;
        }

        return new ValidationResult($"The field {validationContext.DisplayName} is not a valid number.");
    }
}