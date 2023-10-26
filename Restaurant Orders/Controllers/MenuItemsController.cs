using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurant_Orders.Data;
using Restaurant_Orders.Extensions;
using Restaurant_Orders.Models;
using Restaurant_Orders.Models.DTOs;
using Restaurant_Orders.Services;

namespace Restaurant_Orders.Controllers
{
    [ApiController]
    [Route("api/menu-items")]
    public class MenuItemsController : ControllerBase
    {
        private readonly RestaurantContext _context;
        private readonly IMenuItemService _menuItemService;
        private readonly IPaginationService<MenuItem> _paginationService;

        public MenuItemsController(RestaurantContext context, IMenuItemService menuItemService, IPaginationService<MenuItem> paginationService)
        {
            _context = context;
            _menuItemService = menuItemService;
            _paginationService = paginationService;
        }

        [HttpGet]
        [Authorize(Roles = "RestaurantOwner,Customer")]
        public async Task<ActionResult<PagedData<MenuItem>>> GetMenuItems([FromQuery] IndexingDTO indexData)
        {
            if (indexData.SortBy != null && !typeof(MenuItem).FieldExists(indexData.SortBy))
            {
                ModelState.AddModelError(nameof(indexData.SortBy), "Can't find the provided sort property.");
                return ValidationProblem();
            }

            var query = _menuItemService.PrepareIndexQuery(_context.MenuItems.Where(item => true), indexData);

            var page = await _paginationService.Paginate(query, indexData);

            return Ok(page);
        }

        [HttpGet("{id:long}")]
        [Authorize(Roles = "RestaurantOwner,Customer")]
        public async Task<ActionResult<MenuItem>> GetMenuItem(long id)
        {
            var menuItem = await _context.MenuItems.FindAsync(id);

            if (menuItem == null)
            {
                return NotFound();
            }

            return menuItem;
        }

        [HttpPut("{id:long}")]
        [Authorize(Roles = "RestaurantOwner")]
        public async Task<IActionResult> UpdateMenuItem(long id, MenuItemDTO menuItemDto)
        {
            var menuItem = new MenuItem
            {
                Id = id,
                Name = menuItemDto.Name,
                Description = menuItemDto.Description,
                Price = menuItemDto.Price
            };

            _context.MenuItems.Attach(menuItem);
            _context.Entry(menuItem).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MenuItemExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Ok(menuItem);
        }

        [HttpPost]
        [Authorize(Roles = "RestaurantOwner")]
        public async Task<ActionResult<MenuItem>> CreateMenuItem(MenuItemDTO menuItemDto)
        {
            var menuItem = new MenuItem
            {
                Name = menuItemDto.Name,
                Description = menuItemDto.Description,
                Price = menuItemDto.Price
            };

            _context.MenuItems.Add(menuItem);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMenuItem), new { id = menuItem.Id }, menuItem);
        }

        [HttpDelete("{id:long}")]
        [Authorize(Roles = "RestaurantOwner")]
        public async Task<IActionResult> DeleteMenuItem(long id)
        {
            var menuItem = await _context.MenuItems.FindAsync(id);
            if (menuItem == null)
            {
                return NotFound();
            }

            _context.MenuItems.Remove(menuItem);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool MenuItemExists(long id)
        {
            return (_context.MenuItems?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
