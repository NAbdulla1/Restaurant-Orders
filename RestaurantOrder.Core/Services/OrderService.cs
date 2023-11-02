using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RestaurantOrder.Core.DTOs;
using RestaurantOrder.Core.Exceptions;
using RestaurantOrder.Core.Extensions;
using RestaurantOrder.Data.Models;
using RestaurantOrder.Data.Models.DTOs;
using RestaurantOrder.Data.Repositories;
using System.Linq.Expressions;

namespace Restaurant_Orders.Services
{
    public interface IOrderService
    {
        Task<OrderDTO> Create(ICollection<long> menuItemIds, long customerId);
        Task Delete(long id, Guid version);
        Task<PagedData<OrderDTO>> Get(IndexingDTO indexData, OrderFilterDTO orderFilterData);
        Task<OrderDTO?> GetById(long id, long? userId = null);
        Dictionary<long, int> GetItemsCountById(ICollection<long> itemIds);
        Task<bool> IsOrderExists(long id);
        Task<OrderDTO> UpdateOrderItems(OrderDTO order, OrderUpdateDTO orderData);
        Task<OrderDTO> UpdateStatus(long id, OrderStatus newStatus, Guid version);
        Task<OrderDTO> UpdateStatus(OrderDTO orderDTO, OrderStatus newStatus, Guid version);
    }

    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IMenuItemRepository _menuItemRepository;
        private readonly IOrderItemRepository _orderItemRepository;
        private readonly int _defaultPageSize;

        public OrderService(IOrderRepository orderRepository, IMenuItemRepository menuItemRepository, IOrderItemRepository orderItemRepository, IConfiguration configuration)
        {
            _orderRepository = orderRepository;
            _menuItemRepository = menuItemRepository;
            _orderItemRepository = orderItemRepository;
            _defaultPageSize = configuration.GetValue<int>("DefaultPageSize");
        }

        public async Task<PagedData<OrderDTO>> Get(IndexingDTO indexData, OrderFilterDTO orderFilters)
        {
            int pageSize = indexData.PageSize ?? _defaultPageSize;
            var queryDetails = new QueryDetailsDTO<Order>(indexData.Page, pageSize);
            if (indexData.SearchBy != null)
            {
                AddSearchQuery(queryDetails, indexData.SearchBy);
            }

            if (orderFilters.CustomerId != null)
            {
                AddCustomerQuery(queryDetails, orderFilters.CustomerId.GetValueOrDefault());
            }

            if (orderFilters.Status != null)
            {
                AddOrderStatusQuery(queryDetails, orderFilters.Status);
            }

            if (indexData.SortBy != null)
            {
                AddSortQuery(queryDetails, indexData.SortBy, indexData.SortOrder);
            }

            var result = await _orderRepository.GetAll(queryDetails);

            return new PagedData<OrderDTO>
            {
                Page = queryDetails.Page,
                PageSize = queryDetails.PageSize,
                TotalItems = result.Total,
                ItemList = result.Data.Select(order => order.ToOrderDTO())
            };
        }

        public async Task<OrderDTO?> GetById(long id, long? userId = null)
        {
            var order = await _orderRepository.GetById(id);
            if (userId != null && (order == null || order.CustomerId != userId))
            {
                order = null;
            }

            return order?.ToOrderDTO();
        }

        public async Task Delete(long id, Guid version)
        {
            _orderRepository.Delete(id, version);
            await _orderRepository.Commit();
        }

        public async Task<OrderDTO> UpdateStatus(long id, OrderStatus newStatus, Guid version)
        {
            var order = await _orderRepository.GetById(id);
            if (order == null)
            {
                throw new OrderNotFoundException();
            }

            return await UpdateStatus(order.ToOrderDTO(), newStatus, version);
        }

        public async Task<OrderDTO> UpdateStatus(OrderDTO orderDTO, OrderStatus newStatus, Guid version)
        {
            orderDTO.Status = newStatus.ToString();
            var order = orderDTO.ToOrder();

            _orderRepository.UpdateOrder(order, version);
            await _orderRepository.Commit();

            return order.ToOrderDTO();
        }

        public async Task<bool> IsOrderExists(long id)
        {
            return await _orderRepository.OrderExists(id);
        }

        public async Task<OrderDTO> UpdateOrderItems(OrderDTO orderDTO, OrderUpdateDTO orderData)
        {
            var order = orderDTO.ToOrder();
            var deleteExistingOrderItems = UpdateOrRemoveExistingOrderItems(orderData.RemoveMenuItemIds, order);

            var newOrderItems = await AddNewOrderItems(orderData.AddMenuItemIds, order);

            order.OrderItems = order.OrderItems
                .Except(deleteExistingOrderItems)
                .Concat(newOrderItems)
                .ToList();

            var orderTotal = CalculateOrderTotal(order.OrderItems);
            order.Total = orderTotal;

            order = _orderRepository.UpdateOrder(order, orderData.Version);
            await _orderRepository.Commit();

            return order.ToOrderDTO();
        }

