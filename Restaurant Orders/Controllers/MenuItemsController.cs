using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurant_Orders.Data;
using Restaurant_Orders.Data.Entities;
using Restaurant_Orders.Models.DTOs;

namespace Restaurant_Orders.Controllers
{
    [ApiController]
    [Route("api/menu-items")]
    public class MenuItemsController : ControllerBase
    {
        private readonly RestaurantContext _context;

        public MenuItemsController(RestaurantContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = "RestaurantOwner,Customer")]
        public async Task<ActionResult<IEnumerable<MenuItem>>> GetMenuItems()
        {
            if (_context.MenuItems == null)
            {
                return NotFound();
            }

            return await _context.MenuItems.ToListAsync();
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
