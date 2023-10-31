using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Restaurant_Orders.Extensions;
using Restaurant_Orders.Models.DTOs;
using Restaurant_Orders.Services;
using RestaurantOrder.Data.Models;
using RestaurantOrder.Data.Repositories;
using System.Net.Mime;

namespace Restaurant_Orders.Controllers
{
    [ApiController]
    [Route("api/menu-items")]
    public class MenuItemsController : ControllerBase
    {
        private readonly IMenuItemRepository _menuItemRepository;
        private readonly IMenuItemService _menuItemService;
        private readonly IPaginationService<MenuItem> _paginationService;

        public MenuItemsController(IMenuItemRepository menuItemRepository, IMenuItemService menuItemService, IPaginationService<MenuItem> paginationService)
        {
            _menuItemRepository = menuItemRepository;
            _menuItemService = menuItemService;
            _paginationService = paginationService;
        }

        [HttpGet]
        [Authorize(Roles = "RestaurantOwner,Customer")]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<ActionResult<PagedData<MenuItem>>> GetMenuItems([FromQuery] IndexingDTO indexData)
        {
            if (indexData.SortBy != null && !typeof(MenuItem).FieldExists(indexData.SortBy))
            {
                ModelState.AddModelError(nameof(indexData.SortBy), "Can't find the provided sort property.");
                return ValidationProblem();
            }

            var query = _menuItemService.PrepareIndexQuery(_menuItemRepository.GetAll(), indexData);

            var page = await _paginationService.Paginate(query, indexData);

            return Ok(page);
        }

        [HttpGet("{id:long}")]
        [Authorize(Roles = "RestaurantOwner,Customer")]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<MenuItem>> GetMenuItem(long id)
        {
            var menuItem = await _menuItemRepository.GetById(id);

            if (menuItem == null)
            {
                return NotFound();
            }

            return menuItem;
        }

        [HttpPut("{id:long}")]
        [Authorize(Roles = "RestaurantOwner")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<MenuItem>> UpdateMenuItem(long id, MenuItemDTO menuItemDto)
        {
            if (!(await _menuItemRepository.MenuItemExists(id)))
            {
                return NotFound();
            }

            var menuItem = new MenuItem
            {
                Id = id,
                Name = menuItemDto.Name,
                Description = menuItemDto.Description,
                Price = menuItemDto.Price
            };

            _menuItemRepository.UpdateMenuItem(menuItem);

            await _menuItemRepository.Commit();

            return Ok(menuItem);
        }

        [HttpPost]
        [Authorize(Roles = "RestaurantOwner")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult<MenuItem>> CreateMenuItem(MenuItemDTO menuItemDto)
        {
            var menuItem = new MenuItem
            {
                Name = menuItemDto.Name,
                Description = menuItemDto.Description,
                Price = menuItemDto.Price
            };

            _menuItemRepository.Add(menuItem);
            await _menuItemRepository.Commit();

            return CreatedAtAction(nameof(GetMenuItem), new { id = menuItem.Id }, menuItem);
        }

        [HttpDelete("{id:long}")]
        [Authorize(Roles = "RestaurantOwner")]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DeleteMenuItem(long id)
        {
            var menuItem = await _menuItemRepository.GetById(id);
            if (menuItem == null)
            {
                return NotFound();
            }

            _menuItemRepository.Delete(menuItem);
            await _menuItemRepository.Commit();

            return NoContent();
        }
    }
}