        public async Task<OrderDTO> Create(ICollection<long> menuItemIds, long customerId)
        {
            Dictionary<long, int> itemCountById = GetItemsCountById(menuItemIds);

            var menuItems = await CheckAndGetMenuItems(itemCountById);

            var orderItems = BuildOrderItems(menuItems, itemCountById);

            decimal orderTotal = CalculateOrderTotal(orderItems);

            var order = new Order
            {
                CustomerId = customerId,
                OrderItems = orderItems,
                Total = orderTotal,
                Version = Guid.NewGuid()
            };

            order = _orderRepository.Add(order);
            await _orderRepository.Commit();

            return order.ToOrderDTO();
        }

        private async Task<IEnumerable<OrderItem>> AddNewOrderItems(ICollection<long> addMenuItemIds, Order order)
        {
            Dictionary<long, int> itemCountById = GetItemsCountById(addMenuItemIds);

            var menuItems = await CheckAndGetMenuItems(itemCountById);

            var orderItems = BuildOrderItems(menuItems, itemCountById);

            var addAsNew = new List<OrderItem>();
            foreach (var orderItem in orderItems)
            {
                var existingOrderItem = order.OrderItems.Where(oi => oi.MenuItemId == orderItem.MenuItemId).FirstOrDefault();
                if (existingOrderItem != null)
                {
                    existingOrderItem.Quantity += orderItem.Quantity;
                    existingOrderItem.MenuItemName = orderItem.MenuItemName;
                    existingOrderItem.MenuItemPrice = orderItem.MenuItemPrice;
                    existingOrderItem.MenuItemDescription = orderItem.MenuItemDescription;
                    _orderItemRepository.Update(existingOrderItem);
                }
                else
                {
                    var newOrderItem = _orderItemRepository.Add(orderItem);
                    addAsNew.Add(newOrderItem);
                }
            }

            return addAsNew;
        }

        private List<OrderItem> UpdateOrRemoveExistingOrderItems(ICollection<long> removeMenuItemIds, Order order)
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

                var quantityToReduce = removeItemsCountById[menuItemId];
                if (quantityToReduce >= orderItem.Quantity)
                {
                    deleteExistingOrderItems.Add(orderItem);
                }
                else
                {
                    orderItem.Quantity -= quantityToReduce;
                    _orderItemRepository.Update(orderItem);
                }
            }

            _orderItemRepository.DeleteMany(deleteExistingOrderItems);

            return deleteExistingOrderItems;
        }

        public Dictionary<long, int> GetItemsCountById(ICollection<long> itemIds)
        {
            return itemIds
                .GroupBy(id => id)
                .ToDictionary(g => g.Key, g => g.Count());
        }

        private async Task<IEnumerable<MenuItem>> CheckAndGetMenuItems(Dictionary<long, int> itemCountById)
        {
            var menuItems = await _menuItemRepository.GetByIds(itemCountById.Select(item => item.Key).ToList());

            var absentIds = itemCountById.Select(entry => entry.Key).Except(menuItems.Select(menuItem => menuItem.Id));
            if (absentIds.Any())
            {
                throw new MenuItemNotFountException($"The following Menu Items do not exist: [{string.Join(", ", absentIds)}]");
            }

            return menuItems;
        }

        private static List<OrderItem> BuildOrderItems(IEnumerable<MenuItem> menuItems, Dictionary<long, int> itemCountById)
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

        private void AddSearchQuery(QueryDetailsDTO<Order> queryDetails, string searchTerm)
        {
            queryDetails.WhereQueries.Add(order =>
                _orderItemRepository.SearchInName(searchTerm)
                    .Select(oi => oi.OrderId)
                        .Contains(order.Id));
        }

        private static void AddCustomerQuery(QueryDetailsDTO<Order> queryDetails, long customerId)
        {
            queryDetails.WhereQueries.Add(order => order.CustomerId == customerId);
        }

        private static void AddOrderStatusQuery(QueryDetailsDTO<Order> queryDetails, OrderStatus? status)
        {
            queryDetails.WhereQueries.Add(order => order.Status == status);
        }

        private static void AddSortQuery(QueryDetailsDTO<Order> queryDetails, string sortColumn, string? sortDirection)
        {
            if (sortDirection == null)
            {
                sortDirection = "asc";
            }

            queryDetails.SortOrder = sortDirection;
            queryDetails.OrderingExpr = GetOrderExpression(sortColumn);
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
