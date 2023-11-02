using RestaurantOrder.Core.DTOs;
using System.ComponentModel.DataAnnotations;

namespace RestaurantOrder.Core.Validations
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class NotEmptyMenuItemsAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is not OrderUpdateDTO orderUpdateDTO)
            {
                return new ValidationResult($"This attribute works only in the '{nameof(OrderUpdateDTO)}' type.");
            }

            var addMenuItemIds = orderUpdateDTO.AddMenuItemIds;
            var removeMenuItemIds = orderUpdateDTO.RemoveMenuItemIds;

            if (!addMenuItemIds.Any() && !removeMenuItemIds.Any())
            {
                return new ValidationResult($"Either of {nameof(orderUpdateDTO.AddMenuItemIds)} or {nameof(orderUpdateDTO.RemoveMenuItemIds)} field is required.");
            }

            return ValidationResult.Success;
        }
    }
}
