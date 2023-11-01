using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurant_Orders.Extensions;
using Restaurant_Orders.Services;
using RestaurantOrder.Core.DTOs;
using RestaurantOrder.Core.Exceptions;
using System.Net.Mime;

namespace Restaurant_Orders.Controllers
{
    [ApiController]
    [Route("api/menu-items")]
    public class MenuItemsController : ControllerBase
    {
        private readonly IMenuItemService _menuItemService;

        public MenuItemsController(IMenuItemService menuItemService)
        {
            _menuItemService = menuItemService;
        }

        [HttpGet]
        [Authorize(Roles = "RestaurantOwner,Customer")]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<ActionResult<PagedData<MenuItemDTO>>> GetMenuItems([FromQuery] IndexingDTO indexData)
        {
            if (indexData.SortBy != null && !typeof(MenuItemDTO).FieldExists(indexData.SortBy))
            {
                ModelState.AddModelError(nameof(indexData.SortBy), $"Can't find the provided sort property: '{indexData.SortBy}'");
                return ValidationProblem();
            }

            var page = await _menuItemService.Get(indexData);

            return Ok(page);
        }

        [HttpGet("{id:long}")]
        [Authorize(Roles = "RestaurantOwner,Customer")]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<MenuItemDTO>> GetMenuItem(long id)
        {
            try
            {
                var menuItem = await _menuItemService.GetById(id);
                return Ok(menuItem);
            }
            catch (MenuItemDoesNotExists)
            {
                return NotFound();
            }
        }

        [HttpPut("{id:long}")]
        [Authorize(Roles = "RestaurantOwner")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<MenuItemDTO>> UpdateMenuItem(long id, MenuItemUpdateDTO menuItemDto)
        {
            try
            {
                MenuItemDTO menuItem = await _menuItemService.Update(new MenuItemDTO
                {
                    Id = id,
                    Name = menuItemDto.Name,
                    Description = menuItemDto.Description,
                    Price = menuItemDto.Price
                });

                return Ok(menuItem);
            }
            catch (Exception ex) when (ex is MenuItemDoesNotExists ||
                                       ex is DbUpdateConcurrencyException)
            {
                return NotFound();
            }
        }

        [HttpPost]
        [Authorize(Roles = "RestaurantOwner")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult<MenuItemDTO>> CreateMenuItem(MenuItemUpdateDTO menuItemDto)
        {
            var menuItem = new MenuItemDTO
            {
                Name = menuItemDto.Name,
                Description = menuItemDto.Description,
                Price = menuItemDto.Price
            };

            menuItem = await _menuItemService.Create(menuItem);

            return CreatedAtAction(nameof(GetMenuItem), new { id = menuItem.Id }, menuItem);
        }

        [HttpDelete("{id:long}")]
        [Authorize(Roles = "RestaurantOwner")]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DeleteMenuItem(long id)
        {
            try
            {
                await _menuItemService.Delete(id);

                return NoContent();
            }
            catch (Exception ex) when (ex is MenuItemDoesNotExists ||
                                       ex is DbUpdateConcurrencyException)
            {
                return NotFound();
            }
        }
    }
}
