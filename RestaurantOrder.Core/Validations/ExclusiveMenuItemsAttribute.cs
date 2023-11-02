using RestaurantOrder.Core.DTOs;
using RestaurantOrder.Core.Extensions;
using System.ComponentModel.DataAnnotations;

namespace RestaurantOrder.Core.Validations
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ExclusiveMenuItemsAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (validationContext == null)
            {
                return new ValidationResult("Validation context not found.");
            }

            OrderUpdateDTO orderUpdateDTO = validationContext.ObjectInstance as OrderUpdateDTO;
            if (orderUpdateDTO == null || validationContext.ObjectType != typeof(OrderUpdateDTO))
            {
                return new ValidationResult($"This attribute works only inside the '{nameof(OrderUpdateDTO)}' type.");
            }

            var propertyName = validationContext.MemberName;

            var addMenuItemIds = orderUpdateDTO.AddMenuItemIds;
            var removeMenuItemIds = orderUpdateDTO.RemoveMenuItemIds;

            var addIdsDict = addMenuItemIds.CountFrequency();

            foreach (var remMenuItemId in removeMenuItemIds)
            {
                if (addIdsDict.ContainsKey(remMenuItemId))
                {
                    if (propertyName == nameof(orderUpdateDTO.AddMenuItemIds))
                    {
                        return new ValidationResult($"The values of {nameof(orderUpdateDTO.AddMenuItemIds)} must not overlap with {nameof(orderUpdateDTO.RemoveMenuItemIds)}.", new List<string> { nameof(orderUpdateDTO.AddMenuItemIds) });
                    }
                    else if (propertyName == nameof(orderUpdateDTO.RemoveMenuItemIds))
                    {
                        return new ValidationResult($"The values of {nameof(orderUpdateDTO.RemoveMenuItemIds)} must not overlap with {nameof(orderUpdateDTO.AddMenuItemIds)}.", new List<string> { nameof(orderUpdateDTO.RemoveMenuItemIds) });
                    }
                }
            }

            return ValidationResult.Success;
        }
    }
}
