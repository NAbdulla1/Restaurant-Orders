using Microsoft.EntityFrameworkCore;
using Restaurant_Orders.Data;
using Restaurant_Orders.Exceptions;
using Restaurant_Orders.Models;
using Restaurant_Orders.Models.DTOs;
using System.Linq.Expressions;

namespace Restaurant_Orders.Services
{
    public interface IOrderService
    {
        Task AddOrderItems(ICollection<long> addMenuItemIds, Order order);
        Task<Order> BuildOrder(ICollection<long> menuItemIds, long id);
        List<OrderItem> RemoveExistingOrderItems(ICollection<long> removeMenuItemIds, Order order);
        IQueryable<Order> PrepareIndexQuery(IQueryable<Order> query, IndexingDTO indexData, OrderFilterDTO orderFilters);
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
                    existingOrderItem.Quantity += orderItem.Quantity;
                    existingOrderItem.MenuItemName = orderItem.MenuItemName;
                    existingOrderItem.MenuItemPrice = orderItem.MenuItemPrice;
                    existingOrderItem.MenuItemDescription = orderItem.MenuItemDescription;
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

        public IQueryable<Order> PrepareIndexQuery(IQueryable<Order> query, IndexingDTO indexData, OrderFilterDTO orderFilters)
        {
            if (indexData.SearchBy != null)
            {
                query = AddSearchQuery(query, indexData.SearchBy);
            }

            if (orderFilters.CustomerId != null)
            {
                query = AddCustomerQuery(query, orderFilters.CustomerId.GetValueOrDefault());
            }

            if (orderFilters.Status != null)
            {
                query = AddOrderStatusQuery(query, orderFilters.Status);
            }

            if (indexData.SortBy != null)
            {
                query = AddSortQuery(query, indexData.SortBy, indexData.SortOrder);
            }

            return query;
        }

        private IQueryable<Order> AddSearchQuery(IQueryable<Order> query, string searchTerm)
        {
            query = query.Where(order =>
                _dbContext.OrderItems
                    .Where(oi => oi.MenuItemName != null && oi.MenuItemName.Contains(searchTerm))
                    .Select(oi => oi.OrderId)
                        .Contains(order.Id));

            return query;
        }

        private static IQueryable<Order> AddCustomerQuery(IQueryable<Order> query, long customerId)
        {
            query = query.Where(order => order.CustomerId == customerId);

            return query;
        }

        private static IQueryable<Order> AddOrderStatusQuery(IQueryable<Order> query, OrderStatus? status)
        {
            query = query.Where(order => order.Status == status);

            return query;
        }

        private static IQueryable<Order> AddSortQuery(IQueryable<Order> query, string sortColumn, string? sortDirection)
        {
            if (sortDirection == null || sortDirection == "asc")
            {
                query = query.OrderBy(GetOrderExpression(sortColumn));
            }
            else
            {
                query = query.OrderByDescending(GetOrderExpression(sortColumn));
            }

            return query;
        }

        private static Expression<Func<Order, object?>> GetOrderExpression(string sortColumn)
        {
            switch (sortColumn.ToLower())
            {
                case "customerid":
                    return item => item.CustomerId;
                case "createdat":
                    return item => item.CreatedAt;
                case "total":
                    return item => item.Total;
                default:
                    return item => item.Id;
            }
        }
    }
}
