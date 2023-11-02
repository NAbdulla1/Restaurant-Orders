using RestaurantOrder.Core.Exceptions;
using RestaurantOrder.Data.Models;
using RestaurantOrder.Data.Repositories;

namespace RestaurantOrder.Core.Services
{
    public interface IOrderItemService
    {
        Task<IEnumerable<OrderItem>> AddNewOrUpdateExistingOrderItems(Dictionary<long, int> itemCountById, Order order);
        List<OrderItem> BuildOrderItems(IEnumerable<MenuItem> menuItems, Dictionary<long, int> itemCountById);
        Task<IEnumerable<MenuItem>> CheckAndGetMenuItems(Dictionary<long, int> itemCountById);
        Task<List<OrderItem>> UpdateOrRemoveExistingOrderItems(IDictionary<long, int> removeItemsCountById, Order order);
    }

    public class OrderItemService : IOrderItemService
    {
        private readonly IOrderItemRepository _orderItemRepository;
        private readonly IMenuItemRepository _menuItemRepository;

        public OrderItemService(IOrderItemRepository orderItemRepository, IMenuItemRepository menuItemRepository)
        {
            _orderItemRepository = orderItemRepository;
            _menuItemRepository = menuItemRepository;
        }

        public async Task<IEnumerable<OrderItem>> AddNewOrUpdateExistingOrderItems(Dictionary<long, int> itemCountById, Order order)
        {
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

        public async Task<List<OrderItem>> UpdateOrRemoveExistingOrderItems(IDictionary<long, int> removeItemsCountById, Order order)
        {
            var deleteExistingOrderItems = new List<OrderItem>();
            var updatedOrderItems = new List<OrderItem>();
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
                    updatedOrderItems.Add(orderItem);
                }
            }

            var menuItems = await _menuItemRepository.GetByIds(updatedOrderItems.Select(oi => oi.MenuItemId.GetValueOrDefault()).ToList());
            foreach (var menuItem in menuItems)
            {
                var orderItem = updatedOrderItems.FirstOrDefault(oi => oi.MenuItemId == menuItem.Id);
                if (orderItem != null)
                {
                    orderItem.MenuItemPrice = menuItem.Price;
                    orderItem.MenuItemName = menuItem.Name;
                    orderItem.MenuItemDescription = menuItem.Description;
                }
            }

            _orderItemRepository.DeleteMany(deleteExistingOrderItems);

            return deleteExistingOrderItems;
        }

        public async Task<IEnumerable<MenuItem>> CheckAndGetMenuItems(Dictionary<long, int> itemCountById)
        {
            var menuItems = await _menuItemRepository.GetByIds(itemCountById.Select(item => item.Key).ToList());

            var absentIds = itemCountById.Select(entry => entry.Key).Except(menuItems.Select(menuItem => menuItem.Id));
            if (absentIds.Any())
            {
                throw new MenuItemNotFountException($"The following Menu Items do not exist: [{string.Join(", ", absentIds)}]");
            }

            return menuItems;
        }

        public List<OrderItem> BuildOrderItems(IEnumerable<MenuItem> menuItems, Dictionary<long, int> itemCountById)
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
    }
}
