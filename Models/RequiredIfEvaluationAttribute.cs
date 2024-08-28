using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace FDP.Models
{
    public class RequiredIfEvaluationTypeAttribute : ValidationAttribute
    {
        private readonly string _evalTypePropertyName;
        private readonly string _requiredForType;

        public RequiredIfEvaluationTypeAttribute(string evalTypePropertyName, string requiredForType)
        {
            _evalTypePropertyName = evalTypePropertyName;
            _requiredForType = requiredForType;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            // Retrieve the evaluation type property
            var evalTypeProperty = validationContext.ObjectType.GetProperty(_evalTypePropertyName, BindingFlags.Public | BindingFlags.Instance);
            if (evalTypeProperty == null)
                return new ValidationResult($"Unknown property: {_evalTypePropertyName}");

            // Get the value of the evaluation type property
            var evalTypeValue = evalTypeProperty.GetValue(validationContext.ObjectInstance)?.ToString();

            // Check if the value of the evaluation type property matches the required type
            if (evalTypeValue == _requiredForType && string.IsNullOrWhiteSpace(value?.ToString()))
            {
                // Return a validation error if the value is required but not provided
                return new ValidationResult($"{validationContext.DisplayName} is required for {_requiredForType}.");
            }

            return ValidationResult.Success;
        }
    }
}
