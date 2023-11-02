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
    public interface IMenuItemService
    {
        Task<MenuItemDTO> Create(MenuItemDTO item);
        Task Delete(long id);
        Task<MenuItemDTO> GetById(long id);
        Task<PagedData<MenuItemDTO>> Get(IndexingDTO indexData);
        Task<MenuItemDTO> Update(MenuItemDTO menuItemDto);
    }

    public class MenuItemService : IMenuItemService
    {
        private readonly IMenuItemRepository _menuItemRepository;
        private readonly int _defaultPageSize;

        public MenuItemService(IMenuItemRepository menuItemRepository, IConfiguration configuration)
        {
            _menuItemRepository = menuItemRepository;
            _defaultPageSize = configuration.GetValue<int>("DefaultPageSize");
        }

        public async Task<MenuItemDTO> GetById(long menuItemId)
        {
            var menuItem = await _menuItemRepository.GetById(menuItemId);

            if (menuItem == null)
            {
                throw new MenuItemNotFountException();
            }

            return menuItem.ToMenuItemDTO();
        }

        public async Task<MenuItemDTO> Create(MenuItemDTO item)
        {
            var menuItem = item.ToMenuItem();
            _menuItemRepository.Add(menuItem);
            await _menuItemRepository.Commit();

            return menuItem.ToMenuItemDTO();
        }

        public async Task<MenuItemDTO> Update(MenuItemDTO menuItemDTO)
        {
            _menuItemRepository.UpdateMenuItem(menuItemDTO.ToMenuItem());
            var updateCount = await _menuItemRepository.Commit();

            if (updateCount <= 0)
            {
                throw new MenuItemNotFountException();
            }

            return menuItemDTO;
        }

        public async Task Delete(long id)
        {
            _menuItemRepository.Delete(new MenuItem { Id = id });
            var deleteCount = await _menuItemRepository.Commit();

            if(deleteCount <= 0)
            {
                throw new MenuItemNotFountException();
            }
        }

        public async Task<PagedData<MenuItemDTO>> Get(IndexingDTO indexData)
        {
            var queryDetails = new QueryDetailsDTO<MenuItem>(indexData.Page, indexData.PageSize ?? _defaultPageSize);
            if (indexData.SearchBy != null)
            {
                queryDetails.WhereQueries.Add(GetSearchQuery(indexData.SearchBy));
            }

            AddSortQuery(queryDetails, indexData.SortBy, indexData.SortOrder);

            var pageData = await _menuItemRepository.GetAll(queryDetails);

            return new PagedData<MenuItemDTO>
            {
                Page = queryDetails.Page,
                PageSize = queryDetails.PageSize,
                TotalItems = pageData.Total,
                ItemList = pageData.Data.Select(menuItem => menuItem.ToMenuItemDTO())
            };
        }

        private static Expression<Func<MenuItem, bool>> GetSearchQuery(string searchTerm)
        {
            return item =>
                        item.Name.ToLower().Contains(searchTerm.ToLower()) ||
                        (
                            item.Description != null && item.Description.ToLower().Contains(searchTerm.ToLower())
                        );
        }

        private static void AddSortQuery(QueryDetailsDTO<MenuItem> queryDetails, string? sortColumn, string? sortDirection)
        {
            sortDirection ??= "asc";
            sortColumn ??= "id";

            queryDetails.SortOrder = sortDirection;
            queryDetails.OrderingExpr = GetOrderExpression(sortColumn);
        }

        private static Expression<Func<MenuItem, object?>> GetOrderExpression(string sortColumn)
        {
            switch (sortColumn.ToLower())
            {
                case "name":
                    return item => item.Name;
                case "description":
                    return item => item.Description;
                case "price":
                    return item => item.Price;
                default:
                    return item => item.Id;
            }
        }
    }
}
