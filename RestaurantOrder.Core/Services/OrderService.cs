using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RestaurantOrder.Core.DTOs;
using RestaurantOrder.Core.Exceptions;
using RestaurantOrder.Core.Extensions;
using RestaurantOrder.Core.Services;
using RestaurantOrder.Data;
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
        Task<OrderDTO> UpdateOrderItems(long id, OrderUpdateDTO orderData);
        Task<OrderDTO> UpdateStatus(long id, OrderStatus newStatus, Guid version);
    }

    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly int _defaultPageSize;
        private readonly IOrderItemService _orderItemService;

        public OrderService(IUnitOfWork unitOfWork, IConfiguration configuration, IOrderItemService orderItemService)
        {
            _unitOfWork = unitOfWork;
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

            var result = await _unitOfWork.Orders.GetAllAsync(queryDetails);

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
            var order = await _unitOfWork.Orders.GetByIdAsync(id);
            if (userId != null && (order == null || order.CustomerId != userId))
            {
                order = null;
            }

            return order?.ToOrderDTO();
        }

        public async Task Delete(long id, Guid version)
        {
            _unitOfWork.Orders.Delete(new Order { Id = id, Version = version});
            await _unitOfWork.Commit();
        }

        public async Task<OrderDTO> UpdateStatus(long id, OrderStatus newStatus, Guid version)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(id) ?? throw new OrderNotFoundException();
            order.Status = newStatus;
            order.Version = Guid.NewGuid();

            await _unitOfWork.Commit();

            return order.ToOrderDTO();
        }

        public async Task<bool> IsOrderExists(long id)
        {
            return await _unitOfWork.OrderItems.IsItemExistsAsync(id);
        }

        public async Task<OrderDTO> UpdateOrderItems(long id, OrderUpdateDTO orderData)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(id) ?? throw new OrderNotFoundException();
            var deleteExistingOrderItems = await _orderItemService.UpdateOrRemoveExistingOrderItems(orderData.RemoveMenuItemIds.CountFrequency(), order);

            var newOrderItems = await _orderItemService.AddNewOrUpdateExistingOrderItems(orderData.AddMenuItemIds.CountFrequency(), order);

            order.OrderItems = order.OrderItems
                .Except(deleteExistingOrderItems)
                .Concat(newOrderItems)
                .ToList();

            var orderTotal = CalculateOrderTotal(order.OrderItems);
            order.Total = orderTotal;
            order.Version = Guid.NewGuid();

            await _unitOfWork.Commit();

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

            _unitOfWork.Orders.Add(order);
            await _unitOfWork.Commit();

            return order.ToOrderDTO();
        }

        private static decimal CalculateOrderTotal(ICollection<OrderItem> orderItems)
        {
            return orderItems.Aggregate(0.0M, (sum, oi) => sum + oi.MenuItemPrice * oi.Quantity);
        }

        private void AddSearchQuery(QueryDetailsDTO<Order> queryDetails, string searchTerm)
        {
            queryDetails.WhereQueries.Add(order =>
                _unitOfWork.OrderItems.SearchInName(searchTerm)
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
