using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RestaurantOrder.Core.DTOs;
using RestaurantOrder.Core.Exceptions;
using RestaurantOrder.Core.Extensions;
using RestaurantOrder.Core.Services;
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
        Task<bool> IsOrderExists(long id);
        Task<OrderDTO> UpdateOrderItems(OrderDTO order, OrderUpdateDTO orderData);
        Task<OrderDTO> UpdateStatus(long id, OrderStatus newStatus, Guid version);
        Task<OrderDTO> UpdateStatus(OrderDTO orderDTO, OrderStatus newStatus, Guid version);
    }

    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderItemRepository _orderItemRepository;
        private readonly int _defaultPageSize;
        private readonly IOrderItemService _orderItemService;

        public OrderService(IOrderRepository orderRepository, IOrderItemRepository orderItemRepository, IConfiguration configuration, IOrderItemService orderItemService)
        {
            _orderRepository = orderRepository;
            _orderItemRepository = orderItemRepository;
            _defaultPageSize = configuration.GetValue<int>("DefaultPageSize");
            _orderItemService = orderItemService;
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

            AddSortQuery(queryDetails, indexData.SortBy, indexData.SortOrder);

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
            var deleteExistingOrderItems = await _orderItemService.UpdateOrRemoveExistingOrderItems(orderData.RemoveMenuItemIds.CountFrequency(), order);

            var newOrderItems = await _orderItemService.AddNewOrUpdateExistingOrderItems(orderData.AddMenuItemIds.CountFrequency(), order);

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
            var itemCountById = menuItemIds.CountFrequency();

            var menuItems = await _orderItemService.CheckAndGetMenuItems(itemCountById);

            var orderItems = _orderItemService.BuildOrderItems(menuItems, itemCountById);

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

        private static void AddSortQuery(QueryDetailsDTO<Order> queryDetails, string? sortColumn, string? sortDirection)
        {
            sortDirection ??= "asc";
            sortColumn ??= "id";

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
