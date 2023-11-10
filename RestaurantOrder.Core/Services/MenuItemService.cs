using Microsoft.Extensions.Configuration;
using RestaurantOrder.Core.DTOs;
using RestaurantOrder.Core.Exceptions;
using RestaurantOrder.Core.Extensions;
using RestaurantOrder.Data;
using RestaurantOrder.Data.Models;
using RestaurantOrder.Data.Models.DTOs;
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
        private readonly IUnitOfWork _unitOfWork;
        private readonly int _defaultPageSize;

        public MenuItemService(IUnitOfWork unitOfWork, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _defaultPageSize = configuration.GetValue<int>("DefaultPageSize");
        }

        public async Task<MenuItemDTO> GetById(long menuItemId)
        {
            var menuItem = await _unitOfWork.MenuItems.GetByIdAsync(menuItemId);

            if (menuItem == null)
            {
                throw new MenuItemNotFountException();
            }

            return menuItem.ToMenuItemDTO();
        }

        public async Task<MenuItemDTO> Create(MenuItemDTO item)
        {
            var menuItem = item.ToMenuItem();
            _unitOfWork.MenuItems.Add(menuItem);
            await _unitOfWork.Commit();

            return menuItem.ToMenuItemDTO();
        }

        public async Task<MenuItemDTO> Update(MenuItemDTO menuItemDTO)
        {
            var menuItem = await _unitOfWork.MenuItems.GetByIdAsync(menuItemDTO.Id);

            if (menuItem == null)
            {
                throw new MenuItemNotFountException();
            }

            menuItem.Name = menuItemDTO.Name;
            menuItem.Price = menuItemDTO.Price;
            menuItem.Description = menuItemDTO.Description;

            await _unitOfWork.Commit();

            return menuItemDTO;
        }

        public async Task Delete(long id)
        {
            var menuItem = await _unitOfWork.MenuItems.GetByIdAsync(id);

            if (menuItem == null)
            {
                throw new MenuItemNotFountException();
            }
            _unitOfWork.MenuItems.Delete(menuItem);

            await _unitOfWork.Commit();
        }

        public async Task<PagedData<MenuItemDTO>> Get(IndexingDTO indexData)
        {
            var queryDetails = new QueryDetailsDTO<MenuItem>(indexData.Page, indexData.PageSize ?? _defaultPageSize);
            if (indexData.SearchBy != null)
            {
                queryDetails.WhereQueries.Add(GetSearchQuery(indexData.SearchBy));
            }

            AddSortQuery(queryDetails, indexData.SortBy, indexData.SortOrder);

            var pageData = await _unitOfWork.MenuItems.GetAllAsync(queryDetails);

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
