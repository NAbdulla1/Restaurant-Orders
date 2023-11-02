using System.ComponentModel.DataAnnotations;

namespace RestaurantOrder.Core.Validations
{
    public class RequiredGuidAttribute : ValidationAttribute
    {
        public RequiredGuidAttribute()
        {
            ErrorMessage = "The {0} field is required.";
        }

        public override bool IsValid(object? value)
        {
            return value != null
                && value is Guid
                && !Guid.Empty.Equals(value);
        }
    }
}
