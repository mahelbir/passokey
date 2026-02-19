using System.ComponentModel.DataAnnotations;

namespace Application.Models.Attributes;

[AttributeUsage(
    AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Parameter)]
public sealed class GuidDataTypeAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null || string.IsNullOrWhiteSpace(value.ToString())) return ValidationResult.Success;

        if (Guid.TryParse(value.ToString(), out var parsedGuid) && parsedGuid != Guid.Empty)
            return ValidationResult.Success;

        if (ErrorMessage != null) return new ValidationResult(ErrorMessage);

        return new ValidationResult($"The field {validationContext.DisplayName} is invalid.");
    }
}