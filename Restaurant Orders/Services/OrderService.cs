using Microsoft.EntityFrameworkCore;
using Restaurant_Orders.Data;
using Restaurant_Orders.Data.Entities;
using Restaurant_Orders.Exceptions;

namespace Restaurant_Orders.Services
{
    public interface IOrderService
    {
        Task AddOrderItems(ICollection<long> addMenuItemIds, Order order);
        Task<Order> BuildOrder(ICollection<long> menuItemIds, long id);
        List<OrderItem> RemoveExistingOrderItems(ICollection<long> removeMenuItemIds, Order order);
    }

    public class OrderService : IOrderService
    {
        private readonly RestaurantContext _dbContext;

        public OrderService(RestaurantContext context)
        {
            _dbContext = context;
        }

        public async Task<Order> BuildOrder(ICollection<long> menuItemIds, long customerId)
        {
            Dictionary<long, int> itemCountById = GetItemsCountById(menuItemIds);

            List<MenuItem> menuItems = await CheckAndGetMenuItems(itemCountById);

            List<OrderItem> orderItems = BuildOrderItems(menuItems, itemCountById);

            decimal orderTotal = CalculateOrderTotal(orderItems);

            return new Order
            {
                CustomerId = customerId,
                OrderItems = orderItems,
                Total = orderTotal
            };
        }

        public async Task AddOrderItems(ICollection<long> addMenuItemIds, Order order)
        {
            Dictionary<long, int> itemCountById = GetItemsCountById(addMenuItemIds);

            List<MenuItem> menuItems = await CheckAndGetMenuItems(itemCountById);

            List<OrderItem> orderItems = BuildOrderItems(menuItems, itemCountById);

            var addAsNew = new List<OrderItem>();
            foreach (OrderItem orderItem in orderItems)
            {
                var existingOrderItem = order.OrderItems.Where(oi => oi.MenuItemId == orderItem.MenuItemId).FirstOrDefault();
                if (existingOrderItem != null)
                {
                    if (existingOrderItem.MenuItemPrice == orderItem.MenuItemPrice)
                    {
                        existingOrderItem.Quantity += orderItem.Quantity;
                    }
                    else
                    {
                        addAsNew.Add(orderItem);
                    }
                }
                else
                {
                    addAsNew.Add(orderItem);
                }
            }

            order.OrderItems = order.OrderItems.Concat(addAsNew).ToList();

            decimal orderTotal = CalculateOrderTotal(order.OrderItems);

            order.Total = orderTotal;
        }

        public List<OrderItem> RemoveExistingOrderItems(ICollection<long> removeMenuItemIds, Order order)
        {
            var removeItemsCountById = GetItemsCountById(removeMenuItemIds);

            var deleteExistingOrderItems = new List<OrderItem>();
            foreach (var orderItem in order.OrderItems)
            {
                var menuItemId = orderItem.MenuItemId.GetValueOrDefault();
                if (menuItemId == 0 || !removeItemsCountById.ContainsKey(menuItemId))
                {
                    continue;
                }

                int quantity = removeItemsCountById[menuItemId];
                if (quantity >= orderItem.Quantity)
                {
                    deleteExistingOrderItems.Add(orderItem);
                }
                else
                {
                    orderItem.Quantity -= quantity;
                }
            }

            order.OrderItems = order.OrderItems.Except(deleteExistingOrderItems).ToList();

            return deleteExistingOrderItems;
        }

        private static Dictionary<long, int> GetItemsCountById(ICollection<long> itemIds)
        {
            return itemIds
                .GroupBy(id => id)
                .ToDictionary(g => g.Key, g => g.Count());
        }

        private async Task<List<MenuItem>> CheckAndGetMenuItems(Dictionary<long, int> itemCountById)
        {
            var menuItems = await _dbContext.MenuItems
                .Where(menuItem => itemCountById.Select(entry => entry.Key).Contains(menuItem.Id))
                .AsNoTracking()
                .ToListAsync();

            var absentIds = itemCountById.Select(entry => entry.Key).Except(menuItems.Select(menuItem => menuItem.Id));
            if (absentIds.Any())
            {
                throw new MenuItemDoesNotExists($"The following Menu Items do not exist: [{string.Join(", ", absentIds)}]");
            }

            return menuItems;
        }

        private static List<OrderItem> BuildOrderItems(List<MenuItem> menuItems, Dictionary<long, int> itemCountById)
        {
            var orderItems = menuItems
                .Select(menuItemAndQuantity => new OrderItem
                {
                    MenuItemId = menuItemAndQuantity.Id,
                    MenuItemName = menuItemAndQuantity.Name,
                    MenuItemDescription = menuItemAndQuantity.Description,
                    MenuItemPrice = menuItemAndQuantity.Price,
                    Quantity = itemCountById[menuItemAndQuantity.Id]
                })
                .ToList();

            return orderItems;
        }

        private static decimal CalculateOrderTotal(ICollection<OrderItem> orderItems)
        {
            return orderItems.Aggregate(0.0M, (sum, oi) => sum + oi.MenuItemPrice * oi.Quantity);
        }
    }
}
